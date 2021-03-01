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
        public void PutImage(byte[,,] image,int level =2, double wavelength = 632.8,bool unwrap = false, int summDepth = 0, bool smooth = false)
        {
            HilbertPhaseImage2 hpi = new HilbertPhaseImage2(image, level, wavelength, unwrap, smooth);
            hpi.summDepth = summDepth;
            InputQueue.Enqueue(hpi);
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
                            //hpi.Convert();
                            var temp = InputQueue.ToArray();
                            if (hpi.summDepth>1&&temp.Length> hpi.summDepth)
                            {
                                for (int shift = 1;shift< hpi.summDepth;shift++)
                                {
                                    hpi.images[0] = ImageSource.plus(hpi.images[0], temp[shift].images[0]);
                                }
                            }
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


    public class HilbertCalculator2
    {
        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private ConcurrentQueue<HilbertPhaseImage2> InputQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        private ConcurrentQueue<HilbertPhaseImage2> CalcQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        private ConcurrentQueue<HilbertPhaseImage2> UnwrapQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        private ConcurrentQueue<HilbertPhaseImage2> ReverseConvertQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        private ConcurrentQueue<HilbertPhaseImage2> ResultQueue = new ConcurrentQueue<HilbertPhaseImage2>();
        Thread ConvertingThread;
        Thread CalculatingThread;
        Thread UnwrappingThread;
        Thread ReverseConvertingThread;
        public HilbertCalculator2()
        {
            ConvertingThread = new Thread(new ParameterizedThreadStart(ConvertTask));
            ConvertingThread.Start(CancellationTokenSource.Token);
            CalculatingThread = new Thread(new ParameterizedThreadStart(CalcTask));
            CalculatingThread.Start(CancellationTokenSource.Token);
            UnwrappingThread = new Thread(new ParameterizedThreadStart(UnwrapTask));
            UnwrappingThread.Start(CancellationTokenSource.Token);
            ReverseConvertingThread = new Thread(new ParameterizedThreadStart(ReverseConvertTask));
            ReverseConvertingThread.Start(CancellationTokenSource.Token);
        }
        private void ConvertTask(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                if (InputQueue.TryDequeue(out HilbertPhaseImage2 hpi2))
                {
                    hpi2.Convert();
                    CalcQueue.Enqueue(hpi2);
                    while (InputQueue.Count > 10) InputQueue.TryDequeue(out var trash);
                }
                else Thread.Sleep(30);
            }
        }

        private void CalcTask(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                if (CalcQueue.TryDequeue(out HilbertPhaseImage2 hpi2))
                {
                    var temp = CalcQueue.ToArray();
                    if (hpi2.summDepth > 1 && temp.Length > hpi2.summDepth)
                    {
                        for (int shift = 1; shift < hpi2.summDepth; shift++)
                        {
                            hpi2.images[0] = ImageSource.plus(hpi2.images[0], temp[shift].images[0]);
                        }
                    }
                    hpi2.Calc();
                    UnwrapQueue.Enqueue(hpi2);
                    while (CalcQueue.Count > 10) CalcQueue.TryDequeue(out var trash);
                }
                else Thread.Sleep(30);
            }
        }

        private void UnwrapTask(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                if (UnwrapQueue.TryDequeue(out HilbertPhaseImage2 hpi2))
                {
                    hpi2.Convert();
                    ReverseConvertQueue.Enqueue(hpi2);
                    while (UnwrapQueue.Count > 10) UnwrapQueue.TryDequeue(out var trash);
                }
                else Thread.Sleep(30);
            }
        }

        private void ReverseConvertTask(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                if (ReverseConvertQueue.TryDequeue(out HilbertPhaseImage2 hpi2))
                {
                    hpi2.Convert();
                    ResultQueue.Enqueue(hpi2);
                    while (ReverseConvertQueue.Count > 10) ReverseConvertQueue.TryDequeue(out var trash);
                    while (ResultQueue.Count > 10) ResultQueue.TryDequeue(out var trash);
                }
                else Thread.Sleep(30);
            }
        }

        public void PutImage(byte[,,] image, int level = 2, double wavelength = 632.8, bool unwrap = false, int summDepth = 0, bool smooth = false)
        {
            HilbertPhaseImage2 hpi = new HilbertPhaseImage2(image, level, wavelength, unwrap, smooth);
            hpi.summDepth = summDepth;
            InputQueue.Enqueue(hpi);
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
