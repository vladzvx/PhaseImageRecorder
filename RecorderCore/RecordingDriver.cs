using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecorderCore
{
    public class RecordingDriver
    {
        public delegate void PhaseImageReciever(PhaseImage phaseImage);
        private object settingsLocker = new object();
        private ArduinoWorker ArduinoWorker;
        private SettingsContainer settings;
        private ImageCapture imageCapture;
        private ModelImageCapture imageCapture2;
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
            imageProcessor = new ImageProcessor(processingThreadNumber);
            imageCapture.rec += AddImage;
            imageCapture2.rec += AddImage;
            imageCapture.action += ArduinoWorker.Action;
            //imageCapture.Start();
            imageCapture2.Pause();
            imageCapture2.Start();
        }
        public void UpdateSettings(SettingsContainer settings)
        {
            lock (settingsLocker)
            {
                //if (this.settings.AreEqual(settings))
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
            if (settings.model)
            {
                imageCapture.Pause();
                imageCapture2.PauseRelease();
            }
            else
            {
                imageCapture2.Pause();
                imageCapture.PauseRelease();
                
            }
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
                else if (settings.recordingType == SettingsContainer.RecordingType.Camera)
                {
                    BufferPhaseImage = new CameraImage(image);
                    imageProcessor.PutImage(BufferPhaseImage);
                }
                else if (settings.recordingType == SettingsContainer.RecordingType.Hilbert)
                {
                    BufferPhaseImage = new HilbertPhaseImage(image);
                    imageProcessor.PutImage(BufferPhaseImage);
                }

            }
        }

    }
}
