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

        public static Tuple<byte[,,], double[,]> GetTestPair2(int Width, int Height, double MaxValue, double NumLines = 10)
        {
            double[,] image = ImageSource.GetSphere(Width, Height, 500, 500, 150, 250, 250, MaxValue);
            ImageSource.AddPlane(image, Width, Height, NumLines);

            double[,] image2 = ImageSource.GetCos(image, Math.PI / 2, mult: 255);

            double max = 0;
            double min = 0;
            int size0 = image.GetUpperBound(0) + 1;
            int size1 = image.GetUpperBound(1) + 1;

            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    //image[i, j] = Math.Round(image[i, j] / Math.PI * Wavelength, 0);
                    double val1 = image2[i, j];
                    if (val1 < min) min = val1;
                    if (val1 > max) max = val1;

                }
            }

            byte[,,] nImage = new byte[size0, size1, 3];
            Parallel.For(0, size0 , (i) =>
            {
                for (int j = 0; j < size1; j++)
                {
                    byte val = (byte)(255 * (image2[i, j] - min) / (max - min));
                    nImage[i, j, 0] = val;
                    nImage[i, j, 1] = val;
                    nImage[i, j, 2] = val;
                }
            });


            return Tuple.Create(nImage, image);
        }


    }
}
