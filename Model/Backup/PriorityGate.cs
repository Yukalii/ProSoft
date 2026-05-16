namespace EasySave.Model.Backup
{
    /// <summary>
    /// Shared gate that enforces the priority file rule across all running jobs.
    /// A non-priority file is blocked as long as at least one priority file
    /// is still pending in any job.
    /// </summary>
    public sealed class PriorityGate
    {
        private int _pendingPriorityFiles = 0;
        private readonly object _lock = new();

        public void AddPendingPriority(int count)
        {
            lock (_lock)
                _pendingPriorityFiles += count;
        }

        public void OnePriorityDone()
        {
            lock (_lock)
            {
                if (_pendingPriorityFiles > 0)
                    _pendingPriorityFiles--;
            }
        }

        public async Task WaitForClearanceAsync(CancellationToken ct = default)
        {
            while (true)
            {
                lock (_lock)
                {
                    if (_pendingPriorityFiles == 0)
                        return;
                }
                await Task.Delay(100, ct); // poll every 100 ms
            }
        }

        public bool HasPendingPriority
        {
            get { lock (_lock) { return _pendingPriorityFiles > 0; } }
        }
    }
}