using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RecorderCore.Modeling
{
    [TestClass]
    public class ModelImageCaptureTests
    {
        private SettingsContainer settings = new SettingsContainer() { FramePause = 0, MaximumSteps = 4, maxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage };
        static ModelImageCapture capt;
        PhaseImage BufferPhaseImage = null;
        static int count = 0;
        static double lim = 10;
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
        }
        public static void init()
        {
            capt = new ModelImageCapture(1000, 2000);
            while (PhaseImageFactory.phaseImages.Count<=lim)
                PhaseImageFactory.AddImage(capt.GetImage());
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            capt.Stop();
        }

        [TestMethod] 
        public void FPS_test1()
        {
            init();

            DateTime dt1 = DateTime.UtcNow;
            PhaseImageFactory.Calc();
            double ts = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = lim / ts;

        }
    }
}