using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LiverDie.Hospital.Data;
using LiverDie.Hospital.Generation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace LiverDie
{
    public class HospitalGenerator : MonoBehaviour
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
        private AnimationCurve _branchProbabilityCurve = new();

        [SerializeField]
        private Transform _generationZeroStartReference = null!;

        [SerializeField]
        private RoomSpawnInfo[] _roomSpawningOptions = Array.Empty<RoomSpawnInfo>();

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
                segment => segment.gameObject.SetActive(true),
                segment => segment.gameObject.SetActive(false),
                Destroy,
                defaultCapacity: 250
            );

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
        }

        [PublicAPI]
        public void AdvanceCorridor()
        {
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
                _currentCorridor = GenerateCorridor(0, startPos, null);
            }

            _nextCorridor = GenerateCorridor(_currentCorridor.Generation + 1, _currentCorridor.End, _currentCorridor);
        }

        private CorridorState GenerateCorridor(int generation, Vector3 startPosition, CorridorState? previousCorridor)
        {
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
                segment.Generation = generation;
                segmentPosition = targetPos;
                segmentCount++;
            }


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
                foreach (var option in _roomSpawningOptions)
                    for (int i = 0; i < option.Weight; i++)
                        options.Add(option.Room);

                // Generate the rooms!
                var (leftAdjacant, rightAdjacant) = targetDirection.GetAdjacant();
                GenerateRoomsOnRail(state.LeftRail, false, generation, path, rightAdjacant, path is SegmentPath.Left ? previousCorridor : null, corridorSegments, rooms, options);
                GenerateRoomsOnRail(state.RightRail, true, generation, path, leftAdjacant, path is SegmentPath.Right ? previousCorridor : null, corridorSegments, rooms, options);

                ListPool<RoomScriptableObject>.Release(options);
            }

            return state;
        }

        private Vector3 GetSegmentOffset(Vector3 segmentPosition, SegmentDirection segmentDirection)
        {
            return segmentPosition + _segmentLength * segmentDirection.ToNormalizedVector();
        }

        private void GenerateRoomsOnRail(Rail rail, bool isRight, int generation, SegmentPath path, SegmentDirection adjacancy, CorridorState? previousCorridor,
            IReadOnlyList<CorridorSegmentDefinition> corridorSegments, ICollection<RoomDefinition> definitions, IReadOnlyList<RoomScriptableObject> options)
        {
            int attempts = 0;
            const int maxRoomCount = 100; // Just in case something goes horribly wrong, always have exit conditions for while loops.
            while (GetRandomValidRoom(rail, options) is { } roomSo && maxRoomCount >= attempts)
            {
                var startRailPos = rail.Lower;

                // Collision check around inner corners to prevent rooms from colliding.
                if (previousCorridor is not null)
                {
                    var depthRail = path is SegmentPath.Left ? previousCorridor.LeftRail : previousCorridor.RightRail;
                    var start = previousCorridor.Length - depthRail.SampleDistance;
                    var end = previousCorridor.Length - roomSo.Prefab.Depth;

                    bool failedDepthCheck = false;
                    for (float i = start; i >= end; i -= depthRail.SampleDistance)
                    {
                        var depthAt = depthRail.Evaluate(i);
                        if (startRailPos > depthAt)
                            continue;

                        failedDepthCheck = true;
                        break;
                    }

                    if (failedDepthCheck)
                    {
                        rail.Lower += roomSo.Prefab.Length;
                        continue;
                    }
                }

                attempts++;
                var pool = _roomPools[roomSo];
                var definition = pool.Get();
                definition.name = $"Room [Gen {generation}] ({roomSo.RoomName})";

                definition.Template = roomSo;
                definitions.Add(definition);

                var entranceOffset = definition.EntranceOffsetLength;
                if (isRight)
                    entranceOffset = definition.Length - entranceOffset;

                var railDoorLocation = entranceOffset + rail.Lower;

                rail.Lower += definition.Length;
                var corridorIndex = (int)(railDoorLocation / _segmentLength);

                var segment = corridorSegments[corridorIndex];
                segment.SetToDoor(adjacancy);

                definition.MoveTo(segment.GetEntranceLocation(adjacancy));

                for (float i = 0; i < definition.Size.x; i += rail.SampleDistance)
                    rail.AddDepthSample(i + startRailPos, definition.Size.z);
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

#if UNITY_EDITOR

        [SerializeField]
        private bool _advance;

        private void OnValidate()
        {
            if (!_advance || !Application.isPlaying)
                return;

            void ValidationSync()
            {
                EditorApplication.delayCall -= ValidationSync;
                AdvanceCorridor();
            }
            EditorApplication.delayCall += ValidationSync;
            _advance = false;
        }

#endif

        private static SegmentDirection GetNewRandomDirection(SegmentDirection oldDirection)
        {
            var (a, b) = oldDirection.GetAdjacant();
            return UnityEngine.Random.Range(0, 2) == 0 ? a : b;
        }
    }
}
