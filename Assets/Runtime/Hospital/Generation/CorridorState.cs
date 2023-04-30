using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace LiverDie.Hospital.Generation
{
    [PublicAPI]
    public class CorridorState : IDisposable
    {
        public Vector3 Start { get; }

        public Vector3 End { get; }

        public int Generation { get; }

        public SegmentDirection Direction { get; }

        public Rail LeftRail { get; }

        public Rail RightRail { get; }

        public float Length { get; }

        public IReadOnlyList<RoomDefinition> Rooms => _rooms;

        public IReadOnlyList<CorridorSegmentDefinition> CorridorSegments => _corridorSegments;

        private readonly List<RoomDefinition> _rooms;
        private readonly List<CorridorSegmentDefinition> _corridorSegments;

        public CorridorState(
            Vector3 start, Vector3 end, int generation, SegmentDirection direction,
            List<RoomDefinition> rooms, List<CorridorSegmentDefinition> segments,
            float length, int sampleFrequency)
        {
            End = end;
            Start = start;
            Length = length;
            Direction = direction;
            Generation = generation;

            _rooms = rooms;
            _corridorSegments = segments;

            var sampleSize = (int)(length * sampleFrequency);
            LeftRail = new Rail(sampleSize, sampleFrequency);
            RightRail = new Rail(sampleSize, sampleFrequency);
        }

        public void Dispose()
        {
            _rooms.Clear();
            LeftRail.Dispose();
            RightRail.Dispose();
            _corridorSegments.Clear();
        }
    }
}
