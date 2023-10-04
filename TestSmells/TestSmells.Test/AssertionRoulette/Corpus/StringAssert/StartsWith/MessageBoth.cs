using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corpus
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod()
        {
            string a = "abc";
            string b = "ab";
            StringAssert.StartsWith(a, b, "Expected a, actual b");

            string c = "cde";
            string d = "de";
            StringAssert.StartsWith(c, d, "Expected c, actual d");
        }
    }
}