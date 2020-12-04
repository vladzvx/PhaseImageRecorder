using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{
    public abstract class BaseCapture
    {
        public delegate void ImageReciever(double[,] image);
        public event ImageReciever imageReciever;

        protected object ReadSettingsLocker = new object();
        protected Thread WorkingThread;
        protected CancellationTokenSource CancellationTokenSource;
        protected bool paused;
        protected double FPS;

        public void SetFPS(double FPS)
        {
            lock(ReadSettingsLocker)
                this.FPS = FPS;
        }
        public BaseCapture()
        {
            lock (ReadSettingsLocker)
            {
                CancellationTokenSource = new CancellationTokenSource();
                WorkingThread = new Thread(new ParameterizedThreadStart(work));
                paused = false;
                FPS = 25;
            }
        }

        public void Start()
        {
            WorkingThread.Start(CancellationTokenSource.Token);
        }
        public void Stop()
        {
            CancellationTokenSource.Cancel();
        }
        public void Pause()
        {
            lock (ReadSettingsLocker)
                paused = true;
        }
        public void PauseRelease()
        {
            lock (ReadSettingsLocker)
                paused = false;
        }

        internal virtual double[,] GetImage()
        {
            return null;
        }
        internal virtual void work (object cancellationToken)
        {
            CancellationToken ct = (CancellationToken)cancellationToken;
            while (!ct.IsCancellationRequested)
            {
                bool local_pause;
                double local_FPS;
                lock (ReadSettingsLocker)
                {
                    local_FPS = FPS;
                    local_pause = paused;
                }
                if (!local_pause)
                {
                    DateTime dt1 = DateTime.UtcNow;
                    double[,] buffer = GetImage();
                    double CreatingTimespan = DateTime.UtcNow.Subtract(dt1).TotalMilliseconds;
                    if (imageReciever != null) imageReciever.Invoke(buffer);
                    double temp = 1000 / local_FPS - CreatingTimespan;
                    int sleepingTime = temp > 0 ? (int)temp : 0;
                    Thread.Sleep(sleepingTime);
                }
                else Thread.Sleep(100);
            }
        }
    }
}
