using System;

namespace gsCore.FunctionalTests.Models
{
    public class FeatureCumulativeExtrusionMismatch : Exception
    {
        public FeatureCumulativeExtrusionMismatch(string s) : base(s)
        {
        }
    }

    public class FeatureCumulativeDistanceMismatch : Exception
    {
        public FeatureCumulativeDistanceMismatch(string s) : base(s)
        {
        }
    }

    public class FeatureCumulativeDurationMismatch : Exception
    {
        public FeatureCumulativeDurationMismatch(string s) : base(s)
        {
        }
    }

    public class FeatureCenterOfMassMismatch : Exception
    {
        public FeatureCenterOfMassMismatch(string s) : base(s)
        {
        }
    }

    public class FeatureBoundingBoxMismatch : Exception
    {
        public FeatureBoundingBoxMismatch(string s) : base(s)
        {
        }
    }

    public class MissingFeature : Exception
    {
        public MissingFeature(string s) : base(s)
        {
        }
    }

    public class LayerCountMismatch : Exception
    {
        public LayerCountMismatch(string s) : base(s)
        {
        }
    }
}