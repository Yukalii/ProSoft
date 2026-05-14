using System;
using System.Threading;

namespace EasySave.Model.Backup
{
    public sealed class JobControlToken : IDisposable
    {
        private readonly ManualResetEventSlim _pauseEvent = new(true);
        private readonly CancellationTokenSource _cts = new();

        public bool IsPaused { get; private set; }
        public bool IsStopped { get; private set; }

        public CancellationToken Token => _cts.Token;

        public void Play()
        {
            if (IsStopped) return;
            IsPaused = false;
            _pauseEvent.Set();
        }

        public void Pause()
        {
            if (IsStopped) return;
            IsPaused = true;
            _pauseEvent.Reset();
        }

        public void Stop()
        {
            IsStopped = true;
            IsPaused = false;
            _pauseEvent.Set();
            _cts.Cancel();
        }

        public void WaitIfPaused()
        {
            _pauseEvent.Wait(_cts.Token);
        }

        public void Dispose()
        {
            _pauseEvent.Dispose();
            _cts.Dispose();
        }
    }
}