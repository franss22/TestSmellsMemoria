using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Corpus
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async void TestMethod()
        {
            var path = @"C:\Program Files\AMD\atikmdag_dce.log";
            string[] lines = { "foo", "bar" };
            await File.WriteAllLinesAsync(path, lines);
            var data = File.ReadAllBytes(path);
            Assert.IsNotNull(data);
        }
    }
}