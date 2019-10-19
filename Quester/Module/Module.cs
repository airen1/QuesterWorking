using System;
using System.Threading;

namespace WowAI.Module
{
    internal class Module
    {
        internal Host Host;
        internal JTask Task;

        public virtual void Start(Host host)
        {
            Host = host;
            try
            {
                Task = new JTask(() => Run(Task.Token));
                Task.Start();
            }
            catch (Exception error)
            {
                host.log("" + error);
            }
        }

        public virtual void Run(CancellationToken ct)
        {

        }

        public virtual void Stop()
        {
            try
            {
                if (Task != null)
                    Task.CancelSync();
            }
            catch (Exception error)
            {
                Host.log("" + error);
            }
        }
    }
}
