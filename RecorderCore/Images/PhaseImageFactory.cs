using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public static class PhaseImageFactory
    {
        private static object locker = new object();
        private static PhaseImage BufferPhaseImage;
        public static SettingsContainer settings= new SettingsContainer() {recordingType=SettingsContainer.RecordingType.Step, FramePause = 0, MaximumSteps = 4, maxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage };
        public static Queue<PhaseImage> phaseImages = new Queue<PhaseImage>();

        public static void AddImage(double[,] image)
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
                            phaseImages.Enqueue(BufferPhaseImage);
                            BufferPhaseImage = new StepPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep }; ;
                        }
                    }
                    else
                    {
                        BufferPhaseImage = new StepPhaseImage(image) { MaxProcessingStep = settings.maxProcessingStep }; ;
                    }
                }
                else if (settings.recordingType == SettingsContainer.RecordingType.Camera)
                {
                    BufferPhaseImage = new CameraImage(image);
                    phaseImages.Enqueue(BufferPhaseImage);
                }
                else if (settings.recordingType == SettingsContainer.RecordingType.Hilbert)
                {
                    BufferPhaseImage = new HilbertPhaseImage(image);
                    phaseImages.Enqueue(BufferPhaseImage);
                }

            }
        }

        public static void Calc()
        {
            while (phaseImages.Count != 0)
            {
                PhaseImage phaseIm = phaseImages.Dequeue();
                phaseIm.CalculatePhaseImage();
                phaseIm.Unwrap();
                //phaseIm.Process();
            }
        }
    }
}
