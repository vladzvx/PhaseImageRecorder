using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using RecorderCore;
using System;

namespace Benchmarks
{
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
            BenchmarkRunner.Run<PhaseStepImageBenchmark>();
            //BenchmarkRunner.Run<ArrayCopyBenchmark>();
        }
    }
}
