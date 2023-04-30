using System;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    public static class SegmentDirectionExtensions
    {
        public static SegmentDirection GetOpposite(this SegmentDirection direction)
        {
            return (SegmentDirection)((int)(direction + 2) % 4);
        }

        public static (SegmentDirection, SegmentDirection) GetAdjacant(this SegmentDirection direction)
        {
            var adjacant1 = (SegmentDirection)((int)(direction + 1) % 4);
            var adjacant2 = (SegmentDirection)((int)(direction + 3) % 4);
            return (adjacant1, adjacant2);
        }

        public static SegmentDirection InnerCornerWith(this SegmentDirection direction, SegmentDirection path)
        {
            return (direction, path) switch
            {
                (SegmentDirection.North, SegmentDirection.East) => SegmentDirection.South,
                (SegmentDirection.East, SegmentDirection.North) => SegmentDirection.North,
                (SegmentDirection.North, SegmentDirection.West) => SegmentDirection.West,
                (SegmentDirection.West, SegmentDirection.North) => SegmentDirection.East,
                (SegmentDirection.South, SegmentDirection.East) => SegmentDirection.East,
                (SegmentDirection.East, SegmentDirection.South) => SegmentDirection.West,
                (SegmentDirection.South, SegmentDirection.West) => SegmentDirection.North,
                (SegmentDirection.West, SegmentDirection.South) => SegmentDirection.South,
                _ => direction
            };
        }

        public static SegmentPath PathWith(this SegmentDirection direction, SegmentDirection path)
        {
            return (direction, path) switch
            {
                (SegmentDirection.North, SegmentDirection.East) => SegmentPath.Right,
                (SegmentDirection.East, SegmentDirection.North) => SegmentPath.Left,
                (SegmentDirection.North, SegmentDirection.West) => SegmentPath.Left,
                (SegmentDirection.West, SegmentDirection.North) => SegmentPath.Right,
                (SegmentDirection.South, SegmentDirection.East) => SegmentPath.Left,
                (SegmentDirection.East, SegmentDirection.South) => SegmentPath.Right,
                (SegmentDirection.South, SegmentDirection.West) => SegmentPath.Right,
                (SegmentDirection.West, SegmentDirection.South) => SegmentPath.Left,
                _ => SegmentPath.Forward
            };
        }


        public static Vector3 ToNormalizedVector(this SegmentDirection direction)
        {
            return direction switch
            {
                SegmentDirection.North => Vector3.forward,
                SegmentDirection.East => Vector3.right,
                SegmentDirection.South => Vector3.back,
                SegmentDirection.West => Vector3.left,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static SegmentDirection LocalHorizontalCardinal(this Transform transform)
        {
            var degree = transform.localEulerAngles.y % 360;
            return degree switch
            {
                >= -45f and < 45f or >= 315f => SegmentDirection.North,
                >= 45f and < 135f => SegmentDirection.East,
                >= 135f and < 225f => SegmentDirection.South,
                >= 225f and < 315f => SegmentDirection.West,
                _ => throw new ArgumentOutOfRangeException($"Invalid degree {degree}")
            };
        }
    }
}
