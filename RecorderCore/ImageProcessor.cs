using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using static RecorderCore.RecordingDriver;
using System.Runtime.InteropServices;
using System.Linq;

namespace RecorderCore
{
    class ImageProcessor
    {
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public event PhaseImageReciever PhaseImageInterfaceSender;
        public event PhaseImageReciever PhaseImageSender;

        private CancellationTokenSource cancellationTokenSource; 
        private Thread MainImageProcessor;
        private Thread[] SecondaryImageProcessors;
        private ConcurrentQueue<PhaseImage> MainProcessingQuenue;
        private ConcurrentQueue<PhaseImage> SecondaryProcessingQuenue; 
        
        public void PutImage(PhaseImage phaseImage)
        {
            MainProcessingQuenue.TryDequeue(out var phaseImage1);
            MainProcessingQuenue.Enqueue(phaseImage);
        }
        public ImageProcessor(int ProcessingThreadsNumber)
        {
            if (ProcessingThreadsNumber <= 0) throw new ArgumentException("Uncorrect threads number!");
            cancellationTokenSource  = new CancellationTokenSource();
            MainProcessingQuenue = new ConcurrentQueue<PhaseImage>();
            SecondaryProcessingQuenue = new ConcurrentQueue<PhaseImage>();
            MainImageProcessor = new Thread(new ParameterizedThreadStart(MainProcessImage));
            MainImageProcessor.Start(cancellationTokenSource.Token);
            if (ProcessingThreadsNumber > 1)
            {
                SecondaryImageProcessors = new Thread[ProcessingThreadsNumber - 1];
                for (int i=0;i< ProcessingThreadsNumber - 1; i++)
                {
                    SecondaryImageProcessors[i] = new Thread(new ParameterizedThreadStart(SecondaryProcessImage));
                    SecondaryImageProcessors[i].Start(cancellationTokenSource.Token);
                }
            }
            else
            {
                SecondaryImageProcessors = null;
            }
        }
        /// <summary>
        /// Работает только с самым последним изображением, если изображений становится больше, чем обработчиков, лишние отбрасываются.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void MainProcessImage(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                PhaseImage result = null;
                while (MainProcessingQuenue.TryDequeue(out result))
                {
                    if (result != null)
                    {
                        logger.Debug(string.Format("Processing image (recorded at {0})", result.RecordingTime));
                        result.CalculatePhaseImage();
                        result.Unwrap();
                        result.Process();
                        if (PhaseImageInterfaceSender != null)
                            PhaseImageInterfaceSender.Invoke(result);
                    }
                    while (MainProcessingQuenue.TryDequeue(out result) && !MainProcessingQuenue.IsEmpty) ;

                    //logger.Debug(string.Format("Decuenue image (rec. time: {1}) from processing queue. Total {0} images in queue.", MainProcessingQuenue.Count, result.RecordingTime));
                    //if (!MainProcessingQuenue.IsEmpty)
                    //{
                    //    logger.Debug("Using additional threads");
                    //    if (SecondaryImageProcessors!=null)
                    //        SecondaryProcessingQuenue.Enqueue(result);
                    //}
                    //else
                    //{
                    //    logger.Debug("Processing image in main thread additional threads");
                    //    if (result != null)
                    //    {
                    //        logger.Debug(string.Format("Processing image (recorded at {0})", result.RecordingTime));
                    //        result.CalculatePhaseImage();
                    //        result.Unwrap();
                    //        result.Process();
                    //        if (PhaseImageInterfaceSender != null)
                    //            PhaseImageInterfaceSender.Invoke(result);
                    //        if (PhaseImageSender != null)
                    //            PhaseImageSender.Invoke(result);
                    //    }
                    //    else
                    //    {
                    //        logger.Debug("No images in queue");
                    //    }
                    //}
                }

                //Thread.Sleep(50);
            }

        }
        public void SecondaryProcessImage(object cancellationToken)
        {
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                //PhaseImage result = null;
                while (SecondaryProcessingQuenue.TryDequeue(out PhaseImage result))
                {
                    result.CalculatePhaseImage();
                    result.Unwrap();
                    result.Process();
                    if (PhaseImageSender != null)
                        PhaseImageSender.Invoke(result);

                }
                Thread.Sleep(50);
            }

        }
    }


    public class ImagePool
    {
        DateTime LastImageRecordingTime;
        object locker = new object();
        List<PhaseImage> images = new List<PhaseImage>();


        public void AddImage(PhaseImage phaseImage)
        {
            lock (locker)
            {
                images.Add(phaseImage);
            }
        }

        public bool TryGetImage(out PhaseImage phaseImage)
        {
            lock (locker)
            {
                phaseImage = null;
                if (images.Count > 0)
                {
                    images.RemoveAll(item => item.RecordingTime < LastImageRecordingTime);
                    images.Sort((item1, item2) => item1.RecordingTime>item2.RecordingTime?1:(item1.RecordingTime < item2.RecordingTime?-1:0));
                    while (images.Count > 30)
                    {
                        images.RemoveAt(0);
                    }
                    phaseImage = images[0];
                    LastImageRecordingTime = phaseImage.RecordingTime;
                    images.RemoveAt(0);
                    return true;
                }
                return false;
            }
        }
    }
    class ImageProcessor2
    {
        private ImagePool imagePool = new ImagePool();
        protected NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private CancellationTokenSource cancellationTokenSource;
        private Thread[] ImageProcessors;
        private ConcurrentQueue<PhaseImage> MainProcessingQuenue;

        public void PutImage(PhaseImage phaseImage)
        {
            //MainProcessingQuenue.TryDequeue(out var phaseImage1);
            if (MainProcessingQuenue.Count > 50)
            {
                MainProcessingQuenue.TryDequeue(out var phaseImage1);
            }
            MainProcessingQuenue.Enqueue(phaseImage);
        }

        public void CleanQuenue()
        {
            while (MainProcessingQuenue.TryDequeue(out var phaseImage1)) ;
        }
        public ImageProcessor2(int ProcessingThreadsNumber)
        {
            if (ProcessingThreadsNumber <= 0) throw new ArgumentException("Uncorrect threads number!");
            cancellationTokenSource = new CancellationTokenSource();
            MainProcessingQuenue = new ConcurrentQueue<PhaseImage>();
            ImageProcessors = new Thread[ProcessingThreadsNumber ];
            for (int i = 0; i < ProcessingThreadsNumber; i++)
            {
                ImageProcessors[i] = new Thread(new ParameterizedThreadStart(ProcessImage));
                ImageProcessors[i].Start(cancellationTokenSource.Token);
            }
        }

        public void ProcessImage(object cancellationToken)
        {
            Unwrapping3 uwr = null;
            CancellationToken token = (CancellationToken)cancellationToken;
            while (!token.IsCancellationRequested)
            {
                while (MainProcessingQuenue.TryDequeue(out PhaseImage result))
                {
                    if (uwr == null) uwr = new Unwrapping3(result.Image);
                    else uwr.UpdateParamsIfNeed(result.Image);
                    result.unwrapper = uwr;
                    result.CalculatePhaseImage();
                    result.Unwrap();
                    result.Process();
                    imagePool.AddImage(result);
                }
                Thread.Sleep(50);
            }

        }

        public bool TryGetImage(out PhaseImage phaseImage)
        {
            return imagePool.TryGetImage(out phaseImage);
        }
    }

}
