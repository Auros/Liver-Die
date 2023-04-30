using System;
using JetBrains.Annotations;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public class CorridorSegmentDefinition : MonoBehaviour
    {
        private int _generation;

        [SerializeField]
        private WallDefinition _northWall = null!;

        [SerializeField]
        private WallDefinition _eastWall = null!;

        [SerializeField]
        private WallDefinition _southWall = null!;

        [SerializeField]
        private WallDefinition _westWall = null!;

        [SerializeField]
        private GameObject _northSpoke = null!;

        [SerializeField]
        private GameObject _eastSpoke = null!;

        [SerializeField]
        private GameObject _southSpoke = null!;

        [SerializeField]
        private GameObject _westSpoke = null!;

        [SerializeField]
        private RailPairDefinition _northRailPair = null!;

        [SerializeField]
        private RailPairDefinition _eastRailPair = null!;

        [SerializeField]
        private RailPairDefinition _southRailPair = null!;

        [SerializeField]
        private RailPairDefinition _westRailPair = null!;

        public bool IsStart { get; private set; }

        public float Depth { get; set; }

        [PublicAPI]
        public int Generation
        {
            get => _generation;
            set
            {
                _generation = value;
                name = $"Corridor [Gen {_generation}] {transform.localPosition}";
            }
        }

        public void SetWalls(SegmentDirection direction, SegmentDirection oldDirection, bool isStart, bool ignoreDoors = false)
        {
            IsStart = isStart;
            var opposite = direction.GetOpposite();
            var (adjacant1, adjacant2) = direction.GetAdjacant();

            var wall1 = GetWallForDirection(direction);
            if (wall1.State is not WallDefinition.WallState.Door || !ignoreDoors)
                wall1.State = WallDefinition.WallState.Invisible;

            var wall2 = GetWallForDirection(opposite);
            if (wall2.State is not WallDefinition.WallState.Door || !ignoreDoors)
                wall2.State = isStart ? WallDefinition.WallState.Solid : WallDefinition.WallState.Invisible;

            var wall3 = GetWallForDirection(adjacant1);
            if (wall3.State is not WallDefinition.WallState.Door || !ignoreDoors)
                wall3.State = isStart && adjacant2 == oldDirection ? WallDefinition.WallState.Invisible : WallDefinition.WallState.Solid;

            var wall4 = GetWallForDirection(adjacant2);
            if (wall4.State is not WallDefinition.WallState.Door || !ignoreDoors)
                wall4.State = isStart && adjacant1 == oldDirection ? WallDefinition.WallState.Invisible : WallDefinition.WallState.Solid;

            var targetSpokeDirection = oldDirection.InnerCornerWith(direction);
            _northSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.North);
            _eastSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.East);
            _southSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.South);
            _westSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.West);
        }

        public RailPairDefinition GetRailPair(SegmentDirection direction)
        {
            return direction switch
            {
                SegmentDirection.North => _northRailPair,
                SegmentDirection.East => _eastRailPair,
                SegmentDirection.South => _southRailPair,
                SegmentDirection.West => _westRailPair,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public void SetToDoor(SegmentDirection direction)
        {
            GetWallForDirection(direction).State = WallDefinition.WallState.Door;
        }

        public Pose GetEntranceLocation(SegmentDirection direction)
        {
            return GetWallForDirection(direction).Entrance.Location;
        }

        private WallDefinition GetWallForDirection(SegmentDirection direction)
        {
            return direction switch
            {
                SegmentDirection.North => _northWall,
                SegmentDirection.East => _eastWall,
                SegmentDirection.South => _southWall,
                SegmentDirection.West => _westWall,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}
