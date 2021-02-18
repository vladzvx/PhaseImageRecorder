using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RecorderCore;
using System;
using System.Collections.Concurrent;

namespace Benchmarks
{
    class val
    {
        int q = 0;
        string text = "qqqqqqqqqqqq";
    }
    public class EnumBenchmark
    {
        static val va = new val();
        static class strings1
        {
            public const string str1 = "str1";
            public const string str2 = "str2";
            public const string str3 = "str3";
            public const string str4 = "str4";
            public const string str5 = "str5";
            public const string str6 = "str6";
        }

        static class strings2
        {
            public const string str1 = "str1";
            public const string str2 = "str2";
        }


        test t = test.second;
        string text = "text";
        static Random rnd = new Random();
        ConcurrentDictionary<string, ConcurrentDictionary<string, val>> dict1 = new ConcurrentDictionary<string, ConcurrentDictionary<string, val>>();
        ConcurrentDictionary<test, ConcurrentDictionary<test2, val>> dict2 = new ConcurrentDictionary<test, ConcurrentDictionary<test2, val>>();
        public enum test
        {
            first,
            second,
            thrd,
            forth,
            fifth,
            six,
            none
        }

        public enum test2
        {
            first,
            second,
        }

        public void TryAddVal(string key1,string key2)
        {
            if (dict1.ContainsKey(key1))
            {
                dict1[key1].TryAdd(key2, va);
            }
            else
            {
                var temp = new ConcurrentDictionary<string, val>();
                temp.TryAdd(key2,va);
                dict1.TryAdd(key1, temp);
            }
        }
        public void TryAddVal(test key1, test2 key2)
        {
            if (dict2.ContainsKey(key1))
            {
                dict2[key1].TryAdd(key2, va);
            }
            else
            {
                var temp = new ConcurrentDictionary<test2, val>();
                temp.TryAdd(key2, va);
                dict2.TryAdd(key1, temp);
            }
        }


        [Benchmark(Description = "add_enum")]
        public void Test0()
        {
            TryAddVal(test.first, test2.first);
            //TryAddVal(test.first, test2.second);
            //TryAddVal(test.second, test2.second);
            //TryAddVal(test.second, test2.first);
            //TryAddVal(test.thrd, test2.first);
            //TryAddVal(test.thrd, test2.second);
            //TryAddVal(test.forth, test2.second);
            //TryAddVal(test.forth, test2.first);
            //TryAddVal(test.fifth, test2.first);
            //TryAddVal(test.fifth, test2.second);
            //TryAddVal(test.six, test2.second);
            //TryAddVal(test.six, test2.first);
            //TryAddVal(test.first, test2.first);
            //TryAddVal(test.first, test2.second);
            //TryAddVal(test.second, test2.second);
            //TryAddVal(test.second, test2.first);
            //TryAddVal(test.thrd, test2.first);
            //TryAddVal(test.thrd, test2.second);
            //TryAddVal(test.forth, test2.second);
            //TryAddVal(test.forth, test2.first);
            //TryAddVal(test.fifth, test2.first);
            //TryAddVal(test.fifth, test2.second);
            //TryAddVal(test.six, test2.second);
            //TryAddVal(test.six, test2.first);
        }
        [Benchmark(Description = "add_string")]
        public void Test1()
        {
            TryAddVal(strings1.str1, strings2.str1);
            //TryAddVal(strings1.str2, strings2.str1);
            //TryAddVal(strings1.str3, strings2.str1);
            //TryAddVal(strings1.str4, strings2.str1);
            //TryAddVal(strings1.str5, strings2.str1);
            //TryAddVal(strings1.str6, strings2.str1);

            //TryAddVal(strings1.str1, strings2.str2);
            //TryAddVal(strings1.str2, strings2.str2);
            //TryAddVal(strings1.str3, strings2.str2);
            //TryAddVal(strings1.str4, strings2.str2);
            //TryAddVal(strings1.str5, strings2.str2);
            //TryAddVal(strings1.str6, strings2.str2);

            //TryAddVal(strings1.str1, strings2.str1);
            //TryAddVal(strings1.str2, strings2.str1);
            //TryAddVal(strings1.str3, strings2.str1);
            //TryAddVal(strings1.str4, strings2.str1);
            //TryAddVal(strings1.str5, strings2.str1);
            //TryAddVal(strings1.str6, strings2.str1);

            //TryAddVal(strings1.str1, strings2.str2);
            //TryAddVal(strings1.str2, strings2.str2);
            //TryAddVal(strings1.str3, strings2.str2);
            //TryAddVal(strings1.str4, strings2.str2);
            //TryAddVal(strings1.str5, strings2.str2);
            //TryAddVal(strings1.str6, strings2.str2);
        }

        [Benchmark(Description = "read_enum")]
        public void Test2()
        {
            TryAddVal(test.first, test2.first);
            val q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];
            q1 = dict2[test.first][test2.first];

        }

        [Benchmark(Description = "read_string")]
        public void Test3()
        {
            TryAddVal(strings1.str1, strings2.str1);
            val q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];
            q1 = dict1[strings1.str1][strings2.str1];

        }

    }
    public class PhaseStepImageBenchmark
    {
        public static ImageSource source;
        public static double[,] image;
        public static void init()
        {
            source = new ImageSource(300, 300);
            source.CreateImagesForStepMethod(1, 4);
            //image = source.GetNextImage();
        }
        [Benchmark(Description = "init")]
        public void Test0()
        {
            source = new ImageSource(1000, 2000);
            source.CreateImagesForStepMethod(1, 4);
            //StepPhaseImage phaseImage = new StepPhaseImage(source.GetNextImage());
            //phaseImage.AddStep(source.GetNextImage());
            //phaseImage.AddStep(source.GetNextImage());
            //phaseImage.AddStep(source.GetNextImage());
            //phaseImage.MaxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
        }

        [Benchmark(Description = "CreateNew")]//3.6//
        public void Test1()
        {
            source = new ImageSource(1000, 2000);
            source.CreateImagesForStepMethod(1, 4);
            StepPhaseImage phaseImage = new StepPhaseImage(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.MaxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
        }

        [Benchmark(Description = "Calculate")]//8,38//
        public void Test2()
        {
            source = new ImageSource(1000, 2000);
            source.CreateImagesForStepMethod(1, 4);
            StepPhaseImage phaseImage = new StepPhaseImage(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.MaxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
            phaseImage.CalculatePhaseImage();
        }

        [Benchmark(Description = "Unwrap")]//7,96
        public void Test3()
        {
            source = new ImageSource(1000, 2000);
            source.CreateImagesForStepMethod(1, 4);
            StepPhaseImage phaseImage = new StepPhaseImage(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.MaxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
            phaseImage.CalculatePhaseImage();
            phaseImage.Unwrap();
        }
        [Benchmark(Description = "CreateUI matrix")]//5.5
        public void Test4()
        {
            source = new ImageSource(1000, 2000);
            source.CreateImagesForStepMethod(1, 4);
            StepPhaseImage phaseImage = new StepPhaseImage(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.AddStep(source.GetNextImage());
            phaseImage.MaxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
            phaseImage.Process();
        }
    }
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
          //  PhaseStepImageBenchmark.init();
           // PhaseStepImageBenchmark bm = new PhaseStepImageBenchmark();
         //   bm.Test1();
          //  bm.Test2();
        //    bm.Test3();
          //  bm.Test4();
            BenchmarkRunner.Run<EnumBenchmark>();
            //BenchmarkRunner.Run<ArrayCopyBenchmark>();
        }
    }
}
