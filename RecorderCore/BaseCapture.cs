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
        public bool useSpin { get; private set; } = false;
        private event Action actionStarted;
        private event Action actionEnded;
        private event Action cycleEnded;
        private bool DiagnosticEnable;

        protected object ReadSettingsLocker = new object();
        protected Thread WorkingThread;
        protected CancellationTokenSource CancellationTokenSource;
        protected bool paused;
        protected bool action_off=true;
        protected double AfterCaptureSleeping=0;

        #region diagnostic
        protected object DiagnosticLocker = new object();
        public double ActionAverageTimespan = 0;
        public double CycleAverageTimespan = 0;
        internal DateTime WorkingStartedTime;
        internal DateTime ActionStartedTime;
        internal List<double> ActionsTimespans = new List<double>();
        internal List<double> CycleTimespans = new List<double>();
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
                CycleAverageTimespan = CycleTimespans.Sum() / CycleTimespans.Count;
            }

        }
        #endregion
        public void SetAfterCaptureSleeping(int SleepingTime)
        {
            lock(ReadSettingsLocker)
                this.AfterCaptureSleeping = SleepingTime;
        }
        public BaseCapture(bool DiagnosticEnable=true)
        {
            lock (ReadSettingsLocker)
            {
                CancellationTokenSource = new CancellationTokenSource();
                WorkingThread = new Thread(new ParameterizedThreadStart(work));
                paused = false;
                this.DiagnosticEnable = DiagnosticEnable;
                if (this.DiagnosticEnable)
                {
                    actionStarted += ActionStarted;
                    actionEnded += ActionEnded;
                    cycleEnded += CycleEnded;
                }
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

        public void ActionOff()
        {
            lock (ReadSettingsLocker)
                action_off = true;
        }
        public void ActionOn()
        {
            lock (ReadSettingsLocker)
                action_off = false;
        }

        internal virtual void sleep(double sleep)
        {
            Thread.Sleep(sleep>=1?(int)sleep:0);
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
                bool local_action_on;
                double local_SleepingTime;
                lock (ReadSettingsLocker)
                {
                    local_SleepingTime = AfterCaptureSleeping;
                    local_pause = paused;
                    local_action_on = action_off;
                }
                if (!local_pause)
                {
                    if (DiagnosticEnable&&actionStarted != null) actionStarted.Invoke();
                    double[,] buffer = GetImage();
                    if (imageReciever != null) imageReciever.Invoke(buffer);
                    if (externalAction != null&& local_action_on) externalAction.Invoke();
                    if (DiagnosticEnable && actionEnded != null) actionEnded.Invoke();
                    sleep(local_SleepingTime);
                    if (DiagnosticEnable&&cycleEnded != null) cycleEnded.Invoke();
                }
                else Thread.Sleep(100);
            }
        }
    }
}
