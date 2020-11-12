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

    public class ImageContainer
    {
        public double[,] image;
    }

    public class ArduinoWorker
    {
        public void Action(int act)
        {

        }
    }
    public class SettingsContainer
    {
        public enum RecordingType
        {
            Camera,
            Hilbert,
            Step
        }

        public enum ProcessingStep
        {
            Interferogramm,
            WrappedPhaseImage,
            UnwrappedPhaseImage,
            ProcessedImage

        }


        public RecordingType recordingType = RecordingType.Camera;
        public ProcessingStep maxProcessingStep = ProcessingStep.Interferogramm;
        public int MaximumSteps =3;
        public int Camera =0;
        public int FramePause =0;


        public bool AreEqual(SettingsContainer settings)
        {
            return this.maxProcessingStep == settings.maxProcessingStep || this.recordingType == settings.recordingType || this.MaximumSteps == settings.MaximumSteps;
        }
    }

    public class RecordingDriver
    {
        public delegate void PhaseImageReciever(PhaseImage phaseImage);
        private object settingsLocker = new object();
        private ArduinoWorker ArduinoWorker;
        private SettingsContainer settings;
        private ImageCapture imageCapture ;
        private ModelImageCapture imageCapture2 ;

        private ImageProcessor imageProcessor;
        private object locker = new object();
        private PhaseImage BufferPhaseImage;

        public void AddImageReciever(PhaseImageReciever phaseImageReciever)
        {
            this.imageProcessor.PhaseImageInterfaceSender += phaseImageReciever;
        }
        public RecordingDriver(int processingThreadNumber = 3)
        {
            ArduinoWorker = new ArduinoWorker();
            settings = new SettingsContainer();
            imageCapture = new ImageCapture();
            imageCapture2 = new ModelImageCapture();
            //imageCapture.Start();
            imageProcessor = new ImageProcessor(processingThreadNumber);
            imageCapture.rec += AddImage;
            imageCapture2.rec += AddImage;
            imageCapture.action += ArduinoWorker.Action;
            imageCapture2.Start();
        }
        public void UpdateSettings(SettingsContainer settings)
        {
            lock (settingsLocker)
            {
                if (this.settings.AreEqual(settings))
                {
                    this.settings = settings;
                    ApplySettings();
                }
            }
                
        }

        private void ApplySettings()
        {
            imageCapture.UpdateCamera(settings.Camera);
            imageCapture.UpdateFramePause(settings.FramePause);
            imageCapture.UpdateMaxFrameCounter(settings.MaximumSteps);
        }
        private void AddImage(Mat image)
        {
            lock (locker)
            {
                if (settings.recordingType == SettingsContainer.RecordingType.Camera)
                {
                    BufferPhaseImage = new CameraImage(image) { MaxProcessingStep = settings.maxProcessingStep };
                    imageProcessor.PutImage(BufferPhaseImage);
                }
                else if (settings.recordingType == SettingsContainer.RecordingType.Hilbert)
                {
                    BufferPhaseImage = new HilbertPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep };
                    imageProcessor.PutImage(BufferPhaseImage);
                }
                else if (settings.recordingType == SettingsContainer.RecordingType.Step)
                {
                    StepPhaseImage stepPhaseImage = BufferPhaseImage as StepPhaseImage;
                    if (stepPhaseImage != null)
                    {
                        if (stepPhaseImage.StepNumber < settings.MaximumSteps)
                            stepPhaseImage.AddStep(image);
                        else
                        {
                            imageProcessor.PutImage(BufferPhaseImage);
                            BufferPhaseImage = new StepPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep }; ;
                        }
                    }
                    else
                    {
                        BufferPhaseImage = new StepPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep }; ;
                    }
                }
            }
        }

        private void AddImage(double[,] image)
        {
            lock (locker)
            {
                if (settings.recordingType == SettingsContainer.RecordingType.Step)
                {
                    StepPhaseImage stepPhaseImage = BufferPhaseImage as StepPhaseImage;
                    if (stepPhaseImage != null)
                    {
                        if (stepPhaseImage.StepNumber < settings.MaximumSteps)
                            stepPhaseImage.AddStep(image);
                        else
                        {
                            imageProcessor.PutImage(BufferPhaseImage);
                            BufferPhaseImage = new StepPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep }; ;
                        }
                    }
                    else
                    {
                        BufferPhaseImage = new StepPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep }; ;
                    }
                }

            }
        }

    }

}
