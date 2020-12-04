using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{/*
    [TestClass]
    public class BaseCaptureTests2
    {
        static TestCapture capt;

        public static int ActionTimespan = 0;
        public static void testReciever1(double[,] val)
        {
            Task.Delay(ActionTimespan).Wait();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            capt.Stop();
        }

        [TestMethod]
        public void ActionTimeTest1()
        {
            capt = new TestCapture();
            ActionTimespan = 50;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.ActionAverageTimespan <= ActionTimespan + 20 && capt.ActionAverageTimespan>ActionTimespan);

        }

        [TestMethod]
        public void ActionTimeTest2()
        {
            capt = new TestCapture();
            ActionTimespan = 100;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.ActionAverageTimespan <= ActionTimespan + 20 && capt.ActionAverageTimespan > ActionTimespan);

        }
        [TestMethod]
        public void ActionTimeTest3()
        {
            capt = new TestCapture();
            ActionTimespan = 500;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.ActionAverageTimespan <= ActionTimespan + 20 && capt.ActionAverageTimespan > ActionTimespan);

        }


        [TestMethod]
        public void AftercaptureSleepTimeTest1()
        {
            capt = new TestCapture();
            ActionTimespan = 0;
            int AfterCaptureSleepTime = 50;
            capt.imageReciever += testReciever1;
            capt.SetAfterCaptureSleeping(AfterCaptureSleepTime);
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + 20 && capt.CycleAverageTimespan> AfterCaptureSleepTime);

        }



        [TestMethod]
        public void AftercaptureSleepTimeTest2()
        {
            capt = new TestCapture();
            ActionTimespan = 0;
            int AfterCaptureSleepTime = 100;
            capt.imageReciever += testReciever1;
            capt.SetAfterCaptureSleeping(AfterCaptureSleepTime);
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + 20 && capt.CycleAverageTimespan > AfterCaptureSleepTime);

        }


        [TestMethod]
        public void CommonSleepingTest1()
        {
            capt = new TestCapture();
            ActionTimespan = 100;
            int AfterCaptureSleepTime = 100;
            capt.imageReciever += testReciever1;
            capt.SetAfterCaptureSleeping(AfterCaptureSleepTime);
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + ActionTimespan  + 30 && capt.CycleAverageTimespan > AfterCaptureSleepTime+ ActionTimespan);

        }

        [TestMethod]
        public void CommonSleepingTest2()
        {
            capt = new TestCapture();
            ActionTimespan = 50;
            int AfterCaptureSleepTime = 150;
            capt.imageReciever += testReciever1;
            capt.SetAfterCaptureSleeping(AfterCaptureSleepTime);
            capt.Start();
            Thread.Sleep(5000);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + ActionTimespan + 30 && capt.CycleAverageTimespan > AfterCaptureSleepTime + ActionTimespan);

        }
    }
    */
}
