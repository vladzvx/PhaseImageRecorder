using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RecorderCore
{
   //todo вынести функционал потока, ставящегося на паузу в отдельный класс
    public class ImageCapture
    {
        #region fields
        public delegate void ImageReciever(Mat image);
        public delegate void ExternalAction(int count);
        public event ImageReciever rec;
        public event ExternalAction action;
        private int CameraNumber = 0;
        private int FramePause = 0;
        private double MaxFrameCounter = 0;

        private Emgu.CV.VideoCapture capture;
        private Thread thread;
        private object locker = new object();
        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource PauseTokenSource = new CancellationTokenSource();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public bool paused = false;
        #endregion

        #region constructors
        public ImageCapture()
        {
            lock (locker)
            {
                capture = new VideoCapture(0);
                thread = new Thread(new ParameterizedThreadStart(grab));
                
            }

        }

        public void Start()
        {
            //thread.Start(new CancellationToken[2] { CancellationTokenSource.Token, PauseTokenSource.Token });
            thread.Start(CancellationTokenSource.Token);
        }

        public void Stop()
        {
            CancellationTokenSource.Cancel();
        }

        public void Pause()
        {
            _lock.EnterWriteLock();
            paused = true;
            _lock.ExitWriteLock();
        }
        public void PauseRelease()
        {
            _lock.EnterWriteLock();
            paused = false;
            _lock.ExitWriteLock();
        }
        #endregion

        #region image grabbing

        private void grab(object cancellationToken)
        {
            CancellationToken[] cts = cancellationToken as CancellationToken[];
            if (cts != null)
            {
                int frameCounter = 0;
                while (!cts[0].IsCancellationRequested)
                {
                    _lock.EnterReadLock();
                    bool local_pause = paused;
                    _lock.ExitReadLock();
                    if (!local_pause)
                    {
                        lock (locker)
                        {
                            if (action != null) action.Invoke(frameCounter);
                            Thread.Sleep(FramePause);
                            Grab();
                            frameCounter = frameCounter < MaxFrameCounter ? frameCounter + 1 : 0;
                        }
                    }
                    else Thread.Sleep(300);

                }
            }
            else
            {
                CancellationToken ct = (CancellationToken)cancellationToken;
                int frameCounter = 0;
                while (!ct.IsCancellationRequested)
                {
                    lock (locker)
                    {
                        if (action != null) action.Invoke(frameCounter);
                        Thread.Sleep(FramePause);
                        Grab();
                        frameCounter = frameCounter < MaxFrameCounter ? frameCounter + 1 : 0;
                    }
                }
            }

        }

        private void Grab()
        {
            Mat mat = new Mat();
            capture.Grab();
            capture.Retrieve(mat);
            if (rec != null)
            {
                rec.Invoke(mat);
            } 
        }

        #endregion

        #region external ruling
        public void UpdateCamera(int CameraNumber)
        {
            lock (locker)
            {
                if (this.CameraNumber!= CameraNumber)
                    capture = new VideoCapture(CameraNumber);
            }
        }

        public void UpdateMaxFrameCounter(int MaxFrameCounter)
        {
            lock (locker)
            {
                this.MaxFrameCounter = MaxFrameCounter;
            }
        }

        public void UpdateFramePause(int FramePause)
        {
            lock (locker)
            {
                this.FramePause = FramePause;
            }
        }

        #endregion

        #region add and remove handlers
        public void AddReciever(ImageReciever imageReciever)
        {
            rec += imageReciever;
        }
        public void AddExternalAction(ExternalAction externalAction)
        {
            action += externalAction;
        }
        public void RemoveReciever(ImageReciever imageReciever)
        {
            rec -= imageReciever;
        }
        public void RemoveExternalAction(ExternalAction externalAction)
        {
            action -= externalAction;
        }
        #endregion

    }



}
