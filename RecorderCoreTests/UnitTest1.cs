using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RecorderCore.Models.Tests
{
    [TestClass]
    public class StepMethod
    {
        private static int count=0;
        private static void reciever(double[,] d)
        {
            count++;
        }
        [TestMethod]
        public void FPS_test()
        {
            ModelImageCapture modelImageCapture = new ModelImageCapture();

            modelImageCapture.rec += reciever;
            DateTime dt1 = DateTime.UtcNow;

            modelImageCapture.Start();
            System.Threading.Thread.Sleep(30000);

            double t = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double res = count / t;

        }
    }
}
