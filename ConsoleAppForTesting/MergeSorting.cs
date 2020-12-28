using System;
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
        private static Random rnd = new Random();
        public static void Refresh(IEnumerable<Element> elements)
        {
            foreach (Element el in elements)
                el.Refresh();
        }

        public static Element[] CreateArray(int size)
        {
            Element[] elements = new Element[size];
            for (int i = 0; i < size; i++)
            {
                elements[i] = new Element();
            }
            return elements;
        }

        public static double[] CreateDouble(int size)
        {
            double[] elements = new double[size];
            for (int i = 0; i < size; i++)
            {
                elements[i] = rnd.NextDouble() ;
            }
            return elements;
        }


        public double ValueToSort;

        public Element()
        {
            ValueToSort = rnd.NextDouble();
        }
        public void Refresh()
        {
            ValueToSort = rnd.NextDouble();
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

        public static void QuickSort(Element[] array)
        {
            QuickSort(array, 0, array.Length-1);
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

        #endregion


        

        #region сортировка слиянием


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

        #endregion
    }
}
