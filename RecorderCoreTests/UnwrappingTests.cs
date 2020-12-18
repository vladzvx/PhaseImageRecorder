using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecorderCore.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RecorderCore.Unwrapping;

namespace RecorderCore
{
    [TestClass]
    public class UnwrappingTests
    {
        internal bool sortingCheck(List<edge>  edges)
        {
            for (int i = 1; i < edges.Count; i++)
            {
                if (edges[i - 1].reaibility < edges[i].reaibility)
                    return false;
            }
            return true;
        }

        [TestMethod]
        public void EdgdeSortingTest1()
        {
            List<edge> edges = new List<edge>()
            {
                new edge(){reaibility=0},
                new edge(){reaibility=1},
                new edge(){reaibility=1},
                new edge(){reaibility=2},

            };
            edges.Sort(new edgeComparer());
            Assert.IsTrue(sortingCheck(edges));
        }
        [TestMethod]
        public void EdgdeSortingTest2()
        {
            List<edge> edges = new List<edge>()
            {
                new edge(){reaibility=10},
                new edge(){reaibility=1},
                new edge(){reaibility=12},
                new edge(){reaibility=2},

            };
            edges.Sort(new edgeComparer());
            Assert.IsTrue(sortingCheck(edges));
        }
    }
}
