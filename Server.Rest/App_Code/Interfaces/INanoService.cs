namespace Server.Rest
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using ionix.Utils;

    public interface INanoService
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
    }


    public abstract class NanoServiceBase : Singleton, INanoService
    {
        private Timer _timer;
        private readonly TimeSpan _interval;

        protected NanoServiceBase(TimeSpan interval)
        {
            this._interval = interval;
        }

        public virtual void Start()
        {
            this.Stop();

            try
            {
                this._timer = new Timer(this.OnTickInternal, null, TimeSpan.Zero, this._interval);
                this.IsRunning = true;
            }
            catch (Exception ex)
            {
                SQLog.Logger.Create(new StackTrace()).OnException(ex).SaveAsync();

                this.IsRunning = false;
            }
        }

        public virtual void Stop()
        {
            this._timer?.Dispose();
            this._timer = null;

            this.IsRunning = false;
        }

        public bool IsRunning { get; protected set; }


        private void OnTickInternal(object state)
        {
            this.OnTick();
        }

        protected abstract void OnTick();
    }
}
