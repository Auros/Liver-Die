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
    }
}
