using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public static class Unwrapping
    {
        static double treshhold = Math.PI/2;
        static double wrapStep = Math.PI;
        // gamma function in the paper
        public static double gamma(double pixel_value)
        {
            double wrapped_pixel_value;
            if (pixel_value > treshhold)
                wrapped_pixel_value = pixel_value - wrapStep;
            else if (pixel_value < -treshhold)
                wrapped_pixel_value = pixel_value + wrapStep;
            else
                wrapped_pixel_value = pixel_value;
            return wrapped_pixel_value;
        }

        public static int find_wrap(double pixelL_value, double pixelR_value)
        {
            double difference;
            int wrap_value;
            difference = pixelL_value - pixelR_value;

            if (difference > wrapStep)
                wrap_value = -1;
            else if (difference < wrapStep)
                wrap_value = 1;
            else
                wrap_value = 0;

            return wrap_value;
        }


        public static double[,] ReliabilityCalc(double[,] image)//todo проверки выходов за диапазоны
        {
            int size0 = image.GetUpperBound(0);
            int size1 = image.GetUpperBound(1);
            double[,] result = new double[size0-1,size1-1];
            for (int i = 1; i < size0-1; i++)
            {
                for (int j = 1; j < size1-1; j++)
                {
                    double H = gamma(image[i - 1, j] - image[i, j]) - gamma(image[i, j] - image[i + 1, j]); 
                    double V = gamma(image[i, j-1] - image[i, j]) - gamma(image[i, j] - image[i , j+1]); 
                    double D1 = gamma(image[i-1, j-1] - image[i, j]) - gamma(image[i, j] - image[i+1 , j+1]); 
                    double D2 = gamma(image[i-1, j+1] - image[i, j]) - gamma(image[i, j] - image[i+1 , j-1]);
                    result[i, j] = 1/Math.Sqrt(H*H+V*V+D1*D1+D2*D2);
                }
            }
            return result;

        }

        public static double[,] HorizontEdgesCalc(double[,] reliability)//todo проверки выходов за диапазоны
        {
            int size0 = reliability.GetUpperBound(0);
            int size1 = reliability.GetUpperBound(1);
            double[,] result = new double[size0+1, size1];
            for (int i = 0; i < size0+1; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    result[i, j] = reliability[i, j] + reliability[i, j + 1];
                }
            }
            return result;

        }
        public static double[,] VerticalEdgesCalc(double[,] reliability)//todo проверки выходов за диапазоны
        {
            int size0 = reliability.GetUpperBound(0);
            int size1 = reliability.GetUpperBound(1);
            double[,] result = new double[size0, size1+1];
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1+1; j++)
                {
                    result[i, j] = reliability[i, j] + reliability[i+1, j];
                }
            }
            return result;

        }

    }
}
