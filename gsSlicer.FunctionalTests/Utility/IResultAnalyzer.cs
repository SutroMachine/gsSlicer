namespace gsCore.FunctionalTests.Utility
{
    public interface IResultAnalyzer
    {
        void CompareResults(string pathExpected, string pathActual);
    }
}