using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{
    public class ImageSource
    {
        public static bool AreEqual(double[,] arr1, double[,] arr2)
        {
            int arr1_0size = arr1.GetUpperBound(0);
            int arr2_0size = arr2.GetUpperBound(0);
            int arr1_1size = arr1.GetUpperBound(1);
            int arr2_1size = arr2.GetUpperBound(1);
            if (arr1_0size != arr2_0size || arr1_1size != arr2_1size) return false;
            else
            {
                for (int i = 0; i < arr1_0size + 1; i++)
                {
                    for (int j = 0; j < arr1_1size + 1; j++)
                    {
                        if (arr1[i, j] != arr2[i, j]) return false;
                    }
                }
                return true;
            }
        }
        public int Width = 1920;
        public int Height = 1024;
        private int CurrentIndex = 0;
        private List<double[,]> images = new List<double[,]>();
        private List<double[,]> phaseImages = new List<double[,]>();
        private object locker = new object();
        public ImageSource(int Height, int Width)
        {
            this.Height = Height;
            this.Width = Width;
        }

        public double[,] GetNextImage()
        {
            lock (locker)
            {
                CurrentIndex = CurrentIndex < images.Count ? CurrentIndex : 0;
                double[,] buffer = (double[,])images[CurrentIndex].Clone();
                CurrentIndex++;
                return buffer;
            }
        }

        public static List<Tuple<int,int,double>> GetNoZero(double[,] matrix,int dim)
        {
            List<Tuple<int, int, double>> res = new List<Tuple<int, int, double>>();
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (Math.Round(matrix[i, j],dim)!=0)
                    {
                        res.Add(Tuple.Create(i,j,matrix[i,j]));
                    }
                }
            }
            return res;

        }
        public void CreateImagesForStepMethod(int PhaseImagesNumber, uint NSteps)
        {
            lock (locker)
            {
                CurrentIndex = 0;
                images = new List<double[,]>();
                for (int i = 0; i < PhaseImagesNumber; i++)
                {
                    var t = getStepImagesPack(2 * ((double)i) / PhaseImagesNumber, NSteps);
                    images.AddRange(t.Item1);
                    phaseImages.Add(t.Item2);
                }
            }
        }
        public void CreateImagesForHilbert(int PhaseImagesNumber)
        {
            lock (locker)
            {
                images = new List<double[,]>();
                for (int i = 0; i < PhaseImagesNumber; i++)
                {
                    images.Add(getSingleImage(2 * ((double)i) / PhaseImagesNumber));
                }
            }
        }

        #region image Generator
        public static double[,] GetSphere(int x, int y, double x_size, double y_size, double R, double x_center_shift, double y_center_shift, double PhaseHeigh = 1)
        {

            double x_step = x_size / x;
            double MaxValue = 0;
            double y_step = y_size / y;
            double[,] matrix = new double[y, x];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    double _x = (i * x_step - x_center_shift);
                    double _y = (j * y_step - y_center_shift);
                    double value = R * R - (_x * _x + _y * _y);
                    value = value > 0 ? 2 * Math.Sqrt(value) : 0;
                    MaxValue = value > MaxValue ? value : MaxValue;
                    matrix[i, j] = value / R / 2 * PhaseHeigh;
                }
            }
            return matrix;
        }

        public static double[,] GetPlane(int x, int y, double LineNums = 1)
        {
            double[,] matrix = new double[y, x];
            double step = 2 * Math.PI * LineNums / y;
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    matrix[i, j] = (i * step);
                }
            }
            return matrix;
        }

        public struct point
        {
            public int ko;
            public int k1;
            public double value;
        }
        public static double[,] GetPlane(int size0, int size1, point point0, point point1, point point2)
        {
            double[,] matrix = new double[size0, size1];
            for (int y = 0; y < size0; y++)
            {
                for (int x = 0; x < size1; x++) 
                {
                    //matrix[y, x] =( point0.z + ((x - point0.x) * (point1.z - point0.z) * (point2.y - point0.y) + (y - point0.y) * (point1.x - point0.x) * (point2.z - point0.z) - (x - point0.x) * (point1.y - point0.y) * (point2.z - point0.z) - (y - point0.y) * (point1.z - point0.z) * (point2.x - point0.x)) / ((point1.x-point0.x)*(point2.y-point0.y)-(point1.y-point0.y)*(point2.x-point0.x)));

                    matrix[y, x] = (point0.value + ((x - point0.k1) * (point1.value - point0.value) * (point2.ko - point0.ko) + (y - point0.ko) * (point1.k1 - point0.k1) * (point2.value - point0.value) - (x - point0.k1) * (point1.ko - point0.ko) * (point2.value - point0.value) - (y - point0.ko) * (point1.value - point0.value) * (point2.k1 - point0.k1)) / ((point1.k1 - point0.k1) * (point2.ko - point0.ko) - (point1.ko - point0.ko) * (point2.k1 - point0.k1)));
                }
            }
            return matrix;
        }

        public static double[,] GetTrendPlane(double[,] image)
        {

            int size0 = image.GetUpperBound(0)+1;
            int size1 = image.GetUpperBound(1)+1;
            int k1_0 =(int) size1 / 10;
            int k0_0 =(int) size0 / 10;

            int k1_1 = size1 - (int)size1 / 10;
            int k0_1 = (int)size0 / 10;

            int k1_2 =  (int)size1 / 2;
            int k0_2 = size0 - (int)size0 / 10;


            point point0 = new point() { ko = k0_0, k1 = k1_0, value = image[k0_0, k1_0] };
            point point1 = new point() { ko = k0_1, k1 = k1_1, value = image[k0_1, k1_1] };
            point point2 = new point() { ko = k0_2, k1 = k1_2, value = image[k0_2, k1_2] };


            double[,] matrix = new double[size0, size1];
            for (int y = 0; y < size0; y++)
            {
                for (int x = 0; x < size1; x++)
                {
                    matrix[y, x] = (point0.value + ((x - point0.k1) * (point1.value - point0.value) * (point2.ko - point0.ko) + (y - point0.ko) * (point1.k1 - point0.k1) * (point2.value - point0.value) - (x - point0.k1) * (point1.ko - point0.ko) * (point2.value - point0.value) - (y - point0.ko) * (point1.value - point0.value) * (point2.k1 - point0.k1)) / ((point1.k1 - point0.k1) * (point2.ko - point0.ko) - (point1.ko - point0.ko) * (point2.k1 - point0.k1)));
                }
            }
            return matrix;
        }


        public static double[,] DelTrend(double[,] image)
        {
            return diff(image, GetTrendPlane(image));;
        }

        public static void AddPlane(double[,] matrix, int x, int y, double LineNums = 1)
        {
            double step = 2 * Math.PI * LineNums / y;
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    matrix[i, j] += (i * step);
                }
            }
        }

        public static void smooth(double[,] matrix, int windowSize)
        {
            if (windowSize % 2 == 0) throw new Exception("windowSize must be odd number!");
            int size0 = matrix.GetUpperBound(0) + 1;
            int size1 = matrix.GetUpperBound(1) + 1;
            for (int i= windowSize / 2; i< size0- windowSize/2; i++)
            {
                for (int j = windowSize / 2; j < size1- windowSize / 2; j++)
                {
                    double val = 0;
                    double c = 0;
                    for (int k0=-windowSize / 2;k0<= windowSize / 2; k0++)
                    {
                        for (int k1 = -windowSize / 2; k1 <= windowSize / 2; k1++)
                        {
                            val = val + matrix[i + k0, j + k1];
                            c++;
                        }
                    }
                    matrix[i, j] = val / c;
                }
            }
        }

        public static void AddNoise(double[,] matrix, double A)
        {
            Random rnd = new Random();
            int size0 = matrix.GetUpperBound(0) + 1;
            int size1 = matrix.GetUpperBound(1) + 1;
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    matrix[i, j] = matrix[i, j]+((rnd.NextDouble() * 2) - 1d)* A;
                }
            }
        }

        public static double[,] CreateOneValueArray(double value, int size0, int size1)
        {
            double[,] marixs = new double[size0, size1];
            Random rnd = new Random();
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    marixs[i, j] = value;
                }
            }
            return marixs;
        }


        public static double std(double[,] matrix1, double[,] matrix2)
        {
            double d = 0;
            if (matrix1.GetUpperBound(0) != matrix2.GetUpperBound(0) ||
                matrix1.GetUpperBound(1) != matrix2.GetUpperBound(1))
                throw new ArgumentException("Incompatible matrix sizes!");
            double[,] difference = diff(matrix1, matrix2);
            pow(difference, 2);
            double meanValue = mean(difference);
            return Math.Sqrt(meanValue);
        }

        public static double[,] diff(double[,] matrix1, double[,] matrix2)
        {
            
            if (matrix1.GetUpperBound(0) != matrix2.GetUpperBound(0) ||
                matrix1.GetUpperBound(1) != matrix2.GetUpperBound(1))
                throw new ArgumentException("Incompatible matrix sizes!");
            double[,] ForRetirn = new double[matrix1.GetUpperBound(0) + 1, matrix1.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix1.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix1.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = matrix1[i, j] - matrix2[i, j];
                }
            }
            return ForRetirn;
        }

        public static void subtract_min(double[,] matrix)
        {
            double val = min(matrix);
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    matrix[i, j] = matrix[i, j] - val;
                }
            }
        }

        public static void del_level(double[,] matrix, double value)
        {
            if (value == 0) return;
            subtract_min(matrix);
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    double newValue = matrix[i, j] - value;
                    matrix[i, j] = newValue>=0? newValue: 0;
                }
            }
        }

        public static double sum(double[,] matrix)
        {
            double d = 0;
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    d += matrix[i, j];
                }
            }
            return d;
        }

        public static double mean(double[,] matrix)
        {
            double s = sum(matrix);
            return s / (matrix.GetUpperBound(0) + 1) / (matrix.GetUpperBound(1) + 1);
        }
        public static double[,] plus(double[,] matrix1, double[,] matrix2)
        {
            int size0 = matrix1.GetUpperBound(0)+1;
            int size1 = matrix1.GetUpperBound(1)+1;
            double[,] matrix = new double[size0, size1];
            for (int i = 0; i <size0 ; i++)
            {
                for (int j = 0; j <size1; j++)
                {
                    matrix[i, j] = matrix1[i, j] + matrix2[i, j];
                }
            }
            return matrix;
        }

        public static void pow(double[,] matrix, double exp)
        {
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    matrix[i, j] = Math.Pow(matrix[i, j], exp);
                }
            }
        }
        public static double max(double[,] matrix)
        {
            double maxValue = matrix[0, 0];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (matrix[i, j] > maxValue)
                        maxValue = matrix[i, j];
                }
            }
            return maxValue;
        }

        public double[,] SummMatrix(double[,] matrix1, double[,] matrix2)
        {
            double[,] ForRetirn = new double[matrix1.GetUpperBound(0) + 1, matrix1.GetUpperBound(1) + 1];
            if (matrix1.GetUpperBound(0) != matrix2.GetUpperBound(0) ||
                matrix1.GetUpperBound(1) != matrix2.GetUpperBound(1))
                throw new ArgumentException("Incompatible matrix sizes!");
            for (int i = 0; i <= matrix1.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix1.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = matrix1[i, j] + matrix2[i, j];
                }
            }
            return ForRetirn;
        }

        public static double[,] GetCos(double[,] matrix, double shift = 0, double mult = 1)
        {
            double[,] ForRetirn = new double[matrix.GetUpperBound(0) + 1, matrix.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = (1 + Math.Cos(matrix[i, j] + shift)) * mult * 0.5;
                }
            }
            return ForRetirn;
        }

        private void SetCos(double[,] matrix, double shift = 0, double mult = 1)
        {
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    matrix[i, j] = (1 + Math.Cos(matrix[i, j] + shift)) * mult * 0.5;
                }
            }
        }

        public static void mult(double[,] matrix1, double[,] matrix2)
        {
            int size0 = matrix1.GetUpperBound(0);
            int size1 = matrix1.GetUpperBound(1);
            int _size0 = matrix2.GetUpperBound(0);
            int _size1 = matrix2.GetUpperBound(1);
            if (size0 != _size0 || size1 != _size1) throw new ArgumentException("Uncomp. array sizes!0");
            for (int i = 0; i <= size0; i++)
            {
                for (int j = 0; j <= size1; j++)
                {
                    matrix1[i, j] = matrix1[i, j] * matrix2[i, j];
                }
            }
        }

        public static void mult(double[,] matrix1, double value)
        {
            int size0 = matrix1.GetUpperBound(0);
            int size1 = matrix1.GetUpperBound(1);
            for (int i = 0; i <= size0; i++)
            {
                for (int j = 0; j <= size1; j++)
                {
                    matrix1[i, j] = matrix1[i, j] * value;
                }
            }
        }


        public static double min(double[,] matrix)
        {
            double minValue = matrix[0, 0];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (matrix[i, j] <= minValue)
                        minValue = matrix[i, j];
                }
            }
            return minValue;
        }


        public static double minParallel(double[,] matrix)
        {
            object locker = new object();
            double MinValue = matrix[0, 0];
            int size0 = matrix.GetUpperBound(0) + 1;
            int size1 = matrix.GetUpperBound(1) + 1;
            Parallel.For(0, size0, (i) => 
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (matrix[i, j] < MinValue)
                        lock(locker)
                            MinValue = matrix[i, j];
                }
            });
            return MinValue;
        }

        public static double maxParallel(double[,] matrix)
        {
            object locker = new object();
            double MaxValue = matrix[0, 0];
            int size0 = matrix.GetUpperBound(0) + 1;
            int size1 = matrix.GetUpperBound(1) + 1;
            Parallel.For(0, size0, (i) =>
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (matrix[i, j] > MaxValue)
                        lock (locker)
                            MaxValue = matrix[i, j];
                }
            });
            return MaxValue;
        }

        public double[,] GetPhaseImage(double val=0)
        {
            double[,] matrix1 = GetSphere(Width, Height, 500, 500, 150, 250, 250, Math.PI * 4 * (1.2 + Math.Cos(Math.PI * val)));
            AddPlane(matrix1, Width, Height, 10);
            return matrix1;
        }
        private Tuple<List<double[,]>, double[,]> getStepImagesPack(double val, uint NSteps)
        {
            List<double[,]> temp = new List<double[,]>();
            double[,] matrix1 = GetSphere(Width, Height, 500, 500, 150, 250, 250, Math.PI * 4 * (1.2 + Math.Cos(Math.PI * val)));
            AddPlane(matrix1, Width, Height, 10);

            temp.Add(GetCos(matrix1, mult: 255));
            temp.Add(GetCos(matrix1, Math.PI / 2, mult: 255));
            temp.Add(GetCos(matrix1, Math.PI, mult: 255));
            if (NSteps >= 4) temp.Add(GetCos(matrix1, 3 * Math.PI / 2, mult: 255));
            if (NSteps >= 5) temp.Add(GetCos(matrix1, 4 * Math.PI / 2, mult: 255));
            return Tuple.Create(temp,matrix1);
        }

        private double[,] getSingleImage(double val)
        {
            double[,] matrix1 = GetSphere(Width, Height, 500, 500, 150, 250, 250, Math.PI * 4 * (1.2 + Math.Cos(Math.PI * val)));
            AddPlane(matrix1, Width, Height, 10);
            SetCos(matrix1, mult: 255);
            return matrix1;
        }
        #endregion
    }
}
