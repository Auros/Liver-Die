using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LiverDie.Gremlin;
using LiverDie.Hospital.Data;
using LiverDie.Hospital.Generation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace LiverDie
{
    public class HospitalGenerator : MonoBehaviour, CorridorSegmentDefinition.IPlayerEnterListener, CorridorSegmentDefinition.IPlayerExitListener
    {
        [Min(0.01f), SerializeField]
        private float _segmentLength = 5f;

        [FormerlySerializedAs("_maximumSegmentDepth")]
        [SerializeField]
        private int _maximumSegmentCount = 50;

        [SerializeField]
        private SegmentDirection _startDirection;

        [SerializeField]
        private CorridorSegmentDefinition _segmentPrefab = null!;

        [SerializeField]
        private MaterialSwappingController _swappingController = null!;

        [SerializeField]
        private AnimationCurve _branchProbabilityCurve = new();

        [SerializeField]
        private Transform _generationZeroStartReference = null!;

        [SerializeField]
        private RoomSpawnInfo[] _roomSpawningOptions = Array.Empty<RoomSpawnInfo>();

        [SerializeField]
        private float _defaultRoomSpawnProbability = 0.5f;

        [SerializeField]
        private int _initialCorridorSegmentPoolSize;

        [SerializeField]
        private int _initialRoomPoolSizePerRoom;

        [SerializeField]
        private UnityEvent<int> _onCorridorStartup = new();

        [SerializeField]
        private UnityEvent<int> _onCorridorShutdown = new();

        private CorridorState? _nextCorridor;
        private CorridorState? _currentCorridor;
        private CorridorState? _previousCorridor;
        private IObjectPool<CorridorSegmentDefinition> _segmentPool = null!;
        private IDictionary<RoomScriptableObject, IObjectPool<RoomDefinition>> _roomPools = null!;

        private const int _sampleFrequency = 10;

        private static bool _first = true;

        private void Awake()
        {
            _segmentPool = new ObjectPool<CorridorSegmentDefinition>(
                () => Instantiate(_segmentPrefab, transform),
                segment =>
                {
                    segment.AddEnterListener(this);
                    segment.AddExitListener(this);
                    segment.gameObject.SetActive(true);
                },
                segment =>
                {
                    segment.RemoveExitListener(this);
                    segment.RemoveEnterListener(this);
                    segment.gameObject.SetActive(false);
                },
                Destroy,
                defaultCapacity: 250
            );

            using (ListPool<CorridorSegmentDefinition>.Get(out var prespawned))
            {
                for (int i = 0; i < _initialCorridorSegmentPoolSize; i++)
                    prespawned.Add(_segmentPool.Get());
                prespawned.ForEach(_segmentPool.Release);
            }

            // Setup the object pools for the rooms at the start.
            _roomPools = new Dictionary<RoomScriptableObject, IObjectPool<RoomDefinition>>(_roomSpawningOptions.Length);
            foreach (var option in _roomSpawningOptions)
            {
                var room = option.Room;
                if (_roomPools.TryGetValue(room, out var pool))
                    continue;

                pool = new ObjectPool<RoomDefinition>(
                    () => Instantiate(room.Prefab, transform),
                    definition => definition.gameObject.SetActive(true),
                    definition => definition.gameObject.SetActive(false),
                    Destroy
                );

                using (ListPool<RoomDefinition>.Get(out var prespawned))
                {
                    for (int i = 0; i < _initialRoomPoolSizePerRoom; i++)
                        prespawned.Add(pool.Get());

                    prespawned.ForEach(pool.Release);
                }

                _roomPools[room] = pool;
            }

        }

        private void Start()
        {
            AdvanceCorridor();
        }

        private void OnDestroy()
        {
            _segmentPool.Clear();
            _previousCorridor?.Dispose();
            _currentCorridor?.Dispose();
            _nextCorridor?.Dispose();
        }

        /// <summary>
        /// Advance the hospital to the next corridor.
        /// </summary>
        /// <param name="roomSpawnProbability">The probability (0.0-1.0) that a room at a given point will spawn.</param>
        [PublicAPI]
        public void AdvanceCorridor(float? roomSpawnProbability = null)
        {
            roomSpawnProbability ??= _defaultRoomSpawnProbability;

            // Despawn the old corridor
            if (_previousCorridor is not null)
            {
                var segments = _previousCorridor.CorridorSegments;
                // ReSharper disable once ForCanBeConvertedToForeach [Zero-Alloc]
                for (int i = 0; i < segments.Count; i++)
                    _segmentPool.Release(segments[i]);

                var rooms = _previousCorridor.Rooms;
                // ReSharper disable once ForCanBeConvertedToForeach [Zero-Alloc]
                for (int i = 0; i < rooms.Count; i++)
                    _roomPools[rooms[i].Template].Release(rooms[i]);

                _onCorridorShutdown.Invoke(_previousCorridor.Generation);
                _previousCorridor.Dispose();
            }

            // Close off the old corridor
            if (_currentCorridor is not null)
            {
                var segments = _currentCorridor.CorridorSegments;
                // ReSharper disable once ForCanBeConvertedToForeach [Zero-Alloc]
                for (int i = 0; i < segments.Count; i++)
                {
                    var segment = segments[i];
                    if (!segment.IsStart)
                        continue;

                    var direction = _currentCorridor.Direction;
                    segment.SetWalls(direction, direction, true, true);
                    break;
                }
            }

            _previousCorridor = _currentCorridor;
            _currentCorridor = _nextCorridor;

            if (_currentCorridor is null)
            {
                var startPos = _generationZeroStartReference.localPosition;
                _currentCorridor = GenerateCorridor(0, startPos, null, roomSpawnProbability.Value);
                var direciion = _currentCorridor.Direction;
                if (_currentCorridor.CorridorSegments.Count is not 0)
                    _currentCorridor.CorridorSegments[0].SetWalls(direciion, direciion, true, false, true);
            }

            _nextCorridor = GenerateCorridor(_currentCorridor.Generation + 1, _currentCorridor.End, _currentCorridor, roomSpawnProbability.Value);
        }

        private CorridorState GenerateCorridor(int generation, Vector3 startPosition, CorridorState? previousCorridor, float roomSpawnProbability)
        {
            _onCorridorStartup.Invoke(generation);

            var segmentPosition = startPosition;
            var oldSegmentDirection = previousCorridor?.Direction ?? _startDirection;
            var targetDirection = previousCorridor is not null ? GetNewRandomDirection(oldSegmentDirection) : _startDirection;

            int segmentCount = 0;
            bool shouldBranch = false;

            List<RoomDefinition> rooms = new();
            List<CorridorSegmentDefinition> corridorSegments = new();

            while (!shouldBranch && _maximumSegmentCount >= segmentCount)
            {
                // Check the probability curve to decide if and when we want to change directions and create a new corridor.
                var possibility = _branchProbabilityCurve.Evaluate(segmentCount);
                var randomValue = UnityEngine.Random.Range(0f, 1f);

                if (possibility >= randomValue)
                {
                    shouldBranch = true;
                    continue;
                }

                // Spawn a corridor segment, position it, and setup the proper walls for the current direction.
                var segment = _segmentPool.Get();
                var targetPos = GetSegmentOffset(segmentPosition, segmentCount is not 0 ? targetDirection : oldSegmentDirection);
                segment.SetWalls(targetDirection, oldSegmentDirection, segmentCount is 0);
                corridorSegments.Add(segment);

                segment.transform.SetLocalPositionAndRotation(targetPos, Quaternion.identity);
                segment.Position = segmentCount * _segmentLength;
                segment.Generation = generation;
                segmentPosition = targetPos;
                segmentCount++;
            }

            if (corridorSegments.Count > 0)
                for (int i = 0; i < segmentCount; i++)
                    _swappingController.SetColorOfCorridorSegment(corridorSegments[i], i / (segmentCount - 1f));

            if (corridorSegments.Count > 1)
                corridorSegments[^1].IsEnd = true;

            // Calculate the length of each rail for this segment.
            var railLength = _segmentLength * corridorSegments.Count;

            // Create the state
            CorridorState state = new(startPosition, segmentPosition, generation, targetDirection, rooms, corridorSegments, railLength, _sampleFrequency);

            // ReSharper disable once InvertIf
            if (_roomSpawningOptions.Length is not 0)
            {
                // To compensate for the corner bending, we pad the rail that
                // intersects with the inner corner of the previous corridor.
                var path = oldSegmentDirection.PathWith(targetDirection);
                var paddedRail = path switch
                {
                    SegmentPath.Left => state.LeftRail,
                    SegmentPath.Right => state.RightRail,
                    SegmentPath.Forward => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(path))
                };

                if (paddedRail is not null)
                    paddedRail.Lower = _segmentLength;

                state.LeftRail.Upper = railLength;
                state.RightRail.Upper = railLength;

                // Build a weighted list of the potential room options.
                var options = ListPool<RoomScriptableObject>.Get();
                float weightMin = _roomSpawningOptions[0].Weight;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var option in _roomSpawningOptions)
                    if (weightMin > option.Weight && option.Weight > 0)
                        weightMin = option.Weight;

                foreach (var option in _roomSpawningOptions)
                {
                    // Skip things with no weights
                    if (0 >= option.Weight)
                        continue;

                    var representation = weightMin * option.Weight;
                    for (int i = 0; i < representation; i++)
                        options.Add(option.Room);
                }

                // Generate the rooms!
                var (leftAdjacant, rightAdjacant) = targetDirection.GetAdjacant();
                GenerateRoomsOnRail(state.LeftRail, false, generation, path, rightAdjacant, path is SegmentPath.Left ? previousCorridor : null, roomSpawnProbability, corridorSegments, rooms, options);
                GenerateRoomsOnRail(state.RightRail, true, generation, path, leftAdjacant, path is SegmentPath.Right ? previousCorridor : null, roomSpawnProbability, corridorSegments, rooms, options);

                ListPool<RoomScriptableObject>.Release(options);
            }

            return state;
        }

        private Vector3 GetSegmentOffset(Vector3 segmentPosition, SegmentDirection segmentDirection)
        {
            return segmentPosition + _segmentLength * segmentDirection.ToNormalizedVector();
        }

        private void GenerateRoomsOnRail(Rail rail, bool isRight, int generation, SegmentPath path, SegmentDirection adjacancy, CorridorState? previousCorridor, float roomSpawnProbability,
            IReadOnlyList<CorridorSegmentDefinition> corridorSegments, ICollection<RoomDefinition> definitions, IReadOnlyList<RoomScriptableObject> options)
        {
            int attempts = 0;
            const int maxRoomCount = 100; // Just in case something goes horribly wrong, always have exit conditions for while loops.
            while (GetRandomValidRoom(rail, options) is { } roomSo && maxRoomCount >= attempts)
            {
                attempts++;

                bool doNotSpawn = false;
                var startRailPos = rail.Lower;
                var depth = roomSo.Prefab.Depth;
                var length = roomSo.Prefab.Length;
                var padding = length % _segmentLength;

                // Collision check around inner corners to prevent rooms from colliding.
                if (previousCorridor is not null)
                {
                    var depthRail = path is SegmentPath.Left ? previousCorridor.LeftRail : previousCorridor.RightRail;
                    var start = previousCorridor.Length - depthRail.SampleDistance;
                    var end = previousCorridor.Length - depth;

                    for (float i = start; i >= end; i -= depthRail.SampleDistance)
                    {
                        var depthAt = depthRail.Evaluate(i);
                        if (startRailPos > depthAt)
                            continue;

                        doNotSpawn = true;
                        break;
                    }
                }

                if (doNotSpawn || UnityEngine.Random.value > roomSpawnProbability)
                {
                    rail.Lower += padding + length;
                    continue;
                }

                var pool = _roomPools[roomSo];
                var definition = pool.Get();
                definition.name = $"Room [Gen {generation}] ({roomSo.RoomName})";
                length = definition.Length;
                depth = definition.Depth;

                definition.Template = roomSo;
                definitions.Add(definition);

                var entranceOffset = definition.EntranceOffsetLength;
                if (isRight)
                    entranceOffset = length - entranceOffset;

                var railDoorLocation = entranceOffset + rail.Lower;

                rail.Lower += length + padding;
                var corridorIndex = (int)(railDoorLocation / _segmentLength);

                var segment = corridorSegments[corridorIndex];
                segment.SetToDoor(adjacancy);

                definition.MoveTo(segment.GetEntranceLocation(adjacancy));
                _swappingController.SetColorOfRoom(definition, corridorIndex / (corridorSegments.Count - 1f));

                for (float i = 0; i < length; i += rail.SampleDistance)
                    rail.AddDepthSample(i + startRailPos, depth);
            }
        }

        private static RoomScriptableObject? GetRandomValidRoom(Rail rail, IReadOnlyList<RoomScriptableObject> options)
        {
            // Maximum valid length of the rail
            var depth = rail.Upper - rail.Lower;

            var validChoices = ListPool<RoomScriptableObject>.Get();
            // ReSharper disable once LoopCanBeConvertedToQuery [Zero-Alloc]
            // ReSharper disable once ForCanBeConvertedToForeach [Zero-Alloc]
            for (int i = 0; i < options.Count; i++)
                if (depth >= options[i].Prefab.Length)
                    validChoices.Add(options[i]);

            if (validChoices.Count is 0)
                return null;

            //temporarily always spawn the latest room first for testing
            var randomIndex = _first ? validChoices.Count - 1 : UnityEngine.Random.Range(0, validChoices.Count);
            if (_first) _first = false;
            var target = validChoices[randomIndex];
            ListPool<RoomScriptableObject>.Release(validChoices);

            return target;
        }

        public void Entered(GremlinController player, CorridorSegmentDefinition definition)
        {
            if (_nextCorridor is null || _nextCorridor.Generation != definition.Generation || !definition.IsStart)
                return;

            _swappingController.Unlock();
            AdvanceCorridor();
        }

        public void Exited(GremlinController player, CorridorSegmentDefinition definition)
        {
        }

        private static SegmentDirection GetNewRandomDirection(SegmentDirection oldDirection)
        {
            var (a, b) = oldDirection.GetAdjacant();
            return UnityEngine.Random.Range(0, 2) == 0 ? a : b;
        }
    }
}
