using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RecorderCoreTests
{
    [TestClass]
    public class ImageCalculatingTests
    {
        private SettingsContainer settings = new SettingsContainer() { FramePause = 0, MaximumSteps = 4, maxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage };
        static ModelImageCapture capt;
        static PhaseImage BufferPhaseImage = null;
        static int count = 0;
        static double lim = 10;
        static double[,] reserve;
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {

        }
        public static void init()
        {
            capt = new ModelImageCapture(1000, 2000);
            capt.CreateImagesForStepMethod(1, 4);
            while (PhaseImageFactory.phaseImages.Count < 1)
                PhaseImageFactory.AddImage(capt.GetImage());
            BufferPhaseImage = PhaseImageFactory.phaseImages.Dequeue();
            reserve = (double[,])BufferPhaseImage.Image.Clone();
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
          //  capt.Stop();
        }

        [TestMethod] 
        public void PreclisionTest1()
        {
            DateTime dt1 = DateTime.UtcNow;
            double count = 0;
            List<double> temp = new List<double>();
            List<double> temp2 = new List<double>();
            for (double i = -5; i < 5; i++)
            {
                for (int l = 10; l < 15; l++)
                {
                    var for_test = TestImageGenerator.GetTestPair(1000, 2000, Math.PI / 2 * i,l);
                    
                    for_test.Item1.CalculatePhaseImage();
                    DateTime dt2 = DateTime.UtcNow;
                    for_test.Item1.Unwrap();
                    double time = DateTime.UtcNow.Subtract(dt2).TotalSeconds;
                    temp.Add(time);
                    ImageSource.subtract_min(for_test.Item1.Image);
                    ImageSource.subtract_min(for_test.Item2);
                    double res1 = ImageSource.std(for_test.Item1.Image, for_test.Item2);
                    Assert.IsTrue(res1 < Math.PI / 100);
                    count++;
                }

            }
            double res = count/DateTime.UtcNow.Subtract(dt1).TotalSeconds;

        }

        [TestMethod]
        public void PreclisionTest2()
        {
            DateTime dt1 = DateTime.UtcNow;
            double count = 0;
            List<double> temp = new List<double>();
            List<double> temp2 = new List<double>();
            for (double i = -5; i < 5; i++)
            {
                for (int l = 10; l < 15; l++)
                {
                    var for_test = TestImageGenerator.GetTestPair(1000, 2000, Math.PI / 2 * i, l);

                    for_test.Item1.CalculatePhaseImage();
                    DateTime dt2 = DateTime.UtcNow;
                    Unwrapping.Unwrap(for_test.Item1.Image);
                    //for_test.Item1.Unwrap();
                    double time = DateTime.UtcNow.Subtract(dt2).TotalSeconds;
                    temp.Add(time);
                    ImageSource.subtract_min(for_test.Item1.Image);
                    ImageSource.subtract_min(for_test.Item2);
                    double res1 = ImageSource.std(for_test.Item1.Image, for_test.Item2);
                    Assert.IsTrue(res1 < Math.PI / 100);
                    count++;
                }

            }
            double res = count / DateTime.UtcNow.Subtract(dt1).TotalSeconds;

        }
    }
}