namespace EasySave.Model.Backup
{
    /// <summary>
    /// Executes a set of backup jobs by 1-based index without opening the WPF UI.
    /// Writes progress and results to the console.
    /// </summary>
    public static class BackupCliRunner
    {
        public static async Task<int> RunAsync(BackupJobManager jobManager, List<int> indices)
        {
            int exitCode = 0;

            foreach (int idx in indices)
            {
                // Convert 1-based index to 0-based
                int zeroIdx = idx - 1;

                if (zeroIdx < 0 || zeroIdx >= jobManager.Jobs.Count)
                {
                    Console.Error.WriteLine($"[ERROR] No job at index {idx}. " +
                        $"Valid range: 1-{jobManager.Jobs.Count}.");
                    exitCode = 1;
                    continue;
                }

                var job = jobManager.Jobs[zeroIdx];
                Console.WriteLine($"[{idx}] Starting job '{job.Name}' ...");

                try
                {
                    await jobManager.ExecuteJob(job.Name);
                    Console.WriteLine($"[{idx}] '{job.Name}' completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[{idx}] '{job.Name}' FAILED: {ex.Message}");
                    exitCode = 1;
                }
            }

            return exitCode;
        }
    }
}