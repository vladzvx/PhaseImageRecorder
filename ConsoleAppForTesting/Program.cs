using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppForTesting
{
    class Program
    {

        static void Main(string[] args)
        {
             var q = Element.CreateArray(4000000);
            Element[] elements = q.Item1;
            double[] d = q.Item2;
            //Element[] elements = Element.CreateArray(21);
            int NIters = 30;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(d, elements);
                Array.Sort(d);
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                //Sortings.HibridSort(elements,4);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                //if (!Sortings.SortingChecker(elements)) throw new Exception();
                Element.Refresh(elements,d);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }
    }
}
