using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore.Images
{
    public class PhaseImage2
    {
        private object locker = new object();
        internal List<byte[,,]> _images = new List<byte[,,]>();
        internal List<double[,]> images = new List<double[,]>();
        internal int level = 0;
        public PhaseImage2(byte[,,] image, int level)
        {
            _images.Add(image);
            this.level = level;
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
                    Parallel.For(0, size0 + 1, (i) =>
                    {
                        for (int j = 0; j <= size1; j++)
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
                _images = new List<byte[,,]>();
                foreach (double[,] image in images)
                {
                    int size0 = image.GetUpperBound(0) + 1;
                    int size1 = image.GetUpperBound(1) + 1;
                    byte[,,] nImage = new byte[size0, size1,3];
                    _images.Add(nImage);
                    Parallel.For(0, size0 + 1, (i) =>
                    {
                        for (int j = 0; j <= size1; j++)
                        {
                            byte val = (byte)image[i, j];
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
