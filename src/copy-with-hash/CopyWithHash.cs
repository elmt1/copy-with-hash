using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CopyWithHash
{
    public static class CopyWithHash
    {
        static void Main(string[] args)
        {
            if (args.Length.Equals(1))
            {
                RenameFiles(args[0]);
            }
            else if (args.Length.Equals(2))
            {
                CopyFiles(args[0], args[1]);
            }
            else
            {
                Console.WriteLine("Usage: CopyWithHash <fromDirectory> <optional targetDirectory>");
            }
        }

        public static void CopyFiles(string fromPath, string toPath)
        {
            var existingFileNames = string.Join(string.Empty, Directory.GetFiles(toPath));
            var fromDirectoryInfo = new DirectoryInfo(fromPath);
            var fileInfos = fromDirectoryInfo.GetFiles();
            FileStream fromFileStream = null;

            foreach (var fileInfo in fileInfos)
            {
                fromFileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var textReader = new StreamReader(fromFileStream))
                {
                    var hash = GetHash(fromFileStream);

                    // Don't copy if a file already exists with the same content but set last modified date
                    if (existingFileNames.Contains(hash))
                    {
                        var toDirectoryInfo = new DirectoryInfo((toPath + (toPath.EndsWith("\\") ? string.Empty : "\\") + "*" + hash + "*.*"));
                        var toFileInfo = toDirectoryInfo.GetFiles().FirstOrDefault();

                        if (toFileInfo != null && toFileInfo.LastWriteTime < fileInfo.LastWriteTime)
                        {
                            File.SetLastWriteTime(toFileInfo.FullName, fileInfo.LastWriteTime);
                        }
                    }
                    else
                    {
                        var toFilePath = toPath + (toPath.EndsWith("\\") ? string.Empty : "\\") + InsertHash(fileInfo, hash);
                        using (var outFileStream = new FileStream(toFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                        {
                            fromFileStream.CopyTo(outFileStream);
                            outFileStream.Flush();
                        }
                    }
                }
            }
        }

        public static void RenameFiles(string fromPath)
        {
            var fromDirectoryInfo = new DirectoryInfo(fromPath);
            var fileInfos = fromDirectoryInfo.GetFiles();
            FileStream fromFileStream = null;

            foreach (var fileInfo in fileInfos)
            {
                string fileName = null;
                fromFileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var textReader = new StreamReader(fromFileStream))
                {
                    var hash = GetHash(fromFileStream);
                    fileName = InsertHash(fileInfo, hash);
                }

                if (fileInfo.Name != fileName)
                {
                    File.Move(fileInfo.FullName, fileInfo.DirectoryName + "\\" + fileName);
                }
            }
        }

        /// <summary>Inserts the file hash if it isn't already there.</summary>
        /// <param name="fromStream">From stream.</param>
        /// <param name="fileInfo">The file information.</param>
        /// <returns></returns>
        public static string GetHash(Stream fromStream)
        {
            string hash = null;
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(fromStream);
                fromStream.Seek(0, SeekOrigin.Begin); // reset to start
                var stringBuilder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    stringBuilder.Append(bytes[i].ToString("x2"));
                }
                hash = stringBuilder.ToString();
            }

            return hash;
        }

        /// <summary>Inserts a hash into a file name, i.e. file.xyz become file.filecontenthash.xyz.</summary>
        /// <param name="fileInfo">The file information.</param>
        /// <param name="hash">The hash to insert.</param>
        /// <returns></returns>
        public static string InsertHash(FileInfo fileInfo, string hash)
        {
            var pos = fileInfo.Name.LastIndexOf(".");
            if (pos <= 0)
            {
                pos = fileInfo.Name.Length;
            }

            return (fileInfo.Name.Contains(hash) ? fileInfo.Name : fileInfo.Name.Insert(pos, "." + hash));
        }
    }
}
