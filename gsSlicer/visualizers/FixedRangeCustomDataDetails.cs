using System;

namespace gs
{
    public class FixedRangeCustomDataDetails : CustomDataDetails
    {
        public FixedRangeCustomDataDetails(
            Func<string> labelF, Func<float, string> colorScaleLabelerF,
            float rangeMin, float rangeMax)
            : base(labelF, colorScaleLabelerF)
        {
            RangeMin = rangeMin;
            RangeMax = rangeMax;
        }
    }
}