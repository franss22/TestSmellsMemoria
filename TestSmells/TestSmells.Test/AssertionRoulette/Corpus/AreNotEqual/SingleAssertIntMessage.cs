using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corpus
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod()
        {
            int a = 1;
            int b = 2;
            Assert.AreNotEqual(a, b, "Expected a, actual b");
        }
    }
}