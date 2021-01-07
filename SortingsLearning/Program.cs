using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortingsLearning
{
    class Sorter
    {
        private static void SwapIfGreater(double[] keys, Element[] elements, int a, int b)
        {
            if (a != b)
            {
                if (keys[a] > keys[b])
                {
                    Element el = elements[a];
                    double key = keys[a];
                    keys[a] = keys[b];
                    keys[b] = key;
                    elements[a] = elements[b];
                    elements[b] = el;
                }
            }
        }

        public static void QuickSort(double[] keys, Element[] elements, int left, int right)
        {

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
                        Element el = elements[i];
                        double key = keys[i];
                        keys[i] = keys[j];
                        keys[j] = key;
                        elements[i] = elements[j];
                        elements[j] = el;
                    }
                    i++;
                    j--;
                } while (i <= j);


                if (j - left <= right - i)
                {
                    if (left < j)
                    {
                        QuickSort(keys, elements, left, j);
                    }
                    left = i;
                }
                else
                {
                    if (i < right)
                    {
                        QuickSort(keys, elements, i, right);
                    }
                    right = j;
                }
            } while (left < right);
        }

        public static void ParallelQuickSort(double[] keys, Element[] elements, int left, int right)
        {

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
                        Element el = elements[i];
                        double key = keys[i];
                        keys[i] = keys[j];
                        keys[j] = key;
                        elements[i] = elements[j];
                        elements[j] = el;
                    }
                    i++;
                    j--;
                } while (i <= j);


                if (left < j&& i < right)
                {
                    Task t1 = Task.Factory.StartNew(()=> 
                    {
                        QuickSort(keys, elements, left, j);
                    });
                    Task t2 = Task.Factory.StartNew(() =>
                    {
                        QuickSort(keys, elements, i, right);
                    });
                    Task.WaitAll(t1, t2);
                }
                right = j;
                left = i;
            } while (left < right);
        }

        public static void ArrayPreprocessing(double[] keys, Element[] elements, int left, int right, List<int> bounds,int depth_limit=2, int depth = 0)
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
                        Element el = elements[i];
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
                Element el1 = elements[i];
                Element el2 = elements[j];
                if (depth_limit >= depth)
                {
                    bounds.Add(i);
                    //bounds.Add(j);
                }

               // Task t1 = 
                //Task.Factory.StartNew(() =>
               // {
                    ArrayPreprocessing(keys, elements, left, j, bounds, depth_limit, depth);
               // });

               // Task t2 =
                //Task.Factory.StartNew(() =>
                //{
                    ArrayPreprocessing(keys, elements, i, right, bounds, depth_limit, depth);
               // });

              //  Task.WaitAll(t1, t2);
                right = j;
                left = i;
            } while (left < right);
        }
    }

    class Element
    {
        #region static
        private static Random rnd = new Random();
        public static void Refresh(Element[] elements, double[] keys)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].Refresh();
                keys[i] = elements[i].ValueToSort;
            }

        }
        public static Tuple<Element[], double[]> CreateArrays(int size)
        {
            Element[] elements = new Element[size];
            double[] doubles = new double[size];
            for (int i = 0; i < size; i++)
            {
                elements[i] = new Element();
                doubles[i] = elements[i].ValueToSort;
            }
            return Tuple.Create(elements, doubles);
        }


        public static bool CheckSortingResult(Element[] elements)
        {
            for (int i = 0; i < elements.Length - 1; i++)
            {
                if (elements[i].ValueToSort > elements[i + 1].ValueToSort)
                    return false;
            }
            return true;
        }

        #endregion

        public double ValueToSort;

        public Element()
        {
            ValueToSort = rnd.NextDouble();
            //ValueToSort = Math.Round(rnd.NextDouble(), 3);
        }
        public void Refresh()
        {
            ValueToSort = rnd.NextDouble();
            //ValueToSort = Math.Round(rnd.NextDouble(), 3);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var arrays = Element.CreateArrays(4000000);
            Element[] elements = arrays.Item1;
            double[] keys = arrays.Item2;
            List<int> bounds = new List<int>();
            DateTime dt2 = DateTime.UtcNow;

            // keys.Min();
            // keys.Max();
            bounds.Add(0);
            


            Sorter.ArrayPreprocessing(keys, elements, 0, keys.Length-1, bounds,3,0);
            double SecondsSpent = DateTime.UtcNow.Subtract(dt2).TotalSeconds;
            bounds.Sort();
            bounds.Add(keys.Length - 1);
            Parallel.For(0, bounds.Count - 1, (i) => {
                Array.Sort(keys,elements)
            });
            if (Element.CheckSortingResult(elements))
                Console.WriteLine("Time spent: " + SecondsSpent.ToString() + " sec");
            else
                Console.WriteLine("Sorting failed!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();


        }
    }
}
