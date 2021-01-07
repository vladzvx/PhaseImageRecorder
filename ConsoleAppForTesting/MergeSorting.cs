using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("RecorderCoreTests")]

namespace ConsoleAppForTesting
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
        public static Tuple<Element[],double[]> CreateArray(int size)
        {
            Element[] elements = new Element[size];
            double[] doubles = new double[size];
            for (int i = 0; i < size; i++)
            {
                elements[i] = new Element();
                doubles[i] = elements[i].ValueToSort;
            }
            return Tuple.Create(elements,doubles);
        }

        #endregion

        public double ValueToSort;

        public Element()
        {
            //ValueToSort = rnd.NextDouble();
            ValueToSort = Math.Round(rnd.NextDouble(),3);
        }
        public void Refresh()
        {
            //ValueToSort = rnd.NextDouble();
            ValueToSort = Math.Round(rnd.NextDouble(), 3);
        }
       
    }

    
    internal class ElementComparer : IComparer<Element>
    {
        public int Compare(Element x, Element y)
        {
            if (x.ValueToSort < y.ValueToSort)
            {
                return -1;
            }
            else if (x.ValueToSort > y.ValueToSort)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }


    internal static class Sortings
    {
        public static void MyQuickSort2(double[] keys, Element[] array, int start, int end)
        {
            //double start_value = keys[start];
            //double end_value = keys[end];
            //while (start< end && keys[start]>= start_value)
            //{
            //    start++;
            //}
            //while (end> start && keys[end] <= end_value)
            //{
            //    end--;
            //}
            if (end - start < 1)
            {
                return;
            }
            double[] support_array = new double[end - start + 1];
            Element[] support_array2 = new Element[end - start + 1];
            int support = (start + end) / 2;

            SwapIfGreater2(keys, array, start, support);
            SwapIfGreater2(keys, array, start, end);
            SwapIfGreater2(keys, array, support, end);


            //ConcurrentBag<int> lower = new ConcurrentBag<int>();
            List<int> lower = new List<int>();
            List<int> higher = new List<int>();
            List<int> equals = new List<int>();

            double support_value = keys[support];
            //foreach (Element el in array)
            //{
            //    Console.Write(el.ValueToSort);
            //    Console.Write("; ");
            //}

            //Console.WriteLine();
            //Console.WriteLine(string.Format("support index: {0}; value: {1}; start: {2}; end: {3}", support, support_value.ValueToSort, start, end));
            for (int i = start; i < end; i++)
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
            //Parallel.For(start, end+1, (i) =>
            //{
            //    if (keys[i] < support_value)
            //    {
            //        lower.Add(i);
            //    }
            //    else if (keys[i] > support_value)
            //    {
            //        higher.Add(i);
            //    }
            //    else
            //    {
            //        equals.Add(i);
            //    }
            //});

            int k_l = 0;
            //Parallel.ForEach(lower,(l)=> { })
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
            //support_array[lower.Count] = keys[support];
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
            /*
            Parallel.Invoke(
                ()=> MyQuickSort(keys,array, start, start+lower.Count-1),
                ()=>MyQuickSort(keys,array, start+lower.Count + equals.Count, end));
            */
            MyQuickSort2(keys, array, start, start + lower.Count - 1);
            MyQuickSort2(keys, array, start + lower.Count + equals.Count, end);
        }

        public static void MyQuickSort(double[] keys,Element[] array,int start,int end)
        {
            //double start_value = keys[start];
            //double end_value = keys[end];
            //while (start< end && keys[start]>= start_value)
            //{
            //    start++;
            //}
            //while (end> start && keys[end] <= end_value)
            //{
            //    end--;
            //}
            if (end - start < 1)
            {
                return;
            }
            double[] support_array = new double[end - start + 1];
            Element[] support_array2 = new Element[end - start + 1];
            int support = (start + end) / 2;

            SwapIfGreater2(keys, array, start, support);
            SwapIfGreater2(keys, array, start, end);
            SwapIfGreater2(keys, array, support, end);


            List<int> lower = new List<int>();
            List<int> higher = new List<int>();
            List<int> equals = new List<int>();
            
            double support_value = keys[support];
            //foreach (Element el in array)
            //{
            //    Console.Write(el.ValueToSort);
            //    Console.Write("; ");
            //}

            //Console.WriteLine();
            //Console.WriteLine(string.Format("support index: {0}; value: {1}; start: {2}; end: {3}", support, support_value.ValueToSort, start, end));
            for (int i = start; i <= end; i++)
            {
                if (keys[i]< support_value)
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
            foreach(int l in lower)
            {
                support_array[k_l] = keys[l];
                support_array2[k_l] = array[l];
                k_l ++;
            }
            int k_e = lower.Count; 
            foreach (int e in equals)
            {
                support_array[k_e] = keys[e];
                support_array2[k_e] = array[e];
                k_e++;
            }
            //support_array[lower.Count] = keys[support];
            int k_h = lower.Count+equals.Count;
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
            /*
            Parallel.Invoke(
                ()=> MyQuickSort(keys,array, start, start+lower.Count-1),
                ()=>MyQuickSort(keys,array, start+lower.Count + equals.Count, end));
            */
            MyQuickSort(keys, array, start, start + lower.Count - 1);
            MyQuickSort(keys, array, start + lower.Count + equals.Count, end);
        }
        private static void SwapIfGreater2(double[] keys, Element[] elements, int a, int b)
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
        public static void MyQuickSort3(double[] keys, Element[] array, int start, int end)
        {
            int i = start;
            int j = end;
            int middle = i + ((j - i) >> 1);
            SwapIfGreater2(keys, array, i, middle);
            SwapIfGreater2(keys, array, i, j);
            SwapIfGreater2(keys, array, middle, j);
            double support_value = keys[middle];

            do
            {
                while (keys[i] < support_value) i++;
                while (support_value < keys[j]) j--;

                if (i < j)//Если между левой и правой границами (уже сужеными) остается зазор - меняем местами левый и правый элементы
                {
                    Element el = array[i];
                    double key = keys[i];
                    keys[i] = keys[j];
                    keys[j] = key;
                    array[i] = array[j];
                    array[j] = el;
                }
                i++;//Делаем шаг дальше вправо
                j--;//Делае дальше влево
            }
            while (i <= j);


            MyQuickSort3(keys, array, start, middle - 1);
            MyQuickSort3(keys, array, middle,end);

        }

        internal static bool SortingChecker(Element[] elements)
        {
            for (int i = 0; i < elements.Length - 1; i++)
            {
                if (elements[i].ValueToSort > elements[i + 1].ValueToSort)
                    return false;
            }
            return true;
        }

        static Element[] tempArray = new Element[4000000];
        static Random rnd = new Random();
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

        #region быстрая сортировка
        private static int partition(Element[] array, int start, int end)
        {
            Element temp;
            int marker = start;
            for (int i = start; i < end; i++)
            {
                if (array[i].ValueToSort < array[end].ValueToSort)
                {
                    temp = array[marker];
                    array[marker] = array[i];
                    array[i] = temp;
                    marker += 1;
                }
            }
            temp = array[marker];
            array[marker] = array[end];
            array[end] = temp;
            return marker;
        }
        private static int partition(double[] keys,Element[] array, int start, int end)
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

        public static void QuickSort(Element[] array)
        {
            QuickSort(array, 0, array.Length-1);
        }

        public static void QuickSort( double[] keys,Element[] array)
        {
            QuickSort(keys,array, 0, array.Length - 1);
        }

        private static void QuickSort(Element[] array, int start, int end)
        {
            if (start >= end)
            {
                return;
            }
            int pivot = partition(array, start, end);
            QuickSort(array, start, pivot - 1);
            QuickSort(array, pivot + 1, end);
        }

        private static void QuickSort(double[] keys, Element[] array, int start, int end)
        {
            if (start >= end)
            {
                return;
            }
            int pivot = partition(keys,array, start, end);
            QuickSort(keys,array, start, pivot - 1);
            QuickSort(keys,array, pivot + 1, end);
        }

        #endregion




        #region сортировка слиянием

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Merge(Element[] array, int lowIndex, int middleIndex, int highIndex)
        {
            var left = lowIndex;
            var right = middleIndex + 1;
            Element[] tempArray = new Element[highIndex - lowIndex + 1];
            var index = 0;

            while ((left <= middleIndex) && (right <= highIndex))
            {
                if (array[left].ValueToSort < array[right].ValueToSort)
                {
                    tempArray[index] = array[left];
                    left++;
                }
                else
                {
                    tempArray[index] = array[right];
                    right++;
                }

                index++;
            }

            for (var i = left; i <= middleIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = right; i <= highIndex; i++)
            {
                tempArray[index] = array[i];
                index++;
            }

            for (var i = 0; i < tempArray.Length; i++)
            {
                array[lowIndex + i] = tempArray[i];
            }
        }

        private static void Merge2(double[] keys, Element[] array, int lowIndex, int middleIndex, int highIndex)
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
        private static void MergeSort(Element[] array, int lowIndex, int highIndex)
        {
            if (lowIndex < highIndex)
            {
                var middleIndex = (lowIndex + highIndex) / 2;
                MergeSort(array, lowIndex, middleIndex);
                MergeSort(array, middleIndex + 1, highIndex);
                Merge(array, lowIndex, middleIndex, highIndex);
            }
        }

        public static void MergeSort(Element[] array)
        {
            MergeSort(array, 0, array.Length - 1);
        }

        class str
        {
            public int width;
            public int start;
        }

        public class Report
        {
            public double Sorting;
            public double Merge;
        }
        public static void HibridSort(Element[] array, int ThreadsNumber)
        {
            int SortingWindowWidth = array.Length / ThreadsNumber;

            List<Task> tasks = new List<Task>();
            List<int> Bounds = new List<int>() {0 };
            int edge = SortingWindowWidth;
            int StartElement = 0;
            while (edge< array.Length)
            {
                edge = StartElement + SortingWindowWidth;
                int width = edge <= array.Length ? SortingWindowWidth : array.Length - StartElement;
                int start = StartElement;

                Task t = Task.Factory.StartNew(() => {
                    Array.Sort(array, start, width, new ElementComparer()); 
                });
                //t.Wait();
                tasks.Add(t);
                StartElement += SortingWindowWidth ;
                Bounds.Add(StartElement);
            }
            Bounds[Bounds.Count - 1] = array.Length;
            //Bounds.Add(array.Length-1);
            Task.WaitAll(tasks.ToArray());
            List<int> Bounds2 = new List<int>(Bounds);
            while (Bounds.Count >= 3)
            {
                for (int i = 1; i < Bounds.Count-1; i++)
                {
                    Merge(array, 0, Bounds[i]-1, Bounds[i + 1]-1);
                    Bounds2.Remove(Bounds[i]);
                }
                Bounds = Bounds2;
            }
            
        }


        public static void HibridSort2(double[] keys, Element[] array, int ThreadsNumber)
        {
            int SortingWindowWidth = array.Length / ThreadsNumber;
            List<Task> tasks = new List<Task>();
            List<int> Bounds = new List<int>() { 0 };
            int edge = SortingWindowWidth;
            int StartElement = 0;
            while (edge < array.Length)
            {
                edge = StartElement + SortingWindowWidth;
                int width = edge <= array.Length ? SortingWindowWidth : array.Length - StartElement;
                int start = StartElement;

                Task t = Task.Factory.StartNew(() => {
                    int _start = start;
                    int _width = width;
                    Array.Sort(keys,array, _start, _width);
                });
                StartElement += SortingWindowWidth;
                Bounds.Add(StartElement);
                tasks.Add(t);
            }
            Bounds[Bounds.Count - 1] = array.Length;
            Task.WaitAll(tasks.ToArray());
            List<int> Bounds2 = new List<int>(Bounds);
            while (Bounds.Count >= 3)
            {
                for (int i = 1; i < Bounds.Count - 1; i++)
                {
                    //Merge2(keys,array, 0, Bounds[i] - 1, Bounds[i + 1] - 1);
                    Merge(array, 0, Bounds[i] - 1, Bounds[i + 1] - 1);
                    Bounds2.Remove(Bounds[i]);
                }
                Bounds = Bounds2;
            }
        }



        public static void Sort3(double[] keys, int left, int right) 
        {
           // while (depthLimit != 0)
            {
                int index1 = left;
                int index2 = right;
                int index3 = index1 + (index2 - index1 >> 1);
                //ArraySortHelper<double>.SwapIfGreater(keys, comparer, index1, index3);
                //ArraySortHelper<double>.SwapIfGreater(keys, comparer, index1, index2);
                //ArraySortHelper<double>.SwapIfGreater(keys, comparer, index3, index2);
                double obj1 = keys[index3];
                do
                {
                    while (keys[index1]< obj1)
                        ++index1;
                    while (obj1 < keys[index2])
                        --index2;
                    if (index1 <= index2)
                    {
                        if (index1 < index2)
                        {
                            double obj2 = keys[index1];
                            keys[index1] = keys[index2];
                            keys[index2] = obj2;
                        }
                        ++index1;
                        --index2;
                    }
                    else
                        break;
                }
                while (index1 <= index2);


                // --depthLimit;
                if (index2 - left <= right - index1)
                {
                    if (left < index2)
                        Sort3(keys, left, index2);
                    left = index1;
                }
                //else
                //{
                //    if (index1 < right)
                //        ArraySortHelper<T>.DepthLimitedQuickSort(keys, index1, right, comparer, depthLimit);
                //    right = index2;
                //}
                if (left >= right)
                    return;
            }
            //ArraySortHelper<T>.Heapsort(keys, left, right, comparer);
        }

        #region Parallel Sort

        public static class Sort
        {
            public static int Threshold = 150; // array length to use InsertionSort instead of SequentialQuickSort

            public static void InsertionSort<T>(T[] array, int from, int to) where T : IComparable<T>
            {
                for (int i = from + 1; i < to; i++)
                {
                    var a = array[i];
                    int j = i - 1;

                    //while (j >= from && array[j] > a)
                    while (j >= from && array[j].CompareTo(a) == -1)
                    {
                        array[j + 1] = array[j];
                        j--;
                    }
                    array[j + 1] = a;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void Swap<T>(T[] array, int i, int j) where T : IComparable<T>
            {
                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int Partition<T>(T[] array, int from, int to, int pivot) where T : IComparable<T>
            {
                // Pre: from <= pivot < to (other than that, pivot is arbitrary)
                var arrayPivot = array[pivot];  // pivot value
                Swap(array, pivot, to - 1); // move pivot value to end for now, after this pivot not used
                var newPivot = from; // new pivot 
                for (int i = from; i < to - 1; i++) // be careful to leave pivot value at the end
                {
                    // Invariant: from <= newpivot <= i < to - 1 && 
                    // forall from <= j <= newpivot, array[j] <= arrayPivot && forall newpivot < j <= i, array[j] > arrayPivot
                    //if (array[i] <= arrayPivot)
                    if (array[i].CompareTo(arrayPivot) != -1)
                    {
                        Swap(array, newPivot, i);  // move value smaller than arrayPivot down to newpivot
                        newPivot++;
                    }
                }
                Swap(array, newPivot, to - 1); // move pivot value to its final place
                return newPivot; // new pivot
                                 // Post: forall i <= newpivot, array[i] <= array[newpivot]  && forall i > ...
            }

            public static void SequentialQuickSort<T>(T[] array) where T : IComparable<T>
            {
                SequentialQuickSort(array, 0, array.Length);
            }

            static void SequentialQuickSort<T>(T[] array, int from, int to) where T : IComparable<T>
            {
                if (to - from <= Threshold)
                {
                    InsertionSort<T>(array, from, to);
                }
                else
                {
                    int pivot = from + (to - from) / 2; // could be anything, use middle
                    pivot = Partition<T>(array, from, to, pivot);
                    // Assert: forall i < pivot, array[i] <= array[pivot]  && forall i > ...
                    SequentialQuickSort(array, from, pivot);
                    SequentialQuickSort(array, pivot + 1, to);
                }
            }

            public static void ParallelQuickSort<T>(T[] array) where T : IComparable<T>
            {
                ParallelQuickSort(array,  0, array.Length,
                     (int)Math.Log(Environment.ProcessorCount, 2) + 4);
            }

            static void ParallelQuickSort<T>(T[] array,int from, int to, int depthRemaining) where T : IComparable<T>
            {
                if (to - from <= Threshold)
                {
                    InsertionSort<T>(array, from, to);
                }
                else
                {
                    int pivot = from + (to - from) / 2; // could be anything, use middle
                    pivot = Partition<T>(array, from, to, pivot);
                    if (depthRemaining > 0)
                    {
                        Parallel.Invoke(
                            () => ParallelQuickSort(array, from, pivot, depthRemaining - 1),
                            () => ParallelQuickSort(array,  pivot + 1, to, depthRemaining - 1));
                    }
                    else
                    {
                        ParallelQuickSort(array,  from, pivot, 0);
                        ParallelQuickSort(array,  pivot + 1, to, 0);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
