using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.Model.Storage
{
    /// <summary>
    /// Implementation of IStorage using the local file system. Make the link between the backup engine and System.IO.
    /// </summary>
    public class LocalStorage : IStorage
    {
        // Go through each file in a directory
        public IEnumerable<string> EnumerateFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                yield break;

            foreach (var file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                yield return file;
        }

        public bool CopyFile(string sourcePath, string destinationPath)
        {
            try
            {
                File.Copy(sourcePath, destinationPath, overwrite: true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public FileMetadata GetFileInfo(string filePath)
        {
            if (!File.Exists(filePath))
                return new FileMetadata(false, 0, DateTime.MinValue);

            var info = new FileInfo(filePath);
            return new FileMetadata(
                exists: true,
                size: info.Length,
                lastModified: info.LastWriteTime
            );
        }
    }
}