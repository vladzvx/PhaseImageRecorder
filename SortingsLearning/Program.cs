using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortingsLearning
{
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
    class Sortings
    {
        #region MergeSort

        private static void Merge(double[] keys, Element[] array, int lowIndex, int middleIndex, int highIndex)
        {
            var left = lowIndex;
            var right = middleIndex + 1;
            Element[] tempArray = new Element[highIndex - lowIndex + 1];
            double[] tempArray2 = new double[highIndex - lowIndex + 1];
            var index = 0;

            while ((left <= middleIndex) && (right <= highIndex))
            {
                if (keys[left] < keys[right])
                {
                    tempArray[index] = array[left];
                    tempArray2[index] = keys[left];
                    left++;
                }
                else
                {
                    tempArray[index] = array[right];
                    tempArray2[index] = keys[right];
                    right++;
                }

                index++;
            }

            for (var i = left; i <= middleIndex; i++)
            {
                tempArray[index] = array[i];
                tempArray2[index] = keys[i];
                index++;
            }

            for (var i = right; i <= highIndex; i++)
            {
                tempArray[index] = array[i];
                tempArray2[index] = keys[i];
                index++;
            }

            for (var i = 0; i < tempArray.Length; i++)
            {
                array[lowIndex + i] = tempArray[i];
                keys[lowIndex + i] = tempArray2[i];
            }
        }


        //сортировка слиянием
        public static void MergeSort(double[] keys, Element[] array, int lowIndex, int highIndex)
        {
            if (lowIndex < highIndex)
            {
                var middleIndex = (lowIndex + highIndex) / 2;
                MergeSort(keys, array, lowIndex, middleIndex);
                MergeSort(keys, array, middleIndex + 1, highIndex);
                Merge(keys, array, lowIndex, middleIndex, highIndex);
            }
        }



        #endregion

        #region copypasted quicksort
        private static int partition(double[] keys, Element[] array, int start, int end)
        {
            Element temp;
            double temp1;
            int marker = start;
            for (int i = start; i < end; i++)
            {
                if (keys[i] < keys[end])
                {
                    temp1 = keys[marker];
                    temp = array[marker];
                    array[marker] = array[i];
                    keys[marker] = keys[i];
                    array[i] = temp;
                    keys[i] = temp1;
                    marker += 1;
                }
            }

            temp = array[marker];
            temp1 = keys[marker];
            array[marker] = array[end];
            keys[marker] = keys[end];
            array[end] = temp;
            keys[end] = temp1;
            return marker;
        }

        public static void CopyPastedQuickSort(double[] keys, Element[] array)
        {
            CopyPastedQuickSort(keys, array, 0, array.Length - 1);
        }

        private static void CopyPastedQuickSort(double[] keys, Element[] array, int start, int end)
        {
            if (start >= end)
            {
                return;
            }
            int pivot = partition(keys, array, start, end);
            CopyPastedQuickSort(keys, array, start, pivot - 1);
            CopyPastedQuickSort(keys, array, pivot + 1, end);
        }

        #endregion
        public static void MyQuickSort(double[] keys, Element[] array, int start, int end)
        {
            if (end - start < 1)
            {
                return;
            }
            double[] support_array = new double[end - start + 1];
            Element[] support_array2 = new Element[end - start + 1];
            int support = (start + end) / 2;

            SwapIfGreater(keys, array, start, support);
            SwapIfGreater(keys, array, start, end);
            SwapIfGreater(keys, array, support, end);


            List<int> lower = new List<int>();
            List<int> higher = new List<int>();
            List<int> equals = new List<int>();

            double support_value = keys[support];
            for (int i = start; i <= end; i++)
            {
                if (keys[i] < support_value)
                {
                    lower.Add(i);
                }
                else if (keys[i] > support_value)
                {
                    higher.Add(i);
                }
                else
                {
                    equals.Add(i);
                }
            }
            int k_l = 0;
            foreach (int l in lower)
            {
                support_array[k_l] = keys[l];
                support_array2[k_l] = array[l];
                k_l++;
            }
            int k_e = lower.Count;
            foreach (int e in equals)
            {
                support_array[k_e] = keys[e];
                support_array2[k_e] = array[e];
                k_e++;
            }
            int k_h = lower.Count + equals.Count;
            foreach (int h in higher)
            {
                support_array[k_h] = keys[h];
                support_array2[k_h] = array[h];
                k_h++;
            }

            for (int i = 0; i < support_array.Length; i++)
            {
                keys[start + i] = support_array[i];
                array[start + i] = support_array2[i];
            }
            MyQuickSort(keys, array, start, start + lower.Count - 1);
            MyQuickSort(keys, array, start + lower.Count + equals.Count, end);
        }

        public static void InsertionSort(Element[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                int j;
                Element buf = array[i];
                for (j = i - 1; j >= 0; j--)
                {
                    if (array[j].ValueToSort < buf.ValueToSort)
                        break;

                    array[j + 1] = array[j];
                }
                array[j + 1] = buf;
            }
        }

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

        public static void ParallelQuickSort(double[] keys, Element[] elements)
        {
            ConcurrentBag<int> _bounds = new ConcurrentBag<int>() { 0 };
            Sortings.ArrayPreprocessing(keys, elements, 0, keys.Length - 1, _bounds, 3, 0);
            List<int> bounds = _bounds.ToList();
            bounds.Add(keys.Length);
            bounds.Sort();
            Parallel.For(0, bounds.Count - 1, (i) => {
                Array.Sort(keys, elements, bounds[i], bounds[i + 1] - bounds[i]);
            });
        }

        public static void ArrayPreprocessing(double[] keys, Element[] elements, int left, int right, ConcurrentBag<int> bounds, int depth_limit = 2, int depth = 0)
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
    class Program
    {

        static void Main(string[] args)
        {
            var q = Element.CreateArrays(4000000);
            Element[] elements = q.Item1;
            double[] d = q.Item2;
            //Element[] elements = Element.CreateArray(21);
            int NIters = 15;
            List<double> forResults = new List<double>(NIters);

            for (int i = 0; i < NIters; i++)
            {
                DateTime dt1 = DateTime.UtcNow;
                //Array.Sort(d, elements);



                Sortings.ParallelQuickSort(d, elements);

                if (i != 0)
                    forResults.Add(DateTime.UtcNow.Subtract(dt1).TotalSeconds);
                if (!Element.CheckSortingResult(elements)) throw new Exception();
                Element.Refresh(elements, d);
            }
            double mean = forResults.Sum() / (double)forResults.Count;
        }
    }
}
