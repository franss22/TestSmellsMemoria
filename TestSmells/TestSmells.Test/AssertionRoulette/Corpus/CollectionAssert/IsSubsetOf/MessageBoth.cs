using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corpus
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod()
        {
            int[] a = {1, 2, 3};
            int[] b = {2, 3, 4};
            CollectionAssert.IsSubsetOf(a, b, "Expected a, actual b");

            int[] c = {1, 2, 3};
            int[] d = {2, 3, 4};
            CollectionAssert.IsSubsetOf(c, d, "Expected c, actual d");
        }
    }
}