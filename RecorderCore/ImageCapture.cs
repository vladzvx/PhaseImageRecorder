using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RecorderCore
{
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

        #endregion

        #region constructors
        public ImageCapture()
        {
            lock (locker)
            {
                capture = new VideoCapture(0);
                thread = new Thread(new ThreadStart(grab));
                
            }

        }

        public void Start()
        {
            thread.Start();

        }

        #endregion

        #region image grabbing

        private void grab()
        {
            int frameCounter = 0;
            while (true)
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
