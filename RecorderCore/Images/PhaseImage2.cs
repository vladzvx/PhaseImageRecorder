using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore.Images
{
    public class HilbertPhaseImage2: PhaseImage2
    {
        static double[,] filter;
        public HilbertPhaseImage2(byte[,,] image, int level, double Wavelength):
            base(image,level,Wavelength)
        {

        }
        public void Save(string path)
        {
            try
            {
                List<string> lines = new List<string>();
                for (int i = 0; i < images[0].GetUpperBound(0) + 1; i++)
                {
                    string line = "";
                    for (int j = 0; j < images[0].GetUpperBound(1) + 1; j++)
                    {
                        line += ((int)images[0][i, j]).ToString() + ";";
                    }
                    lines.Add(line);
                }
                File.WriteAllLines(path + ".csv", lines);
            }
            catch (Exception ex)
            {

            }
            
        }
        public double[,] CreateFilterIfNeed(int size0, int size1)
        {
            if(filter==null|| size0 != images[0].GetUpperBound(0) + 1 || size1 != images[0].GetUpperBound(1) + 1)
                filter = Tools.CreateHilbertFilter1(size0, size1);
            return filter;
        }
        public override void Calc()
        {
            int size0 = images[0].GetUpperBound(0) + 1;
            int size1 = images[0].GetUpperBound(1) + 1;
            Complex[,] test_image = Complex.CreateComplexArray(images[0]);
            FourierTransform.FFT2(test_image, FourierTransform.Direction.Forward);
            filter = CreateFilterIfNeed(size0, size1);
            Complex.ApplyFilter(test_image, filter);
            FourierTransform.FFT2(test_image, FourierTransform.Direction.Backward);
            images[0] = Tools.CalculatePhaseImageByHilbert(test_image);
        }
    }
    public class PhaseImage2
    {
        private object locker = new object();
        public List<byte[,,]> source_images=new List<byte[,,]>();
        public List<byte[,,]> _images = new List<byte[,,]>();
        public List<double[,]> images = new List<double[,]>();
        public int level = 0;
        public double Wavelength=1;
        public PhaseImage2(byte[,,] image, int level,double Wavelength)
        {
            _images.Add(image);
            this.level = level;
            this.Wavelength = Wavelength;
        }

        public virtual void Convert()
        {
            lock (locker)
            {
                foreach (byte[,,] image in _images)
                {
                    int size0 = image.GetUpperBound(0) + 1;
                    int size1 = image.GetUpperBound(1) + 1;
                    double[,] nImage = new double[size0, size1];
                    images.Add(nImage);
                    Parallel.For(0, size0 , (i) =>
                    {
                        for (int j = 0; j < size1; j++)
                        {
                            nImage[i, j] = (double)image[i, j, level];
                        }
                    });
                }
            }

        }

        public virtual void ReverseConvert()
        {
            lock (locker)
            {
                source_images = new List<byte[,,]>(_images);
                _images = new List<byte[,,]>();
                foreach (double[,] image in images)
                {
                    double max = 0;
                    double min = 0;
                    int size0 = image.GetUpperBound(0) + 1;
                    int size1 = image.GetUpperBound(1) + 1;

                    for (int i = 0; i < size0; i++)
                    {
                        for (int j = 0; j < size1; j++)
                        {
                            image[i, j] = Math.Round(image[i, j] / Math.PI * Wavelength, 0);
                            double val1 = image[i, j];
                            if (val1 < min) min = val1;
                            if (val1 > max) max = val1;

                        }
                    }

                    byte[,,] nImage = new byte[size0, size1,3];
                    _images.Add(nImage);
                    Parallel.For(0, size0 , (i) =>
                    {
                        for (int j = 0; j < size1; j++)
                        {
                            byte val = (byte)(254 * (image[i, j] - min) / (max - min));
                            nImage[i, j, 0] = val;
                            nImage[i, j, 1] = val;
                            nImage[i, j, 2] = val;
                        }
                    });
                }
            }

        }

        public virtual void Calc()
        {

        }

        public virtual void Unwrapp()
        {
            Unwrapping3 unwrapper = new Unwrapping3(images[0]);
            foreach (double[,] image in images)
            {
                unwrapper.UnwrapParallel(image, out var t);
            }
        }
    }
}
