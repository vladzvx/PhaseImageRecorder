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
        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected bool paused = false;

        public BaseCapture()
        {
            lock (ReadSettingsLocker)
            {
                WorkingThread = new Thread(new ParameterizedThreadStart(work));
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
                lock (ReadSettingsLocker)
                {
                    local_pause = paused;
                }
                if (!local_pause)
                {
                    if (imageReciever != null) imageReciever.Invoke(GetImage());
                }
                else Thread.Sleep(100);
            }
        }
    }
}
