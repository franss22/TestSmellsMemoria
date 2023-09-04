using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            var file = new FileStream(path, FileMode.Open);
            var arr = new byte[file.Length];
            await file.WriteAsync(arr, 1, 2);

            Assert.IsNotNull(file);
        }

    }
}