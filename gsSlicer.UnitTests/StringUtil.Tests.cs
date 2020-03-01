using gs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gsCore.UnitTests
{
    [TestClass]
    public class StringUtilTests
    {
        [TestMethod]
        public void FormatSettingOverride_Flat()
        {
            var actual = StringUtil.FormatSettingOverride("SettingName:2");
            var expected = "{\"SettingName\":2}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FormatSettingOverride_OneDeep()
        {
            var actual = StringUtil.FormatSettingOverride("Subsetting.SettingName:2");
            var expected = "{\"Subsetting\":{\"SettingName\":2}}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FormatSettingOverride_ThreeDeep()
        {
            var actual = StringUtil.FormatSettingOverride("a.b.c:2");
            var expected = "{\"a\":{\"b\":{\"c\":2}}}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FormatSettingOverride_StringHasQuotes()
        {
            var actual = StringUtil.FormatSettingOverride("SettingName:SettingValue");
            var expected = "{\"SettingName\":\"SettingValue\"}";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FormatSettingOverride_Boolean()
        {
            var actual = StringUtil.FormatSettingOverride("SettingName:true");
            var expected = "{\"SettingName\":true}";

            Assert.AreEqual(expected, actual);
        }
    }
}