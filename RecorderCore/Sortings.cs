using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    class Sortings
    {

        private static void SwapIfGreater(double[] keys, Unwrapping3.edge[] elements, int a, int b)
        {
            if (a != b)
            {
                if (keys[a] > keys[b])
                {
                    Unwrapping3.edge el = elements[a];
                    double key = keys[a];
                    keys[a] = keys[b];
                    keys[b] = key;
                    elements[a] = elements[b];
                    elements[b] = el;
                }
            }
        }

        public static void ParallelQuickSort(double[] keys, Unwrapping3.edge[] elements)
        {
            ConcurrentBag<int> _bounds = new ConcurrentBag<int>() { 0 };
            Sortings.ArrayPreprocessing(keys, elements, 0, keys.Length - 1, _bounds, 2, 0);
            List<int> bounds = _bounds.ToList();
            bounds.Add(keys.Length);
            bounds.Sort();
            Parallel.For(0, bounds.Count - 1, (i) => {
                Array.Sort(keys, elements, bounds[i], bounds[i + 1] - bounds[i]);
            });
        }

        public static void ArrayPreprocessing(double[] keys, Unwrapping3.edge[] elements, 
            int left, int right, ConcurrentBag<int> bounds, int depth_limit = 2, int depth = 0)
        {

            if (depth > depth_limit) return;
            do
            {
                int i = left;
                int j = right;
                int middle = i + ((j - i) >> 1);
                SwapIfGreater(keys, elements, i, middle);
                SwapIfGreater(keys, elements, i, j);
                SwapIfGreater(keys, elements, middle, j);
                double x = keys[middle];
                do
                {
                    while (keys[i] < x) i++;
                    while (x < keys[j]) j--;
                    if (i > j)
                    {
                        break;
                    }
                    if (i < j)
                    {
                        Unwrapping3.edge el = elements[i];
                        double key = keys[i];
                        keys[i] = keys[j];
                        keys[j] = key;
                        elements[i] = elements[j];
                        elements[j] = el;
                    }
                    i++;
                    j--;
                } while (i <= j);
                depth++;
               // Unwrapping2.edge el1 = elements[i];
                //Unwrapping2.edge el2 = elements[j];
                if (depth_limit >= depth)
                {
                    bounds.Add(i);
                    //bounds.Add(j);
                }

                ArrayPreprocessing(keys, elements, left, j, bounds, depth_limit, depth);
                ArrayPreprocessing(keys, elements, i, right, bounds, depth_limit, depth);

                /*
                Task t1 = Task.Factory.StartNew(() =>
                {
                    ArrayPreprocessing(keys, elements, left, j, bounds, depth_limit, depth);
                });

                Task t2 = Task.Factory.StartNew(() =>
                {
                    ArrayPreprocessing(keys, elements, i, right, bounds, depth_limit, depth);
                });
                Task.WaitAll(t1, t2);
                */

                right = j;
                left = i;
            } while (left < right);
        }
    }
}
