using RecorderCore.Images;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{
    
    public class HilbertCalculator
    {
        Task ProcessingTask;
        private ConcurrentQueue<HilbertPhaseImage2> InputQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        private ConcurrentQueue<HilbertPhaseImage2> ResultQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        private object locker = new object();
        public void PutImage(byte[,,] image,int level =0, double wavelength = 632.8)
        {
            InputQueue.Enqueue(new HilbertPhaseImage2(image, level, wavelength));
            bool TryEnterLockResult = false;
            Monitor.TryEnter(locker, ref TryEnterLockResult);
            if (ProcessingTask == null&&TryEnterLockResult)
            {
                try
                {
                    ProcessingTask = Task.Factory.StartNew(() =>
                    {
                        while (!InputQueue.IsEmpty && InputQueue.TryDequeue(out HilbertPhaseImage2 hpi))
                        {
                            hpi.Convert();
                            hpi.Calc();
                            hpi.Unwrapp();
                            hpi.ReverseConvert();
                            ResultQueue.Enqueue(hpi);
                            while (InputQueue.Count > 10) InputQueue.TryDequeue(out HilbertPhaseImage2 hpi2);
                            while (ResultQueue.Count > 10) ResultQueue.TryDequeue(out HilbertPhaseImage2 hpi2);
                        }
                        ProcessingTask = null;
                    });
                }
                catch { };
                Monitor.Exit(locker);
            }
            

        }
        public HilbertPhaseImage2 GetImage()
        {
            if (ResultQueue.TryDequeue(out HilbertPhaseImage2 hpi2))
            {
                return hpi2;
            }
            else return null;
        }
    }
}
