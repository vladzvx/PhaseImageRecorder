using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            int NIters = 15;
            List<double> forResults = new List<double>(NIters);
            List<Sortings.Report> reports = new List<Sortings.Report>(NIters);
            ArraySortHelper<double> sh = new ArraySortHelper<double>();
            
            for (int i = 0; i < NIters; i++)
            {
                ConcurrentBag<Report2> bag = new ConcurrentBag<Report2>();
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(d, elements);
                //Array.Sort(d);
                //Array.Sort(elements, new ElementComparer());
                //Sortings.MyQuickSort(d,elements,0,elements.Length-1);
                //Sortings.HibridSort2(d,elements,2);
                
                //
                
                ArraySortHelper< double>.DepthLimitedQuickSort2(d,elements, 0, d.Length - 1);
                //ArraySortHelper< double,Element>.Heapsort2(d,elements, 0, d.Length - 1);
                //ArraySortHelper< double>.Heapsort(d, 0, d.Length - 1,Comparer<double>.Default);
               // sh.De;
                //Sortings.Sort3(d,0,d.Length-1);
                //reports.Add(report);
                //Sortings.QuickSort(d,elements);
                //Sortings.MergeSort(elements);
                forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                if (!Sortings.SortingChecker(elements)) throw new Exception();
                Element.Refresh(elements,d);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }
    }
}
