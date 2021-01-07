using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RecorderCore.Modeling
{


    [TestClass]
    public class ImageSourceTests
    {
        static ImageSource imageSource = new ImageSource(300, 300);
        static int imageNumber = 10;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            imageSource.CreateImagesForHilbert(imageNumber);
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void ImageInfIterNumberTest1()
        {
            double count = 0;
            while (true)
            {
                var im = imageSource.GetNextImage();
                Assert.IsNotNull(im);
                count++;
                if (count > 2* imageNumber)
                    break;
            }
        }

        [TestMethod]
        public void ImageInfIterNumberTest2()
        {
            double count = 0;
            List<double[,]> images1 = new List<double[,]>();
            List<double[,]> images2 = new List<double[,]>();
            while (true)
            {
                var im = imageSource.GetNextImage();
                Assert.IsNotNull(im);
                images1.Add(im);
                images2.Add(im);
                count++;
                if (count > 3* imageNumber)
                    break;
            }
            int count2 = 0;
            foreach (var im in images1)
            {
                if (images2.RemoveAll(item => ImageSource.AreEqual(item, im)) > 0)
                {
                    count2++;
                }
            }
            Assert.IsTrue(count2 <= imageNumber);
        }


    }


}
