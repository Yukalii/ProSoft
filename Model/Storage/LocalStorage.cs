using EasySave.Model.Backup;

namespace EasySave.Model.Storage
{
    /// <summary>
    /// Implementation of IStorage using the local file system. Make the link between the backup engine and System.IO.
    /// </summary>
    public class LocalStorage : IStorage
    {
        private const int BufferSize = 81920;

        // Go through each file in a directory
        public IEnumerable<string> EnumerateFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                yield break;

            foreach (var file in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                yield return file;
        }

        public bool CopyFile(string sourcePath, string destinationPath)
            => CopyFile(sourcePath, destinationPath, null);

        public bool CopyFile(string sourcePath, string destinationPath,
                             JobControlToken? controlToken)
        {
            try
            {
                using var src = new FileStream(sourcePath, FileMode.Open,
                                                FileAccess.Read, FileShare.Read,
                                                BufferSize, FileOptions.SequentialScan);
                using var dest = new FileStream(destinationPath, FileMode.Create,
                                                FileAccess.Write, FileShare.None,
                                                BufferSize);

                var buffer = new byte[BufferSize];
                int read;
                while ((read = src.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Throws OperationCanceledException if Stop() was called
                    controlToken?.Token.ThrowIfCancellationRequested();
                    dest.Write(buffer, 0, read);
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                // Propagate so the strategy can emit a "Stopped" snapshot
                throw;
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
            return new FileMetadata(true, info.Length, info.LastWriteTime);
        }
    }
}