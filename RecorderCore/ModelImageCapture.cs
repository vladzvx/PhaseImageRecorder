using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{
    public class ModelImageCapture
    {
        public ModelImageCapture()
        {
            thread = new Thread(new ThreadStart(GetImage));
            Recreate(4);
        }

        public void Recreate(int NSteps)
        {
            images = new List<double[,]>();
            for (int i = 0; i < 10; i++)
            {
                images.AddRange(get_image(i * Math.PI / 25, 4));
            }
        }
        //private double FPS = 1;
        private object locker = new object();
        private new List<double[,]> images = new List<double[,]>();
        private int currentImage = 0;
        private Thread thread;
        private Random rnd = new Random();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private bool paused = false;
        #region image Generator
        private double[,] GetSphere(int x, int y, double x_size, double y_size, double R, double x_center_shift, double y_center_shift, double PhaseHeigh = 1)
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

        private double[,] GetPlane(int x, int y, double LineNums = 1)
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
        private double max(double[,] matrix)
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

        private double[,] SummMatrix(double[,] matrix1, double[,] matrix2)
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

        private double[,] GetCos(double[,] matrix, double shift = 0, double mult = 1)
        {
            double[,] ForRetirn = new double[matrix.GetUpperBound(0) + 1, matrix.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = (1 + Math.Cos(matrix[i, j] + shift)) * mult*0.5;
                }
            }
            return ForRetirn;
        }

        private double[,] MultipleMatrix(double[,] matrix)
        {
            double[,] ForRetirn = new double[matrix.GetUpperBound(0) + 1, matrix.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = matrix[i, j];
                }
            }
            return ForRetirn;
        }

        private double min(double[,] matrix)
        {
            double minValue = matrix[0, 0];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (matrix[i, j] < minValue)
                        minValue = matrix[i, j];
                }
            }
            return minValue;
        }

        public void Start()
        {
            thread.Start();
        }
        public void Stop()
        {
            thread.Start();
        }
        public void Pause()
        {
            _lock.EnterWriteLock();
            paused = true;
            _lock.ExitWriteLock();
        }

        public void PauseRelease()
        {
            _lock.EnterWriteLock();
            paused = false;
            _lock.ExitWriteLock();
        }
        private List<double[,]> get_image(double val,uint NSteps)
        {
            List<double[,]> temp = new List<double[,]>();
            //Bitmap bitmap = new Bitmap(pictureBox1.Height, pictureBox1.Width);
            double[,] matrix1 = GetSphere(Height, Width, 500, 500, 150, 250, 250, Math.PI * 4 * (1.2 + Math.Cos(Math.PI * val)));
            double[,] matrix2 = GetPlane(Height, Width, 10);
            double[,] matrix3 = SummMatrix(matrix1, matrix2);

            temp.Add(GetCos(matrix3,mult:255));
            temp.Add(GetCos(matrix3, Math.PI / 2, mult: 255));
            temp.Add(GetCos(matrix3, Math.PI, mult: 255));
            if (NSteps >= 4)  temp.Add(GetCos(matrix3, 3 * Math.PI / 2, mult: 255));

            if (NSteps >= 5) temp.Add(GetCos(matrix3, 4 * Math.PI / 2, mult: 255));
            return temp;
        }

        public void GetImage()
        {
            while (true)
            {
                _lock.EnterReadLock();
                bool local_paused = paused;
                _lock.ExitReadLock();
                if (!local_paused)
                {
                    foreach (double[,] image in images)
                    {
                        //if (rec != null) rec.Invoke((double[,])image.Clone());
                        if (rec != null) rec.Invoke(image);
                        Thread.Sleep((int)(1000 / FPS));

                    }
                }
                else Thread.Sleep(300);

            }

        }
        #endregion

        public delegate void ImageReciever(double[,] image);

        public event ImageReciever rec;

        public double FPS = 25;
        public int Height = 1920;
        public int Width = 1024;

    }
    /*
    class PhaseImageModeling
    {
        #region fields
        public int Height;
        public int Width;
        public double x_size;
        public double y_size;
        #endregion

        public PhaseImageModeling(int Height, int Width, double x_size, double y_size)
        {
            this.Height = Height;
            this.Width = Width;
            this.y_size = y_size;
            this.x_size = x_size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Height">pixels</param>
        /// <param name="Width">pixels</param>
        /// <param name="x_size">um</param>
        /// <param name="y_size">um</param>
        /// <param name="R">um</param>
        /// <param name="x_center_shift">um</param>
        /// <param name="y_center_shift">um</param>
        /// <param name="PhaseHeigh">radians</param>
        /// <returns></returns>
        private double[,] GetSphere(double R, double x_center_shift, double y_center_shift, double PhaseHeigh = 1)
        {

            double x_step = x_size / Width;
            double MaxValue = 0;
            double y_step = y_size / Height;
            double[,] matrix = new double[Height, Width];
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

        /// <summary>
        /// creates plane in matrix
        /// </summary>
        /// <param name="Height">pixels</param>
        /// <param name="Width">pixels</param>
        /// <param name="LineNums">Number of interfierence lines, number of 2Pi in max value of plane</param>
        /// <returns></returns>
        private double[,] GetPlane(double LineNums = 1)
        {
            double[,] matrix = new double[Height, Width];
            double step = 2 * Math.PI * LineNums / Height;
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    matrix[i, j] = (i * step);
                }
            }
            return matrix;
        }

        private double max(double[,] matrix)
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

        private double min(double[,] matrix)
        {
            double minValue = matrix[0, 0];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (matrix[i, j] < minValue)
                        minValue = matrix[i, j];
                }
            }
            return minValue;
        }

        private double[,] SummMatrix(double[,] matrix1, double[,] matrix2)
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


        private double[,] MultipleMatrix(double[,] matrix, double Multiplicator)
        {
            double[,] ForRetirn = new double[matrix.GetUpperBound(0) + 1, matrix.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = matrix[i, j]* Multiplicator;
                }
            }
            return ForRetirn;
        }
        /*
        public Bitmap GetBitmap(double[,] matrix)
        {
            if (matrix.Rank != 2) throw new ArgumentException("Uncorrect matrix dimensions");
            Bitmap bitmap = new Bitmap(matrix.GetUpperBound(0)+1, matrix.GetUpperBound(1) + 1);
            double max = this.max(matrix);
            double min = this.min(matrix);
            double amlp = max - min;

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    double t = matrix[j, i];
                    double value = max != min && t != 0 ? 255 * (matrix[j, i] - min) / amlp : 0;
                    bitmap.SetPixel(i, j, Color.FromArgb((int)(value), (int)(value), (int)(value)));
                }
            }
            return bitmap;
        }

        public double[,] GetInterferogramm(double[,] matrix, double MaxValue = 255, double MinValue = 0)
        {
            double[,] ForRetirn = new double[matrix.GetUpperBound(0) + 1, matrix.GetUpperBound(1) + 1];
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = (Math.Cos(matrix[i, j]) + 1) * (MaxValue - MinValue);
                }
            }
            return ForRetirn;
        }
        
        public double[,] GetSpherePhaseImage(double R, double x_center_shift, double y_center_shift, double PhaseHeigh, double LineNums)
        {
            double[,] matrix1 = GetSphere(R, x_center_shift, y_center_shift, PhaseHeigh);
            double[,] matrix2 = GetPlane(LineNums);
            double[,] matrix3 = SummMatrix(matrix1, matrix2);
            return matrix3;
        }
        public double[,] WrapPhaseImage(double[,] PhaseImage)
        {
            double[,] ForRetirn = new double[PhaseImage.GetUpperBound(0) + 1, PhaseImage.GetUpperBound(1) + 1];
            for (int i = 0; i <= PhaseImage.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= PhaseImage.GetUpperBound(1); j++)
                {
                    double phase_steps = Math.Floor(PhaseImage[i, j] / Math.PI) * Math.PI;
                    ForRetirn[i, j] = PhaseImage[i, j]- phase_steps;
                }
            }
            return ForRetirn;
        }

        public double[,] UnwrapPhaseImage(double[,] PhaseImage, double level=0.8)
        {
            double[,] ForRetirn = new double[PhaseImage.GetUpperBound(0) + 1, PhaseImage.GetUpperBound(1) + 1];
            for (int i = 0; i <= PhaseImage.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= PhaseImage.GetUpperBound(1); j++)
                {
                    ForRetirn[i, j] = PhaseImage[i, j];
                }
            }

            for (int i = 0; i <= ForRetirn.GetUpperBound(0); i++)
            {
                double diff = 0;
                for (int j = 0; j < ForRetirn.GetUpperBound(1); j++)
                {
                    double currentDiff = ForRetirn[i, j] - ForRetirn[i, j + 1];
                    diff += Math.Abs(currentDiff) > level * Math.PI ? Math.Abs(currentDiff) / currentDiff * Math.PI : 0;
                    ForRetirn[i, j+1]+= ForRetirn[i, j]+ diff;
                }
            }

            for (int j = 0; j <= ForRetirn.GetUpperBound(1); j++)
            {
                double diff = 0;
                for (int i = 0; i < ForRetirn.GetUpperBound(0); i++)
                {
                    double currentDiff = ForRetirn[i, j] - ForRetirn[i, j + 1];
                    diff += Math.Abs(currentDiff) >  level * Math.PI ? Math.Abs(currentDiff)/ currentDiff*Math.PI : 0;
                    ForRetirn[i, j + 1] += ForRetirn[i, j] + diff;
                }
            }

            return ForRetirn;
        }

    }

    */
}
