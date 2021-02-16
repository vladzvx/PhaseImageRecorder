﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RecorderCore.Unwrapping;
using RecorderCore.Images;

namespace RecorderCore
{
    [TestClass]
    public class FFT_tests
    {

        [TestMethod]
        public void FFT2_test1()
        {
            var tuple = TestImageGenerator.GetTestPair(1024, 1024, 4);
            Complex[,] test_image = Complex.CreateComplexArray(tuple.Item1.images[0]);
            FourierTransform.FFT2(test_image, FourierTransform.Direction.Forward);
            Complex[,] reserv = (Complex[,])test_image.Clone();
            FourierTransform.FFT2(test_image, FourierTransform.Direction.Backward);
            double[,] inverse_result  = Complex.CreateDoubleArray(test_image);
            double std = ImageSource.std(tuple.Item1.images[0], inverse_result);
            Assert.IsTrue(std< ImageSource.mean(tuple.Item1.images[0])/1000);
        }

        [TestMethod]
        public void FFT2_test2()
        {
            var tuple = TestImageGenerator.GetTestPair(1024, 2048, 4);
            //DateTime dt1 = DateTime.UtcNow;
            Complex[,] test_image = Complex.CreateComplexArray(tuple.Item1.images[0]);
            DateTime dt1 = DateTime.UtcNow;
            FourierTransform.FFT2(test_image, FourierTransform.Direction.Forward);
            Complex[,] reserv = (Complex[,])test_image.Clone();
            FourierTransform.FFT2(test_image, FourierTransform.Direction.Backward);
            double[,] inverse_result = Complex.CreateDoubleArray(test_image);
            double resss = dt1.Subtract(DateTime.UtcNow).TotalSeconds;

            double std = ImageSource.std(tuple.Item1.images[0], inverse_result);
            Assert.IsTrue(std < ImageSource.mean(tuple.Item1.images[0]) / 1000);
        }


    }

    [TestClass]
    public class HilbertTests
    {

        [TestMethod]
        public void HilbertTests1()
        {
            int size0 = 1024;
            int size1 = 1024;
            var tuple = TestImageGenerator.GetTestPair2(size0, size1, 4);

            HilbertPhaseImage2 hpi = new HilbertPhaseImage2(tuple.Item1, 0, 1);

            hpi.Convert();
            hpi.Calc();
            hpi.Unwrapp();
            hpi.ReverseConvert();
            

            double _min = ImageSource.min(tuple.Item2);
            double _max = ImageSource.max(tuple.Item2);
            double _mean = ImageSource.mean(tuple.Item2);


            double min = ImageSource.min(hpi.images[0]);
            double max = ImageSource.max(hpi.images[0]);
            double mean = ImageSource.mean(hpi.images[0]);
            double std = ImageSource.std(tuple.Item2, hpi.images[0]);
            Assert.IsTrue(std < ImageSource.mean(tuple.Item2) / 1000);
        }

    }

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
        /*
        [TestMethod]
        public void PreclisionTest1()
        {
            DateTime dt1 = DateTime.UtcNow;
            double count = 0;
            List<double> temp = new List<double>();
            List<double> temp2 = new List<double>();
            for (double i = -1; i < 1; i++)
            {
                for (int l = 2; l < 4; l++)
                {
                    var for_test = TestImageGenerator.GetTestPair(1000, 2000, Math.PI / 2 * i, l);

                    for_test.Item1.CalculatePhaseImage();
                    DateTime dt2 = DateTime.UtcNow;
                    for_test.Item1.Unwrap();
                    double time = DateTime.UtcNow.Subtract(dt2).TotalSeconds;
                    temp.Add(time);
                    ImageSource.subtract_min(for_test.Item1.Image);
                    ImageSource.subtract_min(for_test.Item2);
                    double[,] difference = ImageSource.diff(for_test.Item1.Image, for_test.Item2);
                    var q = ImageSource.GetNoZero(difference, 2);
                    double res1 = ImageSource.std(for_test.Item1.Image, for_test.Item2);
                    Assert.IsTrue(res1 < Math.PI / 100);
                    count++;
                }

            }
            double res2 = temp.Sum() / temp.Count();
            double res = count / DateTime.UtcNow.Subtract(dt1).TotalSeconds;

        }
        */
        [TestMethod]
        public void PreclisionTest1()
        {
            DateTime dt1 = DateTime.UtcNow;
            double count = 0;
            List<double> temp = new List<double>();
            List<double> temp2 = new List<double>();
            for (double i = -1; i < 1; i++)
            {
                for (int l = 1; l < 4; l++)
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
                    double[,] difference = ImageSource.diff(for_test.Item1.Image, for_test.Item2);
                    var q = ImageSource.GetNoZero(difference, 2);
                    double res1 = ImageSource.std(for_test.Item1.Image, for_test.Item2);
                    Assert.IsTrue(res1 < Math.PI / 100);
                    count++;
                }

            }
            double res2 = temp.Sum() / temp.Count();
            double res = count / DateTime.UtcNow.Subtract(dt1).TotalSeconds;

        }


        [TestMethod]
        public void PreclisionTest2()
        {
            Unwrapping2 uwr = new Unwrapping2(TestImageGenerator.GetTestPair(1000, 2000, Math.PI / 2 * 1, 10).Item2);

            DateTime dt1 = DateTime.UtcNow;
            double count = 0;
            List<double> temp = new List<double>();
            List<double> temp2 = new List<double>();
            List<UwrReport> temp3 = new List<UwrReport>();
            //Unwrapping2.SetParamsByImage(for_test.Item1.Image);
            for (double i = -1; i < 1; i++)
            {
                for (int l = 2; l < 3; l++)
                {
                    var for_test = TestImageGenerator.GetTestPair(1000, 2000, Math.PI / 2 * i, l);

                    for_test.Item1.CalculatePhaseImage();
                    DateTime dt2 = DateTime.UtcNow;

                    uwr.Unwrap(for_test.Item1.Image, out UwrReport s);
                    temp3.Add(s);
                    //for_test.Item1.Unwrap();
                    double time = DateTime.UtcNow.Subtract(dt2).TotalSeconds;
                    temp.Add(time);
                    ImageSource.subtract_min(for_test.Item1.Image);
                    ImageSource.subtract_min(for_test.Item2);
                    double[,] difference = ImageSource.diff(for_test.Item1.Image, for_test.Item2);
                    var q = ImageSource.GetNoZero(difference, 2);
                    double res1 = ImageSource.std(for_test.Item1.Image, for_test.Item2);
                    temp2.Add(res1);
                    Assert.IsTrue(res1 < Math.PI / 100);
                    count++;
                }

            }
            double res2 = temp.Sum() / temp.Count();
            double res = count / DateTime.UtcNow.Subtract(dt1).TotalSeconds;

        }
    }
    /*
    [TestClass]
    public class UnwrappingTests
    {
        internal bool sortingCheck(List<edge>  edges)
        {
            for (int i = 1; i < edges.Count; i++)
            {
                if (edges[i - 1].reaibility < edges[i].reaibility)
                    return false;
            }
            return true;
        }

        [TestMethod]
        public void EdgdeSortingTest1()
        {
            List<edge> edges = new List<edge>()
            {
                new edge(){reaibility=0},
                new edge(){reaibility=1},
                new edge(){reaibility=1},
                new edge(){reaibility=2},

            };
            edges.Sort(new Unwrapping.edgeComparer());
            Assert.IsTrue(sortingCheck(edges));
        }
        [TestMethod]
        public void EdgdeSortingTest2()
        {
            List<edge> edges = new List<edge>()
            {
                new edge(){reaibility=10},
                new edge(){reaibility=1},
                new edge(){reaibility=12},
                new edge(){reaibility=2},

            };
            edges.Sort(new Unwrapping.edgeComparer());
            Assert.IsTrue(sortingCheck(edges));
        }
    }


    [TestClass]
    public class Unwrapping2Tests
    {
        internal bool sortingCheck(List<Unwrapping2.edge> edges)
        {
            for (int i = 1; i < edges.Count; i++)
            {
                if (edges[i - 1].reaibility < edges[i].reaibility)
                    return false;
            }
            return true;
        }

        [TestMethod]
        public void EdgdeSortingTest1()
        {
            List<Unwrapping2.edge> edges = new List<Unwrapping2.edge>()
            {
                new Unwrapping2.edge(){reaibility=0},
                new Unwrapping2.edge(){reaibility=1},
                new Unwrapping2.edge(){reaibility=1},
                new Unwrapping2.edge(){reaibility=2},

            };
            edges.Sort(new Unwrapping2.edgeComparer());
            Assert.IsTrue(sortingCheck(edges));
        }
        [TestMethod]
        public void EdgdeSortingTest2()
        {
            List<Unwrapping2.edge> edges = new List<Unwrapping2.edge>()
            {
                new Unwrapping2.edge(){reaibility=10},
                new Unwrapping2.edge(){reaibility=1},
                new Unwrapping2.edge(){reaibility=12},
                new Unwrapping2.edge(){reaibility=2},

            };
            edges.Sort(new Unwrapping2.edgeComparer());
            Assert.IsTrue(sortingCheck(edges));
        }
    }

    */
}
