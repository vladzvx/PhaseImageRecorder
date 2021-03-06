﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RecorderCore.ImageSource;

namespace SmoothTests
{
    [TestClass]
    public class DeltrendTrendTests
    {
        [TestMethod]
        public void Test1()
        {
            double[,] test1 = ImageSource.GetPlane(100, 4024, 10);
            double[,] test2 = ImageSource.GetTrendPlane(test1);

            double mean1 = ImageSource.mean(test1);
            double min1 = ImageSource.min(test1);
            double max1 = ImageSource.max(test1);

            double mean2 = ImageSource.mean(test2);
            double min2 = ImageSource.min(test2);
            double max2 = ImageSource.max(test2);


            double[,] diff = ImageSource.diff(test1, test2);
            double mean3 = ImageSource.mean(diff);

            double min3 = ImageSource.min(diff);
            double max3 = ImageSource.max(diff);

        }
    }

    [TestClass]
    public class SmoothTest1
    {
        static int size0 = 2048;
        static int size1 = 1024;
        static double[,] _matrix = new double[size0, size1];
        [TestMethod]
        public void Test1()
        {
            double[,] matrix0 = new double[size0, size1];
            double[,] matrix1 = new double[size0, size1];
            ImageSource.AddNoise(matrix1,1);
            double q1 = ImageSource.std(matrix0,matrix1);
            ImageSource.smooth(matrix1, 3);
            double q2 = ImageSource.std(matrix0, matrix1);
        }

        [TestMethod]
        public void Test2()
        {
            double[,] matrix0 = new double[size0, size1];
            double[,] matrix1 = new double[size0, size1];
            ImageSource.AddNoise(matrix1, 1);
            double q1 = ImageSource.std(matrix0, matrix1);
            ImageSource.smooth(matrix1, 5);
            double q2 = ImageSource.std(matrix0, matrix1);
        }

        [TestMethod]
        public void Test3()
        {
            ImageSource.smooth(_matrix, 3);
        }
    }
}
