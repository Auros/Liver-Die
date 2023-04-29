using System;
using LiverDie.Hospital.Generation;
using UnityEngine;

namespace LiverDie
{
    public class HospitalGenerator : MonoBehaviour
    {
        private enum SegmentDirection
        {
            North,
            East,
            South,
            West
        }

        [SerializeField]
        private Transform _generationZeroStartReference = null!;

        [SerializeField]
        private AnimationCurve _branchProbabilityCurve = new();

        [SerializeField]
        private CorridorSegmentDefinition _segmentPrefab = null!;

        [Min(0.01f), SerializeField]
        private float _segmentLength = 5f;

        [SerializeField]
        private int _maximumSegmentDepth = 50;

        private void Start()
        {
            const int generationsToMake = 25;
            var lastSegmentDirection = SegmentDirection.North;
            var lastSegmentPosition = _generationZeroStartReference.localPosition;

            for (int i = 0; i < generationsToMake; i++)
            {
                lastSegmentDirection = i is 0 ? SegmentDirection.North : GetNewRandomDirection(lastSegmentDirection);

                int segmentDepth = 0;
                bool shouldBranch = false;

                while (!shouldBranch && _maximumSegmentDepth >= segmentDepth)
                {
                    var possibility = _branchProbabilityCurve.Evaluate(segmentDepth);
                    var randomValue = UnityEngine.Random.Range(0f, 1f);

                    if (possibility >= randomValue)
                    {
                        shouldBranch = true;
                        continue;
                    }

                    var segment = Instantiate(_segmentPrefab);
                    var targetPos = GetSegmentOffset(lastSegmentPosition, lastSegmentDirection);

                    segmentDepth++;
                    lastSegmentPosition = targetPos;
                    segment.transform.SetLocalPositionAndRotation(targetPos, Quaternion.identity);
                    segment.Generation = i;
                }
            }
        }

        private void Update()
        {
            _ = _segmentPrefab;
            _ = _generationZeroStartReference;
            _ = _branchProbabilityCurve;
            _ = _segmentLength;
        }

        private Vector3 GetSegmentOffset(Vector3 segmentPosition, SegmentDirection segmentDirection)
        {
            return segmentPosition + _segmentLength * segmentDirection switch
            {
                SegmentDirection.North => Vector3.forward,
                SegmentDirection.East => Vector3.right,
                SegmentDirection.South => Vector3.back,
                SegmentDirection.West => Vector3.left,
                _ => throw new ArgumentOutOfRangeException(nameof(segmentDirection), segmentDirection, null)
            };
        }

        private static SegmentDirection GetNewRandomDirection(SegmentDirection oldDirection)
        {
            var newDirection = oldDirection;
            var oppositeDirection = GetOppositeDirection(oldDirection);
            while (newDirection == oldDirection || newDirection == oppositeDirection)
                newDirection = (SegmentDirection)UnityEngine.Random.Range(0, 4);
            return newDirection;
        }

        private static SegmentDirection GetOppositeDirection(SegmentDirection direction)
        {
            return (SegmentDirection)((int)(direction + 2) % 4);
        }
    }
}
