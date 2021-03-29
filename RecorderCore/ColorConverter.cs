using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public static class ColorConverter
    {
        private static byte CustomConvertor(double value)
        {
            if (value < 0) return 0;
            else if (value > 255) return 255;
            else return (byte)value;
        }
        private static byte Red(double value)
        {
            return CustomConvertor(-370 + 2.5 * value);
        }
        private static byte Blue(double value)
        {
            if (value < 50)
                return CustomConvertor(105 + 3 * value);
            else
                return CustomConvertor(325 - 1.5 * value);
        }
        private static byte Green(double value)
        {
            return CustomConvertor(-150 + 3 * value);
        }
        public static byte[,,] ConvertTluck(double[,] image)
        {
            ImageSource.subtract_min(image);
            double max = ImageSource.max(image);
            int size0 = image.GetUpperBound(0) + 1;
            int size1 = image.GetUpperBound(1) + 1;
            byte[,,] im = new byte[size0, size1, 3];
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    double value = Math.Round(image[i, j] / (max) * 255, 0);
                    im[i, j, 0] = Blue(value);//blue
                    im[i, j, 1] = Green(value);//green;
                    im[i, j, 2] = Red(value);//red;
                }
            }
            return im;
        
        }

        public static byte[,,] ConvertTluckParallel(double[,] image)
        {
            ImageSource.subtract_min(image);
            double max = ImageSource.max(image);
            int size0 = image.GetUpperBound(0) + 1;
            int size1 = image.GetUpperBound(1) + 1;
            byte[,,] im = new byte[size0, size1, 3];
            Parallel.For(0, size0, (i) =>
            {
                for (int j = 0; j < size1; j++)
                {
                    double value = Math.Round(image[i, j] / (max) * 255, 0);
                    im[i, j, 0] = Blue(value);//blue
                    im[i, j, 1] = Green(value);//green;
                    im[i, j, 2] = Red(value);//red;
                }
            });
            return im;

        }
    }
}
