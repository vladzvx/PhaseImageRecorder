using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleAppForTesting
{
    class Program
    {

        static void Main(string[] args)
        {
            Element[] elements = Element.CreateArray(4000000);
            //Element[] elements = Element.CreateArray(21);
            int NIters = 10;
            List<double> forResults = new List<double>(NIters);
            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(elements, new ElementComparer());
                //Sortings.InsertionSort(elements);
                Sortings.HibridSort(elements,2);
                //Sortings.QuickSort(elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                if (!Sortings.SortingChecker(elements)) throw new Exception();
                Element.Refresh(elements);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }
    }
}
