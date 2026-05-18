using System.Diagnostics;
using EasySave.Model.Config;

namespace EasySave.Model.Encryption
{
    public static class CryptoSoftInvoker
    {
        private static readonly SemaphoreSlim _cryptoLock = new SemaphoreSlim(1, 1);

        public static async Task<int> EncryptFileAsync(string filePath, AppConfig config)
        {
            // Wait until no other job is running CryptoSoft
            await _cryptoLock.WaitAsync();
            try
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

                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var process = Process.Start(psi)
                    ?? throw new InvalidOperationException("Failed to start CryptoSoft process.");

                await process.WaitForExitAsync();
                sw.Stop();

                // Exit code -1 -> single-instance restriction (should no longer occur,
                // but kept as a safety net in case CryptoSoft is launched externally)
                if (process.ExitCode == -1)
                    throw new InvalidOperationException(
                        $"CryptoSoft rejected launch (single-instance). File: {filePath}");

                // Any other negative exit code -> internal CryptoSoft error
                if (process.ExitCode < 0)
                    throw new InvalidOperationException(
                        $"CryptoSoft failed with exit code {process.ExitCode} " +
                        $"(0x{process.ExitCode & 0xFFFFFFFF:X8}). File: {filePath}");

                return (int)sw.ElapsedMilliseconds;
            }
            finally
            {
                // Always release, even on exception, so the next job can proceed
                _cryptoLock.Release();
            }
        }

        public static int EncryptFile(string filePath, AppConfig config)
            => EncryptFileAsync(filePath, config).GetAwaiter().GetResult();

        public static bool ShouldEncrypt(string filePath, AppConfig config)
        {
            string ext = Path.GetExtension(filePath);
            return config.EncryptedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
