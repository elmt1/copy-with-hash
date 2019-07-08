using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CopyWithHash.Test
{
    [TestClass]
    public class TestCopyWithHash
    {
        [TestMethod]
        public void TestGetHash()
        {
            var fromStream = new MemoryStream(Encoding.UTF8.GetBytes("test string"));
            var hash = CopyWithHash.GetHash(fromStream);

            Assert.AreEqual<string>("6f8db599de986fab7a21625b7916589c", hash);
        }

        [TestMethod]
        public void TestInsertHashNormalName()
        {
            var fileInfo = new FileInfo(@"Z:\test\file.ext");
            var hash = "6f8db599de986fab7a21625b7916589c";
            var fileName = CopyWithHash.InsertHash(fileInfo, hash);

            Assert.AreEqual<string>("file.6f8db599de986fab7a21625b7916589c.ext", fileName);
        }

        [TestMethod]
        public void TestInsertHashNoExtension()
        {
            var fileInfo = new FileInfo(@"Z:\test\file");
            var hash = "6f8db599de986fab7a21625b7916589c";
            var fileName = CopyWithHash.InsertHash(fileInfo, hash);

            Assert.AreEqual<string>("file.6f8db599de986fab7a21625b7916589c", fileName);
        }

        [TestMethod]
        public void TestInsertHashMultipleExtensions()
        {
            var fileInfo = new FileInfo(@"Z:\test\file.ext1.ext2");
            var hash = "6f8db599de986fab7a21625b7916589c";
            var fileName = CopyWithHash.InsertHash(fileInfo, hash);

            Assert.AreEqual<string>("file.ext1.6f8db599de986fab7a21625b7916589c.ext2", fileName);
        }

        [TestMethod]
        public void TestRenameFiles()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var fileName = Path.Combine(tempDirectory, Path.GetRandomFileName());
            File.WriteAllText(fileName, "test string");

            CopyWithHash.RenameFiles(tempDirectory);
            var newFileName = Directory.GetFiles(tempDirectory).Single();

            Assert.AreEqual<string>(Path.GetFileNameWithoutExtension(fileName) + ".6f8db599de986fab7a21625b7916589c" + Path.GetExtension(fileName), Path.GetFileName(newFileName));
        }

        [TestMethod]
        public void TestCopyFiles()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var fileName = Path.Combine(tempDirectory, Path.GetRandomFileName());
            File.WriteAllText(fileName, "test string");

            string toTempDirectory = Path.Combine(tempDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(toTempDirectory);

            CopyWithHash.CopyFiles(tempDirectory, toTempDirectory);
            var newFileName = Directory.GetFiles(toTempDirectory).Single();

            Assert.AreEqual<string>(Path.GetFileNameWithoutExtension(fileName) + ".6f8db599de986fab7a21625b7916589c" + Path.GetExtension(fileName), Path.GetFileName(newFileName));
        }

        [TestMethod]
        public void TestCopyExistingFiles()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var fileName = Path.Combine(tempDirectory, Path.GetRandomFileName());
            File.WriteAllText(fileName, "test string");

            string toTempDirectory = Path.Combine(tempDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(toTempDirectory);

            CopyWithHash.CopyFiles(tempDirectory, toTempDirectory);
            var newFileName = Directory.GetFiles(toTempDirectory).Single();

            var originalTime = File.GetLastWriteTimeUtc(newFileName);
            Thread.Sleep(50);

            // Create new copy of file with later timestamp
            File.WriteAllText(fileName, "test string");
            CopyWithHash.CopyFiles(tempDirectory, toTempDirectory);
            var newTime = File.GetLastWriteTimeUtc(newFileName);

            var timespan = (newTime - originalTime).TotalMilliseconds;

            Assert.IsTrue(timespan >= 50);
        }

        [TestMethod]
        public void TestMainRenameFiles()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var fileName = Path.Combine(tempDirectory, Path.GetRandomFileName());
            File.WriteAllText(fileName, "test string");

            CopyWithHash.Main(new string[] { tempDirectory });
            var newFileName = Directory.GetFiles(tempDirectory).Single();

            Assert.AreEqual<string>(Path.GetFileNameWithoutExtension(fileName) + ".6f8db599de986fab7a21625b7916589c" + Path.GetExtension(fileName), Path.GetFileName(newFileName));
        }

        [TestMethod]
        public void TestMainCopyFiles()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            var fileName = Path.Combine(tempDirectory, Path.GetRandomFileName());
            File.WriteAllText(fileName, "test string");

            string toTempDirectory = Path.Combine(tempDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(toTempDirectory);

            CopyWithHash.Main(new string[] { tempDirectory, toTempDirectory });
            var newFileName = Directory.GetFiles(toTempDirectory).Single();

            Assert.AreEqual<string>(Path.GetFileNameWithoutExtension(fileName) + ".6f8db599de986fab7a21625b7916589c" + Path.GetExtension(fileName), Path.GetFileName(newFileName));
        }
    }
}
