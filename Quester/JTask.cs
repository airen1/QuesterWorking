using System;
using System.Threading;
using System.Threading.Tasks;

namespace WowAI
{
    public class JTask
    {
        private readonly Task _task;
        private readonly CancellationTokenSource _cts;


        public JTask(Action action)
        {
            _cts = new CancellationTokenSource();
            _task = new Task(action, _cts.Token);
        }

        public CancellationToken Token => _cts.Token;

        public void Start()
        {
            _task?.Start();
        }

        public bool CancelSync()
        {
            try
            {
                _cts.Cancel();
                return _task.Wait(2000);
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }
}
