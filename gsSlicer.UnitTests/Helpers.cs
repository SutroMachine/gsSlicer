using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gsCore.UnitTests
{
    public class AssertExtensions
    {
        public static void AreEqual(Vector2d expected, Vector2d actual)
        {
            if (!expected.EpsilonEqual(actual, MathUtil.Epsilon))
                throw new AssertFailedException($"AssertExtensions.Assert failed. Expected:{expected}. Actual{actual}");
        }
    }
}