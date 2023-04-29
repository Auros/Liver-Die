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

        [PublicAPI]
        public int Generation
        {
            get => _generation;
            set
            {
                _generation = value;
                name = $"Corridor Gen [{_generation}] {transform.localPosition}";
            }
        }

        public void SetWalls(SegmentDirection direction, SegmentDirection oldDirection, bool isStart)
        {
            var opposite = direction.GetOpposite();
            var (adjacant1, adjacant2) = direction.GetAdjacant();

            GetWallForDirection(direction).State = WallDefinition.WallState.Invisible;
            GetWallForDirection(opposite).State = isStart ? WallDefinition.WallState.Solid : WallDefinition.WallState.Invisible;
            GetWallForDirection(adjacant1).State = isStart && adjacant2 == oldDirection ? WallDefinition.WallState.Invisible : WallDefinition.WallState.Solid;
            GetWallForDirection(adjacant2).State = isStart && adjacant1 == oldDirection ? WallDefinition.WallState.Invisible : WallDefinition.WallState.Solid;

            var targetSpokeDirection = GetSpokeTarget(oldDirection, direction);
            _northSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.North);
            _eastSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.East);
            _southSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.South);
            _westSpoke.SetActive(isStart && targetSpokeDirection is SegmentDirection.West);
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

        private static SegmentDirection GetSpokeTarget(SegmentDirection forward, SegmentDirection bend)
        {
            return (forward, bend) switch
            {
                (SegmentDirection.North, SegmentDirection.East) => SegmentDirection.South,
                (SegmentDirection.East, SegmentDirection.North) => SegmentDirection.North,
                (SegmentDirection.North, SegmentDirection.West) => SegmentDirection.West,
                (SegmentDirection.West, SegmentDirection.North) => SegmentDirection.East,
                (SegmentDirection.South, SegmentDirection.East) => SegmentDirection.East,
                (SegmentDirection.East, SegmentDirection.South) => SegmentDirection.West,
                (SegmentDirection.South, SegmentDirection.West) => SegmentDirection.North,
                (SegmentDirection.West, SegmentDirection.South) => SegmentDirection.South,
                _ => (SegmentDirection)4
            };
        }
    }
}
