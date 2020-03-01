using gs;
using gs.info;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gsCore.UnitTests
{
    [TestClass]
    public class AdditiveSettingsTests
    {
        [TestMethod]
        public void CloneAs_ValuesCloneCorrectly()
        {
            // arrange
            var orig = new FlashforgeSettings();
            orig.Shells = 10;
            orig.Machine.NozzleDiamMM = 20;
            orig.Machine.ManufacturerName = "A";

            // act
            var copy = orig.CloneAs<FlashforgeSettings>();

            // assert
            Assert.AreEqual(10, copy.Shells);
            Assert.AreEqual(20, copy.Machine.NozzleDiamMM);
            Assert.AreEqual("A", orig.Machine.ManufacturerName);
            Assert.AreNotSame(copy.Machine, orig.Machine);
        }

        [TestMethod]
        public void CloneAs_CloneValuesDoNotAffectOriginal()
        {
            // arrange
            var orig = new GenericRepRapSettings();
            orig.Shells = 10;
            orig.Machine.NozzleDiamMM = 20;
            orig.Machine.ManufacturerName = "A";

            // act
            GenericRepRapSettings copy = orig.CloneAs<GenericRepRapSettings>();
            copy.Shells *= 2;
            copy.Machine.NozzleDiamMM *= 20;
            copy.Machine.ManufacturerName = "B";

            // assert
            Assert.AreEqual(10, orig.Shells);
            Assert.AreEqual(20, orig.Machine.NozzleDiamMM);
            Assert.AreEqual("A", orig.Machine.ManufacturerName);
            Assert.AreNotSame(copy.Machine, orig.Machine);
        }

        [TestMethod]
        public void CloneAs_ToDerivedClass()
        {
            // arrange
            var orig = new GenericPrinterSettings("", "", "");

            // act
            var clone = orig.CloneAs<GenericRepRapSettings>();

            // assert
            Assert.IsNotNull(clone);
        }

        [TestMethod]
        public void CloneAs_ToParentClass()
        {
            // arrange
            var orig = new GenericRepRapSettings();

            // act
            var clone = orig.CloneAs<GenericPrinterSettings>();

            // assert
            Assert.IsNotNull(clone);
        }

        [TestMethod]
        public void CloneAs_SiblingClass()
        {
            // arrange
            var orig = new PrusaSettings(Prusa.Models.i3_MK3);

            // act
            var clone = orig.CloneAs<FlashforgeSettings>();

            // assert
            Assert.IsNotNull(clone);
            Assert.AreEqual(Flashforge.Models.Unknown, clone.ModelEnum);
        }
    }
}