using g3;
using System;

namespace gs
{
    public class AdaptiveRangeCustomDataDetails : CustomDataDetails
    {
        protected Interval1d interval = Interval1d.Empty;
        public override float RangeMin { get => (float)interval.a; }
        public override float RangeMax { get => (float)interval.b; }

        public AdaptiveRangeCustomDataDetails(
            Func<string> labelF, Func<float, string> colorScaleLabelerF)
            : base(labelF, colorScaleLabelerF)
        {
        }

        public void ObserveValue(float value)
        {
            interval.Contain(value);
        }
    }
}