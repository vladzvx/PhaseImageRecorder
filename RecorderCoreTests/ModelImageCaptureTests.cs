using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{
    [TestClass]
    public class ModelImageCaptureTests
    {

        static ModelImageCapture capt;
        static int count = 0;
        public static int ActionTimespan = 0;
        public static int SleepingTolerance = 0;//допуск для результатов тестов. Связан с накладными расходами на усыпление потока.
        public static void testReciever1(double[,] val)
        {
            //Assert.IsNotNull(val);
            count++;
            if (ActionTimespan!=0)
                Thread.Sleep(ActionTimespan);
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            List<double> temp = new List<double>();
            for (int i = 0; i < 200; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                Thread.Sleep(1);
                temp.Add(DateTime.UtcNow.Subtract(dt1).TotalMilliseconds);

            }
            double mean = temp.Max() * 1.2;// temp.Sum() / temp.Count;

            SleepingTolerance = (int)(mean);
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
            capt = new ModelImageCapture(5,5);
            //capt.EnableSpins();
            capt.imageReciever += testReciever1;
            double FPS = 2;
            capt.SetMaxFPS(FPS);
            capt.Start();
            DateTime dt1 = DateTime.UtcNow;
            Thread.Sleep(6000);
            capt.Stop();
            double sec = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = count / sec;
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(Math.Abs(d - FPS) / FPS < 0.1);
            count = 0;
        }

        [TestMethod]
        public void FPS_test2()
        {
            count = 0;
            capt = new ModelImageCapture(5, 5);
            capt.imageReciever += testReciever1;
            double FPS = 5;
            capt.SetMaxFPS(FPS);
            capt.Start();
            DateTime dt1 = DateTime.UtcNow;
            Thread.Sleep(6000);
            capt.Stop();
            double sec = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = count / sec;
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(Math.Abs(d - FPS) / FPS < 0.1);
            count = 0;
        }

        [TestMethod]
        public void FPS_test3()
        {
            count = 0;
            capt = new ModelImageCapture(5, 5);
            capt.imageReciever += testReciever1;
            double FPS = 50;
            capt.SetMaxFPS(FPS);
            capt.Start();
            DateTime dt1 = DateTime.UtcNow;
            Thread.Sleep(6000);
            capt.Stop();
            double sec = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = count / sec;
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(Math.Abs(d - FPS) / FPS < 0.1);
            count = 0;
        }
        [TestMethod]
        public void FPS_test4()
        {
            count = 0;
            capt = new ModelImageCapture(10, 20);
            capt.imageReciever += testReciever1;
            double FPS = 80;
            capt.SetMaxFPS(FPS);
            capt.Start();
            DateTime dt1 = DateTime.UtcNow;
            Thread.Sleep(6000);
            capt.Stop();
            double sec = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = count / sec;
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(Math.Abs(d - FPS) / FPS < 0.1);
            count = 0;
        }

        [TestMethod]
        public void FPS_test5()
        {
            count = 0;
            ActionTimespan = 10;
            capt = new ModelImageCapture(10, 20);
            capt.imageReciever += testReciever1;
            double FPS = 10;
            capt.SetMaxFPS(FPS);
            capt.Start();
            DateTime dt1 = DateTime.UtcNow;
            Thread.Sleep(6000);
            capt.Stop();
            double sec = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = count / sec;
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(Math.Abs(d - FPS) / FPS < 0.1);
            count = 0;
        }


        [TestMethod]
        public void FPS_test6()
        {
            count = 0;
            ActionTimespan = 0;
            capt = new ModelImageCapture(1000, 2000,true);
            capt.imageReciever += testReciever1;
            
            double FPS = 1200;
            capt.SetMaxFPS(FPS);
            capt.Start();
            count = 0;
            DateTime dt1 = DateTime.UtcNow;
            Thread.Sleep(7000);
            capt.Stop();
            double sec = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            double d = count / sec;
            capt.imageReciever -= testReciever1;
            //Assert.IsTrue(Math.Abs(d - FPS) / FPS < 0.1);
            count = 0;
        }
    }

}
