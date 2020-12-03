using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace Benchmarks
{
    public class ArrayCopyBenchmark
    {
        public static int h = 1000;
        public static int w = 2000;
        public static double[,] matrix = new double[h, w];


        [Benchmark(Description = "CreateNew")]
        public void Test()
        {
            double[,] result = new double[h, w];
        }

        [Benchmark(Description = "AllItemIter")]
        public void Test0()
        {
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    matrix[i, j] = matrix[i, j];
                }
            }
        }

        [Benchmark(Description = "Clone")]
        public void Test1()
        {
            double[,] result = (double[,])matrix.Clone();
        }

        [Benchmark(Description = "CopyByItem")]
        public void Test2()
        {
            double[,] result = new double[matrix.GetUpperBound(0) + 1, matrix.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    result[i, j] = matrix[i, j];
                }
            }
        }

        [Benchmark(Description = "CopyByItemExistSizes")]
        public void Test3()
        {
            double[,] result = new double[h, w];
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    result[i, j] = matrix[i, j];
                }
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ArrayCopyBenchmark>();
        }
    }
}
