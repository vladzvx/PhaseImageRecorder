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
    class TestCapture : BaseCapture
    {


    }
    /// <summary>
    /// Класс для тестирования основного функционала по управлению базовым классом для захвата изображений
    /// </summary>
    [TestClass]
    public class BaseCaptureTests
    {
        static TestCapture capt;
        static int count = 0;
        public static int ActionTimespan = 0;
        public static int SleepingTolerance = 0;//допуск для результатов тестов. Связан с накладными расходами на усыпление потока.
        public static void testReciever1(double[,] val)
        {
            count++;
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
            double mean = temp.Max()*1.2;// temp.Sum() / temp.Count;

            SleepingTolerance = (int)(mean);
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
            Assert.IsTrue(capt.ActionAverageTimespan <= ActionTimespan + SleepingTolerance && capt.ActionAverageTimespan>ActionTimespan);

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
            Assert.IsTrue(capt.ActionAverageTimespan <= ActionTimespan + SleepingTolerance && capt.ActionAverageTimespan > ActionTimespan);

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
            Assert.IsTrue(capt.ActionAverageTimespan <= ActionTimespan + SleepingTolerance && capt.ActionAverageTimespan > ActionTimespan);

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
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + SleepingTolerance && capt.CycleAverageTimespan> AfterCaptureSleepTime);

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
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + SleepingTolerance && capt.CycleAverageTimespan > AfterCaptureSleepTime);

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
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + ActionTimespan  + SleepingTolerance*2 && capt.CycleAverageTimespan > AfterCaptureSleepTime+ ActionTimespan);

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
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + ActionTimespan + SleepingTolerance*2 && capt.CycleAverageTimespan > AfterCaptureSleepTime + ActionTimespan);

        }


        [TestMethod]
        public void SetAfterCaptureSleepingTest()
        {
            capt = new TestCapture();
            ActionTimespan = 50;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(5000);
            capt.Pause();
            Thread.Sleep(200);
            Assert.IsTrue(capt.CycleAverageTimespan <= ActionTimespan + SleepingTolerance && capt.CycleAverageTimespan >  ActionTimespan);
            capt.PauseRelease();
            Thread.Sleep(200);
            int AfterCaptureSleepTime = 150;
            capt.SetAfterCaptureSleeping(AfterCaptureSleepTime);
            Thread.Sleep(10000);
            capt.Stop();
            Assert.IsTrue(capt.CycleAverageTimespan <= AfterCaptureSleepTime + ActionTimespan + SleepingTolerance*2 && 
                Math.Abs(capt.CycleAverageTimespan - (AfterCaptureSleepTime + ActionTimespan) )/ (AfterCaptureSleepTime + ActionTimespan) <0.1 );
            capt.imageReciever -= testReciever1;
        }



        [TestMethod]
        public void StopTest()
        {
            count = 0;
            capt = new TestCapture();
            ActionTimespan = 0;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(100);
            capt.Stop();
            int temp = count;
            Thread.Sleep(200);
            Assert.IsTrue(temp != 0);
            capt.imageReciever -= testReciever1;
            Assert.AreEqual(temp, count);
            count = 0;
        }

        [TestMethod]
        public void StartTest()
        {
            count = 0;
            capt = new TestCapture();
            ActionTimespan = 0;
            capt.imageReciever += testReciever1;
            int temp = count;
            Thread.Sleep(100);
            Assert.AreEqual(temp, count);
            capt.Start();
            Thread.Sleep(100);
            Assert.IsTrue(count != 0);
            Assert.AreNotEqual(temp, count);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            count = 0;
        }

        [TestMethod]
        public void PauseTest()
        {
            count = 0;
            capt = new TestCapture();
            ActionTimespan = 0;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(100);
            Assert.IsTrue(count != 0);
            capt.Pause();
            Thread.Sleep(100);
            int temp = count;
            Thread.Sleep(200);
            Assert.AreEqual(temp, count);
            capt.Stop();
            capt.imageReciever -= testReciever1;
            count = 0;
        }

        [TestMethod]
        public void PauseReleaseTest()
        {
            count = 0;
            capt = new TestCapture();
            ActionTimespan = 0;
            capt.imageReciever += testReciever1;
            capt.Start();
            Thread.Sleep(100);
            Assert.IsTrue(count != 0);
            capt.Pause();
            Thread.Sleep(100);
            int temp = count;
            Thread.Sleep(200);
            Assert.AreEqual(temp, count);
            capt.PauseRelease();
            Thread.Sleep(200);
            Assert.AreNotEqual(temp, count);

            capt.Stop();
            capt.imageReciever -= testReciever1;
            count = 0;
        }

    }
}
