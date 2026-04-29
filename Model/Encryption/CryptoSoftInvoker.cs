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

            using var process = Process.Start(psi);
            process!.WaitForExit();

            return process.ExitCode;
        }

        public static bool ShouldEncrypt(string filePath, AppConfig config)
        {
            string ext = Path.GetExtension(filePath);
            return config.EncryptedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
    }
}
