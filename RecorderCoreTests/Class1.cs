using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoothTests
{
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
