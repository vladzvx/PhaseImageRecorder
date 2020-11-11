using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RecorderCore
{
    public class ImageCapture
    {
        #region fields
        public delegate void ImageReciever(Mat image);
        public delegate void ExternalAction(int count);
        public event ImageReciever rec;
        public event ExternalAction action;
        private int CameraNumber = 0;
        private int FramePause = 0;
        private double MaxFrameCounter = 0;

        private Emgu.CV.VideoCapture capture;
        private Thread thread;
        private object locker = new object();

        #endregion

        #region constructors
        public ImageCapture()
        {
            lock (locker)
            {
                capture = new VideoCapture(0);
                thread = new Thread(new ThreadStart(grab));
                
            }

        }

        public void Start()
        {
            thread.Start();

        }

        #endregion

        #region image grabbing

        private void grab()
        {
            int frameCounter = 0;
            while (true)
            {
                lock (locker)
                {
                    if (action != null) action.Invoke(frameCounter);
                    Thread.Sleep(FramePause);
                    Grab();
                    frameCounter = frameCounter < MaxFrameCounter ? frameCounter + 1 : 0;
                }

            }
        }

        private void Grab()
        {
            Mat mat = new Mat();
            capture.Grab();
            capture.Retrieve(mat);
            if (rec != null)
            {
                    rec.Invoke(mat);
            } 
        }

        #endregion

        #region external ruling
        public void UpdateCamera(int CameraNumber)
        {
            lock (locker)
            {
                if (this.CameraNumber!= CameraNumber)
                    capture = new VideoCapture(CameraNumber);
            }
        }

        public void UpdateMaxFrameCounter(int MaxFrameCounter)
        {
            lock (locker)
            {
                this.MaxFrameCounter = MaxFrameCounter;
            }
        }

        public void UpdateFramePause(int FramePause)
        {
            lock (locker)
            {
                this.FramePause = FramePause;
            }
        }

        #endregion

        #region add and remove handlers
        public void AddReciever(ImageReciever imageReciever)
        {
            rec += imageReciever;
        }
        public void AddExternalAction(ExternalAction externalAction)
        {
            action += externalAction;
        }
        public void RemoveReciever(ImageReciever imageReciever)
        {
            rec -= imageReciever;
        }
        public void RemoveExternalAction(ExternalAction externalAction)
        {
            action -= externalAction;
        }
        #endregion

    }

    public class ModelImageCapture
    {
        public ModelImageCapture()
        {
            thread=  new Thread(new ThreadStart(GetImage));
        }
        private Thread thread;
        private Random rnd = new Random();
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
                    ForRetirn[i, j] = (1+Math.Cos(matrix[i, j] + shift))*mult;
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

        private void get_image(double val)
        {
            //Bitmap bitmap = new Bitmap(pictureBox1.Height, pictureBox1.Width);
            double[,] matrix1 = GetSphere(Height, Width, 500, 500, 150, 250, 250, Math.PI * 4 * (1.2+Math.Cos(Math.PI*val)));
            double[,] matrix2 = GetPlane(Height, Width, 10 );
            double[,] matrix3 = SummMatrix(matrix1, matrix2);

            double[,] image1 = GetCos(matrix3);
            if (rec != null) rec.Invoke(image1);
            Thread.Sleep((int)(1000*1/ FPS));
            double[,] image2 = GetCos(matrix3, Math.PI / 2);
            if (rec != null) rec.Invoke(image2);
            Thread.Sleep((int)(1000 * 1 / FPS));
            double[,] image3 = GetCos(matrix3, Math.PI);
            if (rec != null) rec.Invoke(image3);
            Thread.Sleep((int)(1000 * 1 / FPS));
            double[,] image4 = GetCos(matrix3, 3 * Math.PI / 2);
            if (rec != null) rec.Invoke(image4);
            Thread.Sleep((int)(1000 * 1 / FPS));
        }

        public void GetImage()
        {
            double rate = 0;
            while (true)
            {
                get_image(rate);
                rate += 0.02;
            }

        }
        #endregion

        public delegate void ImageReciever(double[,] image);

        public event ImageReciever rec;

        public double FPS = 25;
        public int Height = 1920;
        public int Width =1024;

    }

}
