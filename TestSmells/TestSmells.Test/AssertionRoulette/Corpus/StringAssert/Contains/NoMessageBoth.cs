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
            StringAssert.Contains(a, b);

            string c = "cde";
            string d = "de";
            StringAssert.Contains(c, d);
        }
    }
}