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
        public event Action externalAction;

        private event Action actionStarted;
        private event Action actionEnded;
        private event Action cycleEnded;

        protected object ReadSettingsLocker = new object();
        protected Thread WorkingThread;
        protected CancellationTokenSource CancellationTokenSource;
        protected bool paused;
        protected double FPS;
        protected int AfterCaptureSleeping=0;

        #region diagnostic
        protected object DiagnosticLocker = new object();
        public double ActionAverageTimespan = 0;
        public double CycleAverageTimespan = 0;
        internal DateTime WorkingStartedTime;
        internal DateTime ActionStartedTime;
        List<double> ActionsTimespans = new List<double>();
        List<double> CycleTimespans = new List<double>();
        internal virtual void ActionStarted()
        {
            lock (DiagnosticLocker)
                ActionStartedTime = DateTime.UtcNow;
        }

        internal virtual void ActionEnded()
        {
            lock (DiagnosticLocker)
            {
                if (ActionsTimespans.Count > 50) ActionsTimespans.RemoveAt(0);
                ActionsTimespans.Add(DateTime.UtcNow.Subtract(ActionStartedTime).TotalMilliseconds);
                ActionAverageTimespan = ActionsTimespans.Sum() / ActionsTimespans.Count;
            }

        }
     
        internal virtual void CycleEnded()
        {
            lock (DiagnosticLocker)
            {
                if (CycleTimespans.Count > 50) CycleTimespans.RemoveAt(0);
                CycleTimespans.Add(DateTime.UtcNow.Subtract(ActionStartedTime).TotalMilliseconds);
                CycleAverageTimespan = CycleTimespans.Sum() / ActionsTimespans.Count;
            }

        }
        #endregion
        public void SetAfterCaptureSleeping(int SleepingTime)
        {
            lock(ReadSettingsLocker)
                this.AfterCaptureSleeping = SleepingTime;
        }
        public BaseCapture()
        {
            lock (ReadSettingsLocker)
            {
                CancellationTokenSource = new CancellationTokenSource();
                WorkingThread = new Thread(new ParameterizedThreadStart(work));
                paused = false;
                actionStarted += ActionStarted;
                actionEnded += ActionEnded;
                cycleEnded += CycleEnded;
            }
        }

        public void Start()
        {
            WorkingStartedTime = DateTime.UtcNow;
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
            WorkingStartedTime = DateTime.UtcNow;
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
                int local_SleepingTime;
                lock (ReadSettingsLocker)
                {
                    local_SleepingTime = AfterCaptureSleeping;
                    local_pause = paused;
                }
                if (!local_pause)
                {
                    if (actionStarted != null) actionStarted.Invoke();
                    double[,] buffer = GetImage();
                    if (imageReciever != null) imageReciever.Invoke(buffer);
                    if (externalAction != null) externalAction.Invoke();
                    if (actionEnded != null) actionEnded.Invoke();
                    Thread.Sleep(local_SleepingTime);
                    if (cycleEnded != null) cycleEnded.Invoke();
                }
                else Thread.Sleep(100);
            }
        }
    }
}
