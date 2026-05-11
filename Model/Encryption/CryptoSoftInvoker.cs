using System.Diagnostics;
using EasySave.Model.Config;

namespace EasySave.Model.Encryption
{
    public static class CryptoSoftInvoker
    {
        public static int EncryptFile(string filePath, AppConfig config)
        {
            string cryptoSoftFullPath = Path.Combine(
                AppContext.BaseDirectory,
                config.CryptoSoftPath
            );

            var psi = new ProcessStartInfo
            {
                FileName = cryptoSoftFullPath,
                Arguments = $"\"{filePath}\" \"{config.CryptoSoftKey}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start CryptoSoft process.");

            process.WaitForExit();

            // Handle the single-instance restriction specifically
            if (process.ExitCode == -1)
                throw new InvalidOperationException(
                    "CryptoSoft is already running on this machine. " +
                    $"Backup job '{filePath}' has been paused.");

            // Handle all other errors (-99 = internal exception, etc.)
            if (process.ExitCode < 0)
                throw new InvalidOperationException(
                    $"CryptoSoft failed with exit code {process.ExitCode} " +
                    $"(0x{process.ExitCode & 0xFFFFFFFF:X8}). File: {filePath}");

            return process.ExitCode;
        }

        public static bool ShouldEncrypt(string filePath, AppConfig config)
        {
            string ext = Path.GetExtension(filePath);
            return config.EncryptedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
