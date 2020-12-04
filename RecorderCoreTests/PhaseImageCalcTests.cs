using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RecorderCore.Modeling
{


    public class ModelImageCaptureTests
    {
        private SettingsContainer settings = new SettingsContainer() { FramePause = 0, MaximumSteps = 4, maxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage };
        static ModelImageCapture capt;
        PhaseImage BufferPhaseImage = null;
        static int count = 0;
        public static int ActionTimespan = 0;
        public static int SleepingTolerance = 0;//допуск для результатов тестов. Связан с накладными расходами на усыпление потока.
        public static void testReciever1(double[,] val)
        {
            //Assert.IsNotNull(val);
            count++;
            if (ActionTimespan != 0)
                Thread.Sleep(ActionTimespan);
        }
        private void AddImage(double[,] image)
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
                    //    imageProcessor.PutImage(BufferPhaseImage);
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
              //  imageProcessor.PutImage(BufferPhaseImage);
            }
            else if (settings.recordingType == SettingsContainer.RecordingType.Hilbert)
            {
                BufferPhaseImage = new HilbertPhaseImage(image);
               // imageProcessor.PutImage(BufferPhaseImage);
            }
        }


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
        }



        [ClassCleanup]
        public static void ClassCleanup()
        {
            capt.Stop();
        }

        [TestMethod]
        public void FPS_test1()
        {
            count = 0;
            capt = new ModelImageCapture(1000, 2000);
            capt.imageReciever += testReciever1;
            double FPS = 50;
            capt.SetMaxFPS(FPS);
            capt.Start();
            Thread.Sleep(6000);



            count = 0;
        }
    }
}