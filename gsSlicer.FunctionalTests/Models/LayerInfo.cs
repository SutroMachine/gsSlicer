using gs;
using System.Collections.Generic;

namespace gsCore.FunctionalTests.Models
{
    public class LayerInfo<TFeatureInfo> where TFeatureInfo : IFeatureInfo
    {
        private readonly Dictionary<string, TFeatureInfo> perFeatureInfo =
            new Dictionary<string, TFeatureInfo>();

        public void AssertEqualsExpected(LayerInfo<TFeatureInfo> expected)
        {
            foreach (var key in perFeatureInfo.Keys)
                if (!expected.perFeatureInfo.ContainsKey(key))
                    throw new MissingFeature($"Result has unexpected feature {key}");

            foreach (var key in expected.perFeatureInfo.Keys)
                if (!perFeatureInfo.ContainsKey(key))
                    throw new MissingFeature($"Result was missing expected feature {key}");

            foreach (var fillType in perFeatureInfo.Keys)
            {
                perFeatureInfo[fillType].AssertEqualsExpected(expected.perFeatureInfo[fillType]);
            }
        }

        public bool GetFeatureInfo(string fillType, out TFeatureInfo featureInfo)
        {
            return perFeatureInfo.TryGetValue(fillType, out featureInfo);
        }

        public void AddFeatureInfo(TFeatureInfo featureInfo)
        {
            if (featureInfo == null)
                return;

            if (GetFeatureInfo(featureInfo.FillType, out var existingFeatureInfo))
            {
                existingFeatureInfo.Add(featureInfo);
            }
            else
            {
                perFeatureInfo.Add(featureInfo.FillType, featureInfo);
            }
        }
    }
}