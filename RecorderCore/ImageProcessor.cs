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
                    logger.Debug(string.Format("Decuenue image (rec. time: {1}) from processing queue. Total {0} images in queue.", MainProcessingQuenue.Count, result.RecordingTime));
                    if (!MainProcessingQuenue.IsEmpty)
                    {
                        logger.Debug("Using additional threads");
                        if (SecondaryImageProcessors!=null)
                            SecondaryProcessingQuenue.Enqueue(result);
                    }
                    else
                    {
                        logger.Debug("Processing image in main thread additional threads");
                        if (result != null)
                        {
                            logger.Debug(string.Format("Processing image (recorded at {0})", result.RecordingTime));
                            result.CalculatePhaseImage();
                            result.Unwrap();
                            result.Process();
                            if (PhaseImageInterfaceSender != null)
                                PhaseImageInterfaceSender.Invoke(result);
                            if (PhaseImageSender != null)
                                PhaseImageSender.Invoke(result);
                        }
                        else
                        {
                            logger.Debug("No images in queue");
                        }
                    }
                }

                Thread.Sleep(50);
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





}
