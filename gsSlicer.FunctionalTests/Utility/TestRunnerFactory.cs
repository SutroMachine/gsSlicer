using gs;
using gsCore.FunctionalTests.Models;
using gsCore.FunctionalTests.Utility;

namespace gsSlicer.FunctionalTests.Utility
{
    public static class TestRunnerFactoryFFF
    {
        public static PrintTestRunner CreateTestRunner(string caseName, SingleMaterialFFFSettings settings)
        {
            var resultGenerator = CreateResultGenerator(settings);
            var resultAnalyzer = new ResultAnalyzer<FeatureInfo>(new FeatureInfoFactoryFFF());
            return new PrintTestRunner(caseName, resultGenerator, resultAnalyzer);
        }

        public static ResultGenerator<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings> CreateResultGenerator(SingleMaterialFFFSettings settings)
        {
            return new ResultGenerator<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings>(
                new SinglePartGenerator<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings>(), settings, new ConsoleLogger());
        }
    }
}