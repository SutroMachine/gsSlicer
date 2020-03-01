using g3;

namespace gs
{
    public class TemporalPathHash
    {
        public double HashBucketSize = 5.0;

        private DVector<Segment2d> Segments;
        private DVector<int> Times;

        private SegmentHashGrid2d<int> Hash;

        public TemporalPathHash()
        {
            Segments = new DVector<Segment2d>();
            Times = new DVector<int>();

            Hash = new SegmentHashGrid2d<int>(HashBucketSize, -1);
        }

        public void AppendSegment(Vector2d p0, Vector2d p1)
        {
            // todo
        }
    }
}