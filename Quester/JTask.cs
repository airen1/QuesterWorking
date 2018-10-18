using System;
using System.Threading;
using System.Threading.Tasks;

namespace AlbionAI
{
    public class JTask
    {
        private Task task;
        private CancellationTokenSource cts;


        public JTask(Action action)
        {
            cts = new CancellationTokenSource();
            task = new Task(action, cts.Token);
        }

        public CancellationToken token { get { return cts.Token; } }

        public void Start()
        {
            if (task != null)
                task.Start();
        }

        public bool CancelSync()
        {
            try
            {
                cts.Cancel();
                return task.Wait(2000);
            }
            catch
            {
            }
            return false;
        }
    }
}
