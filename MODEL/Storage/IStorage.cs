using System.Collections.Generic;

namespace EasySave.Model.Storage
{
    /// <summary>
    /// Abstraction layer for file system operation
    /// Allows the backup engine to work with the different storage types (local, network)
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Returns all files contained in the given directory with recursiv call.
        /// </summary>
        IEnumerable<string> EnumerateFiles(string directoryPath);

        /// <summary>
        /// Copy a file from source to destination
        /// </summary>
        bool CopyFile(string sourcePath, string destinationPath);

        /// <summary>
        /// Return metadata about a file (size, existence, last modified).
        /// </summary>
        FileMetadata GetFileInfo(string filePath);
    }
}
