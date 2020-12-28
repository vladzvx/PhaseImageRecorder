using ConsoleAppForTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCoreTests
{
    [TestClass]
    public class SortingTests
    {
        static Element[] elements;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            elements = Element.CreateArray(4000000);

        }

      //  [TestMethod]
        public void Test1_Threads()
        {
            int NIters = 10;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                Sortings.HibridSort(elements, 1);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                Assert.IsTrue(SortingChecker(elements));
                Element.Refresh(elements);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }


        [TestMethod]
        public void Test2_Threads()
        {            
            int NIters = 10;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                Sortings.HibridSort(elements, 2);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                Assert.IsTrue(SortingChecker(elements));
                Element.Refresh(elements);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }

        [TestMethod]
        public void Test3_Threads()
        {
            int NIters = 10;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                Sortings.HibridSort(elements, 3);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                Assert.IsTrue(SortingChecker(elements));
                Element.Refresh(elements);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }


        [TestMethod]
        public void Test4_Threads()
        {
            int NIters = 10;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                Sortings.HibridSort(elements, 4);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                Assert.IsTrue(SortingChecker(elements));
                Element.Refresh(elements);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }

        [TestMethod]
        public void Test5_Threads()
        {
            int NIters = 10;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                Sortings.HibridSort(elements, 5);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                Assert.IsTrue(SortingChecker(elements));
                Element.Refresh(elements);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }


        /// <summary>
        /// Метод для проверки корректности сортировки
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        internal static bool SortingChecker(Element[] elements)
        {
            for (int i = 0; i < elements.Length - 1; i++)
            {
                if (elements[i].ValueToSort > elements[i + 1].ValueToSort)
                    return false;
            }
            return true;
        }

    }
}
