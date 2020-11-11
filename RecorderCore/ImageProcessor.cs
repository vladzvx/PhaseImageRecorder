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

    public abstract class PhaseImage
    {

        internal static class NativeMethods
        {
            [DllImport(@"uwr.dll", EntryPoint = "unwrap2D")]
            internal static extern void unwrap(
                IntPtr wrappedImagePointer, IntPtr unwrappedImagePointer, IntPtr maskPointer, int image_width, int image_height,
                  int wrap_around_x, int wrap_around_y, char user_seed, uint seed);

        }

        #region fields
        public static double[,] _GetArrayFromMat(Mat mat, bool Dispose = true)
        {
            Image<Rgb, double> image = mat.ToImage<Rgb, double>();
            int Dim0 = image.Data.GetUpperBound(0) + 1;
            int Dim1 = image.Data.GetUpperBound(1) + 1;
            double[,]  ForReturn = new double[Dim0, Dim1];
            for (int i = 0; i < Dim0; i++)
            {
                for (int j = 0; j < Dim1; j++)
                {
                    ForReturn[i, j] = image.Data[i, j, 0];
                }
            }
            if (Dispose)
            {
                mat.Dispose();
                image.Dispose();
            }
            return ForReturn;
        }
        public void GetArrayFromMat(Mat mat,bool Dispose=true)
        {
            Image<Rgb, double> image = mat.ToImage<Rgb, double>();
            int Dim0 = image.Data.GetUpperBound(0) + 1;
            int Dim1 = image.Data.GetUpperBound(1) + 1;
            Image = new double[Dim0, Dim1];
            ImageForUI = new byte[Dim0, Dim1, image.Data.GetUpperBound(2) + 1];
            for (int i = 0; i < Dim0; i++)
            {
                for (int j = 0; j < Dim1; j++)
                {
                    Image[i, j] = image.Data[i, j, 0];
                    ImageForUI[i, j,0] = (byte)image.Data[i, j, 0];
                    ImageForUI[i, j,1] = (byte)image.Data[i, j, 1];
                    ImageForUI[i, j,2] = (byte)image.Data[i, j, 2];
                }
            }
            if (Dispose)
            {
                mat.Dispose();
                image.Dispose();
            }

        }
        public void GetArrayFromMat(double[,] image)
        {
            int Dim0 = image.GetUpperBound(0) + 1;
            int Dim1 = image.GetUpperBound(1) + 1;
            Image = image;
            ImageForUI = new byte[Dim0, Dim1, 3];
            for (int i = 0; i < Dim0; i++)
            {
                for (int j = 0; j < Dim1; j++)
                {
                    ImageForUI[i, j, 0] = (byte)image[i, j];
                    ImageForUI[i, j, 1] = (byte)image[i, j];
                    ImageForUI[i, j, 2] = (byte)image[i, j];
                }
            }
        }
        public SettingsContainer.ProcessingStep status { get; private set; }
        public SettingsContainer.ProcessingStep MaxProcessingStep { get; set; }
        public DateTime RecordingTime { get; private set; }
        public double[,] Image { get; internal set; }
        public byte[,,] ImageForUI { get; internal set; }
        public Bitmap bitmap { get; internal set; }
        #endregion
        public PhaseImage(Mat image)
        {
            RecordingTime = DateTime.UtcNow;
            status = SettingsContainer.ProcessingStep.Interferogramm;
            GetArrayFromMat(image, true);
        }
        public PhaseImage(double[,] image)
        {
            RecordingTime = DateTime.UtcNow;
            status = SettingsContainer.ProcessingStep.Interferogramm;
            GetArrayFromMat(image);
        }

        public virtual void CalculatePhaseImage()
        {
            if (status <= SettingsContainer.ProcessingStep.Interferogramm)
            {

                status = SettingsContainer.ProcessingStep.WrappedPhaseImage;
            }

        }
        public virtual void Unwrap()
        {
            if (MaxProcessingStep < SettingsContainer.ProcessingStep.UnwrappedPhaseImage) return;
            double[,] matrix = new double[Image.GetUpperBound(0) + 1, Image.GetUpperBound(1) + 1];
            byte[,] mask = new byte[Image.GetUpperBound(0) + 1, Image.GetUpperBound(1) + 1];
            NativeMethods.unwrap(Marshal.UnsafeAddrOfPinnedArrayElement(Image, 0),
                Marshal.UnsafeAddrOfPinnedArrayElement(matrix, 0),
                Marshal.UnsafeAddrOfPinnedArrayElement(mask, 0), Image.GetUpperBound(1) + 1, Image.GetUpperBound(0) + 1, 0, 0, (char)0, (uint)1);
            double max1 = 0;
            double max = 0;
            double min1 = 0;
            double min = 0;
            for (int i = 0; i <= Image.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= Image.GetUpperBound(1); j++)
                {
                    double val1 = matrix[i, j];
                    double val2 = Image[i, j];
                    if (val1 < min) min = val1;
                    if (val1 > max) max = val1;
                    if (val2 < min1) min1 = val2;
                    if (val2 > max1) max1 = val2;

                }
            }
            for (int i = 0; i <= Image.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= Image.GetUpperBound(1); j++)
                {
                    double val1 = matrix[i, j];
                    ImageForUI[i, j, 0] = (byte)(255 * (val1-min)/(max-min));
                    ImageForUI[i, j, 1] = (byte)(255 * (val1 - min) / (max - min));
                    ImageForUI[i, j, 2] = (byte)(255 * (val1 - min) / (max - min));
                }
            }
            Image = matrix;
            if (status <= SettingsContainer.ProcessingStep.WrappedPhaseImage)
            {

                status = SettingsContainer.ProcessingStep.UnwrappedPhaseImage;
            }
        }
        public virtual void Process()
        {
            if (status <= SettingsContainer.ProcessingStep.UnwrappedPhaseImage)
            {

                status = SettingsContainer.ProcessingStep.ProcessedImage;
            }

        }
    }

    public class StepPhaseImage : PhaseImage
    {

        public int StepNumber { get; set; } = 0;
        private List<double[,]> images;
        private List<DateTime> times;
        public StepPhaseImage(Mat image):base(image)
        {
            images = new List<double[,]>() { };
            times = new List<DateTime>() { };
            StepNumber++;
        }

        public StepPhaseImage(double[,] image) : base(image)
        {
            images = new List<double[,]>() { };
            times = new List<DateTime>() { };
            StepNumber++;
        }


        public void AddStep(Mat image)
        {
            images.Add(_GetArrayFromMat(image, true));
            times.Add(DateTime.UtcNow);
            StepNumber++;
        }

        public void AddStep(double[,] image)
        {
            images.Add(image);
            times.Add(DateTime.UtcNow);
            StepNumber++;
        }

        public override void CalculatePhaseImage()
        {
            //np.arctan((image1-2*image2+image3)/(image1-image3))
            if (StepNumber == 3)
            {
                for (int i = 0; i <= Image.GetUpperBound(0); i++)
                {
                    for (int j = 0; j <= Image.GetUpperBound(1); j++)
                    {
                        double val1 = Math.Atan((Image[i, j] - 2 * images[0][i, j] + images[1][i, j]) / (Image[i, j] - images[1][i, j]));
                        Image[i, j] = val1;
                        ImageForUI[i, j,0] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        ImageForUI[i, j,1] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        ImageForUI[i, j,2] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                    }
                }
                images = new List<double[,]>();
            }
            else if (StepNumber == 4)
            {
                //np.arctan((image4 - image2) / (image1 - image3))
                for (int i = 0; i <= Image.GetUpperBound(0); i++)
                {
                    for (int j = 0; j <= Image.GetUpperBound(1); j++)
                    {
                        double val1 = Math.Atan((images[2][i, j] - images[0][i, j]) / (Image[i, j] - images[1][i, j]));
                        Image[i, j] = val1;
                        ImageForUI[i, j,0] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        ImageForUI[i, j,1] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        ImageForUI[i, j,2] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                    }
                }

            }
        }
    }

    public class HilbertPhaseImage : PhaseImage
    {
        public HilbertPhaseImage(Mat image) : base(image)
        {

        }
    }

    public class CameraImage : PhaseImage
    {
        public CameraImage(Mat image) : base(image)
        {

        }
    }
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
                        //SecondaryProcessingQuenue.Enqueue(result);
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
