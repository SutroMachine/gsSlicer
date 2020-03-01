using gs;

namespace gsCore.FunctionalTests.Models
{
    public interface IFeatureInfo
    {
        string FillType { get; set; }

        void Add(IFeatureInfo other);

        void AssertEqualsExpected(IFeatureInfo other);
    }
}