using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    static class Sortings
    {
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

        public static bool SortingChecker(Element[] elements)
        {
            for (int i = 0; i < elements.Length-1; i++)
            {
                if (elements[i].ValueToSort > elements[i + 1].ValueToSort)
                    return false;
            }
            return true;
        }
        public static void Sort(Element[] elements)
        {

        }
    }
}
