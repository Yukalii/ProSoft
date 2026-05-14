using EasySave.Model.Backup;

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
        /// Copy a file from source to destination (no cancel)
        /// </summary>
        bool CopyFile(string sourcePath, string destinationPath);

        /// <summary>
        /// Respects the control token for immediate stop during a file transfer.
        /// </summary>
        bool CopyFile(string sourcePath, string destinationPath, JobControlToken? controlToken);

        /// <summary>
        /// Return metadata about a file (size, existence, last modified).
        /// </summary>
        FileMetadata GetFileInfo(string filePath);
    }
}
