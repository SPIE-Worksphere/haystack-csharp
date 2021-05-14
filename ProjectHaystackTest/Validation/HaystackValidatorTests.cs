using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack.Validation;

namespace ProjectHaystackTest.Validation
{
    [TestClass]
    public class HaystackValidatorTests
    {
        [TestMethod]
        public void TestIsTagName()
        {
            Assert.IsFalse(HaystackValidator.IsTagName(""));
            Assert.IsFalse(HaystackValidator.IsTagName("A"));
            Assert.IsFalse(HaystackValidator.IsTagName(" "));
            Assert.IsTrue(HaystackValidator.IsTagName("a"));
            Assert.IsTrue(HaystackValidator.IsTagName("a_B_19"));
            Assert.IsFalse(HaystackValidator.IsTagName("a b"));
            Assert.IsFalse(HaystackValidator.IsTagName("a\u0128"));
            Assert.IsFalse(HaystackValidator.IsTagName("a\u0129x"));
            Assert.IsFalse(HaystackValidator.IsTagName("a\uabcdx"));
            Assert.IsTrue(HaystackValidator.IsTagName("^tag"));
        }

        [TestMethod]
        public void TestIsId()
        {
            Assert.IsFalse(HaystackValidator.IsReferenceId(""));
            Assert.IsFalse(HaystackValidator.IsReferenceId("%"));
            Assert.IsTrue(HaystackValidator.IsReferenceId("a"));
            Assert.IsTrue(HaystackValidator.IsReferenceId("a-b:c"));
            Assert.IsFalse(HaystackValidator.IsReferenceId("a b"));
            Assert.IsFalse(HaystackValidator.IsReferenceId("a\u0129b"));
        }

        [TestMethod]
        public void VerifyUnitNames()
        {
            Assert.IsTrue(HaystackValidator.IsUnitName(null));
            Assert.IsFalse(HaystackValidator.IsUnitName(""));
            Assert.IsTrue(HaystackValidator.IsUnitName("x_z"));
            Assert.IsFalse(HaystackValidator.IsUnitName("x z"));
            Assert.IsTrue(HaystackValidator.IsUnitName("ft²"));
        }

        [TestMethod]
        public void TestIsLatitude()
        {
            Assert.IsFalse(HaystackValidator.IsLatitude(-91m));
            Assert.IsTrue(HaystackValidator.IsLatitude(-90m));
            Assert.IsTrue(HaystackValidator.IsLatitude(-89m));
            Assert.IsTrue(HaystackValidator.IsLatitude(90m));
            Assert.IsFalse(HaystackValidator.IsLatitude(91m));
        }

        [TestMethod]
        public void TestIsLongitude()
        {
            Assert.IsFalse(HaystackValidator.IsLongitude(-181m));
            Assert.IsTrue(HaystackValidator.IsLongitude(-179.99m));
            Assert.IsTrue(HaystackValidator.IsLongitude(180m));
            Assert.IsFalse(HaystackValidator.IsLongitude(181m));
        }
    }
}