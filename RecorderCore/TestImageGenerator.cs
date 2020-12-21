using RecorderCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public static class TestImageGenerator
    {
        public static Tuple<StepPhaseImage,double[,]> GetTestPair(int Width, int Height,double MaxValue, double NumLines =10)
        {
            double[,] matrix1 = ImageSource.GetSphere(Width, Height, 500, 500, 150, 250, 250, MaxValue);
            ImageSource.AddPlane(matrix1, Width, Height, NumLines);
            StepPhaseImage stepPhase = new StepPhaseImage(ImageSource.GetCos(matrix1, mult: 255));

            stepPhase.AddStep(ImageSource.GetCos(matrix1, Math.PI / 2, mult: 255));
            stepPhase.AddStep(ImageSource.GetCos(matrix1, Math.PI, mult: 255));
            stepPhase.AddStep(ImageSource.GetCos(matrix1, 3 * Math.PI / 2, mult: 255));
            return Tuple.Create(stepPhase, matrix1);
        }
    }
}
