using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public static class Unwrapping2
    {
        static double PI = Math.PI / 2;
        static double TWOPI = 2 * PI;
        static pixel[,] pixels;
        static int size0;// = image.GetUpperBound(0);
        static int size1;// = image.GetUpperBound(1);
        static List<edge> _edges = new List<edge>();
        public static void SetParamsByImage(double[,] image)
        {
            size0 = image.GetUpperBound(0);
            size1 = image.GetUpperBound(1);
            pixels = new pixel[size0 + 1, size1 + 1];
            for (int i = 0; i <= size0; i++)
            {
                for (int j = 0; j <= size1; j++)
                {
                    pixels[i, j] = new pixel() { reliability = 0, i = i, j = j };
                }
            }
            _edges = new List<edge>();
            for (int i = 1; i <= size0; i++)
            {
                for (int j = 1; j <= size1; j++)
                {
                    _edges.Add(new edge() {pixel1 = pixels[i - 1, j], pixel2 = pixels[i, j]});
                    _edges.Add(new edge() {pixel1 = pixels[i, j - 1], pixel2 = pixels[i, j]});
                }
            }
        }
        private class pixel
        {
            public pixel()
            {
                this.head = this;
                this.last = this;
            }
            public pixel head;
            public pixel next;
            public pixel last;
            //public double[,] image;
            public double reliability;
            public int i;
            public int j;

            public bool isProcessed;
            public int number_of_pixels_in_group;
            public int increment;
        }

        private class edge
        {
            public double reaibility;
            public pixel pixel1;
            public pixel pixel2;
            public int increment;
        }

        private class edgeComparer : IComparer<edge>
        {
            public int Compare(edge x, edge y)
            {
                if (x.reaibility < y.reaibility)
                {
                    return -1;
                }
                else if (x.reaibility > y.reaibility)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }


        // gamma function in the paper
        private static double gamma(double pixel_value)
        {
            double wrapped_pixel_value;
            if (pixel_value > PI)
                wrapped_pixel_value = pixel_value - TWOPI;
            else if (pixel_value < -PI)
                wrapped_pixel_value = pixel_value + TWOPI;
            else
                wrapped_pixel_value = pixel_value;
            return wrapped_pixel_value;
        }

        private static int find_wrap(double pixelL_value, double pixelR_value)
        {
            double difference;
            int wrap_value;
            difference = pixelL_value - pixelR_value;

            if (difference > PI)
                wrap_value = -1;
            else if (difference < -PI)
                wrap_value = 1;
            else
                wrap_value = 0;

            return wrap_value;
        }

        public static void Unwrap(double[,] image)//todo проверки выходов за диапазоны
        {
            for (int i = 1; i < size0; i++)
            {
                for (int j = 1; j < size1; j++)
                {
                    double H = gamma(image[i, j - 1] - image[i, j]) - gamma(image[i, j] - image[i, j + 1]); 
                    double V = gamma(image[i - 1, j] - image[i, j]) - gamma(image[i, j] - image[i + 1, j]);
                    double D1 = gamma(image[i - 1, j - 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, j + 1]);
                    double D2 = gamma(image[i - 1, j + 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, j - 1]);
                    pixels[i, j].reliability = (H * H + V * V + D1 * D1 + D2 * D2);
                }
            }

            for (int i = 0; i <= size0; i++)
            {
                pixels[i, 0].reliability = 0;
                pixels[i, size1].reliability = 0;
            }
            for (int j = 0; j <= size1; j++)
            {
                pixels[0, j].reliability = 0;
                pixels[size0, j].reliability = 0;
            }

            foreach (edge _edge in _edges)
            {
                _edge.increment = find_wrap(image[_edge.pixel1.i, _edge.pixel1.j], image[_edge.pixel2.i, _edge.pixel2.j]);
                _edge.reaibility = _edge.pixel1.reliability + _edge.pixel2.reliability;
            }

            _edges.Sort(new edgeComparer());

            foreach (edge _edge in _edges)
            {
                pixel PIXEL1 = _edge.pixel1;
                pixel PIXEL2 = _edge.pixel2;
                pixel group1;
                pixel group2;
                int incremento;

                if (PIXEL2.head != PIXEL1.head)
                {
                    // PIXELM 2 is alone in its group
                    // merge this pixel with PIXELM 1 group and find the number of 2 pi to add
                    // to or subtract to unwrap it
                    if ((PIXEL2.next == null) && (PIXEL2.head == PIXEL2))
                    {
                        PIXEL1.head.last.next = PIXEL2;
                        PIXEL1.head.last = PIXEL2;
                        (PIXEL1.head.number_of_pixels_in_group)++;
                        PIXEL2.head = PIXEL1.head;
                        PIXEL2.increment = PIXEL1.increment - _edge.increment;
                    }

                    // PIXELM 1 is alone in its group
                    // merge this pixel with PIXELM 2 group and find the number of 2 pi to add
                    // to or subtract to unwrap it
                    else if ((PIXEL1.next == null) && (PIXEL1.head == PIXEL1))
                    {
                        PIXEL2.head.last.next = PIXEL1;
                        PIXEL2.head.last = PIXEL1;
                        (PIXEL2.head.number_of_pixels_in_group)++;
                        PIXEL1.head = PIXEL2.head;
                        PIXEL1.increment = PIXEL2.increment + _edge.increment;
                    }

                    // PIXELM 1 and PIXELM 2 both have groups
                    else
                    {
                        group1 = PIXEL1.head;
                        group2 = PIXEL2.head;
                        // if the no. of pixels in PIXELM 1 group is larger than the
                        // no. of pixels in PIXELM 2 group.  Merge PIXELM 2 group to
                        // PIXELM 1 group and find the number of wraps between PIXELM 2
                        // group and PIXELM 1 group to unwrap PIXELM 2 group with respect
                        // to PIXELM 1 group.  the no. of wraps will be added to PIXELM 2
                        // group in the future
                        if (group1.number_of_pixels_in_group >
                            group2.number_of_pixels_in_group)
                        {
                            // merge PIXELM 2 with PIXELM 1 group
                            group1.last.next = group2;
                            group1.last = group2.last;
                            group1.number_of_pixels_in_group =
                                    group1.number_of_pixels_in_group +
                                    group2.number_of_pixels_in_group;
                            incremento =
                                    PIXEL1.increment - _edge.increment - PIXEL2.increment;
                            // merge the other pixels in PIXELM 2 group to PIXELM 1 group
                            while (group2 != null)
                            {
                                group2.head = group1;
                                group2.increment += incremento;
                                group2 = group2.next;
                            }
                        }

                        // if the no. of pixels in PIXELM 2 group is larger than the
                        // no. of pixels in PIXELM 1 group.  Merge PIXELM 1 group to
                        // PIXELM 2 group and find the number of wraps between PIXELM 2
                        // group and PIXELM 1 group to unwrap PIXELM 1 group with respect
                        // to PIXELM 2 group.  the no. of wraps will be added to PIXELM 1
                        // group in the future
                        else
                        {
                            // merge PIXELM 1 with PIXELM 2 group
                            group2.last.next = group1;
                            group2.last = group1.last;
                            group2.number_of_pixels_in_group =
                                    group2.number_of_pixels_in_group +
                                    group1.number_of_pixels_in_group;
                            incremento =
                                    PIXEL2.increment + _edge.increment - PIXEL1.increment;
                            // merge the other pixels in PIXELM 2 group to PIXELM 1 group
                            while (group1 != null)
                            {
                                group1.head = group2;
                                group1.increment += incremento;
                                group1 = group1.next;
                            }  // while

                        }  // else
                    }  // else
                }  // if


            }

            foreach (pixel pixel in pixels)
            {
                image[pixel.i, pixel.j] += TWOPI * ((double)pixel.increment);
            }
        }
    }

    public static class Unwrapping
    {
        static double PI = Math.PI/2;
        static double TWOPI = 2*PI;

        internal class pixel
        {
            public pixel()
            {
                this.head = this;
                this.last = this;
            }
            public pixel head;
            public pixel next;
            public pixel last;
            public double[,] image;
            public double reliability;
            public int i;
            public int j;

            public bool isProcessed;
            public int number_of_pixels_in_group;
            public int increment;
        }

        internal class edge
        {
            public double reaibility;
            public pixel pixel1;
            public pixel pixel2;
            public int increment;
        }

        internal class edgeComparer : IComparer<edge>
        {
            public int Compare(edge x, edge y)
            {
                if (x.reaibility < y.reaibility)
                {
                    return -1;
                }
                else if (x.reaibility > y.reaibility)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }


        // gamma function in the paper
        private static double gamma(double pixel_value)
        {
            double wrapped_pixel_value;
            if (pixel_value > PI)
                wrapped_pixel_value = pixel_value - TWOPI;
            else if (pixel_value < -PI)
                wrapped_pixel_value = pixel_value + TWOPI;
            else
                wrapped_pixel_value = pixel_value;
            return wrapped_pixel_value;
        }

        private static int find_wrap(double pixelL_value, double pixelR_value)
        {
            double difference;
            int wrap_value;
            difference = pixelL_value - pixelR_value;

            if (difference > PI)
                wrap_value = -1;
            else if (difference < -PI)
                wrap_value = 1;
            else
                wrap_value = 0;

            return wrap_value;
        }
        public static void Unwrap(double[,] image)//todo проверки выходов за диапазоны
        {
            int size0 = image.GetUpperBound(0);
            int size1 = image.GetUpperBound(1);
            pixel[,] pixels = new pixel[size0+1, size1+1];
            List<edge> edges = new List<edge>();
            for (int i = 1; i < size0; i++)
            {
                for (int j = 1; j < size1; j++)
                {
                    double H = gamma(image[i, j-1] - image[i, j]) - gamma(image[i, j] - image[i, j+1]); //wrap(image[i,j-1]-image[i,j])-wrap(image[i,j]-image[i,j+1])	
                    double V = gamma(image[i - 1, j] - image[i, j]) - gamma(image[i, j] - image[i + 1, j]);
                    double D1 = gamma(image[i-1, j-1] - image[i, j]) - gamma(image[i, j] - image[i+1 , j+1]); 
                    double D2 = gamma(image[i-1, j+1] - image[i, j]) - gamma(image[i, j] - image[i+1 , j-1]);
                    //double R = 1 / Math.Sqrt(H * H + V * V + D1 * D1 + D2 * D2);
                    double R = (H * H + V * V + D1 * D1 + D2 * D2);
                    //result[i, j] = R;
                    pixel temp = new pixel() { reliability = R, image = image, i = i, j = j };
                    pixels[i, j] = temp;
                }
            }

            for (int i = 0; i <= size0 ; i++)
            {
                int j1 = 0;
                pixel temp1 = new pixel() { reliability = 0, image = image, i = i, j = j1 };
                pixels[i, j1] = temp1;
                int j2 = size1;
                pixel temp2 = new pixel() { reliability = 0, image = image, i = i, j = j2 };
                pixels[i, j2] = temp2;
            }

            for (int j = 0; j <= size1; j++)
            {
                int i1 = 0;
                pixel temp1 = new pixel() { reliability = 0, image = image, i = i1, j = j };
                pixels[i1, j] = temp1;
                int i2 = size0;
                pixel temp2 = new pixel() { reliability = 0, image = image, i = i2, j = j };
                pixels[i2, j] = temp2;
            }


            for (int i = 1; i <= size0; i++)
            {
                for (int j = 1; j <= size1; j++)
                {
                    {
                        pixel pixel1 = pixels[i - 1, j];
                        pixel pixel2 = pixels[i, j];
                        int _increment = find_wrap(pixel1.image[pixel1.i, pixel1.j], pixel2.image[pixel2.i, pixel2.j]);
                        double r = pixel1.reliability + pixel2.reliability;
                      //  if (r > 0)
                        {
                            edge edge = new edge()
                            {
                                pixel1 = pixel1,
                                pixel2 = pixel2,
                                reaibility = r,
                                increment = _increment
                            };
                            edges.Add(edge);
                        }

                    }

                    {

                        pixel pixel1 = pixels[i, j - 1];
                        pixel pixel2 = pixels[i, j];
                        int _increment = find_wrap(pixel1.image[pixel1.i, pixel1.j], pixel2.image[pixel2.i, pixel2.j]);
                        double r = pixel1.reliability + pixel2.reliability;
                       // if (r > 0)
                        {
                            edge edge = new edge()
                            {

                                pixel1 = pixel1,
                                pixel2 = pixel2,
                                reaibility = r,
                                increment = _increment
                            };
                            edges.Add(edge);
                        }

                    }


                }
            }


            double val = ((double)edges.Count) / ((double)pixels.Length);
            edges.Sort(new edgeComparer());
            foreach (edge _edge in edges)
            {
                pixel PIXEL1 = _edge.pixel1;
                pixel PIXEL2 = _edge.pixel2;
                pixel group1;
                pixel group2;
                int incremento;

                if (PIXEL2.head != PIXEL1.head)
                {
                    // PIXELM 2 is alone in its group
                    // merge this pixel with PIXELM 1 group and find the number of 2 pi to add
                    // to or subtract to unwrap it
                    if ((PIXEL2.next == null) && (PIXEL2.head == PIXEL2))
                    {
                        PIXEL1.head.last.next = PIXEL2;
                        PIXEL1.head.last = PIXEL2;
                        (PIXEL1.head.number_of_pixels_in_group)++;
                        PIXEL2.head = PIXEL1.head;
                        PIXEL2.increment = PIXEL1.increment - _edge.increment;
                    }

                    // PIXELM 1 is alone in its group
                    // merge this pixel with PIXELM 2 group and find the number of 2 pi to add
                    // to or subtract to unwrap it
                    else if ((PIXEL1.next == null) && (PIXEL1.head == PIXEL1))
                    {
                        PIXEL2.head.last.next = PIXEL1;
                        PIXEL2.head.last = PIXEL1;
                        (PIXEL2.head.number_of_pixels_in_group)++;
                        PIXEL1.head = PIXEL2.head;
                        PIXEL1.increment = PIXEL2.increment + _edge.increment;
                    }

                    // PIXELM 1 and PIXELM 2 both have groups
                    else
                    {
                        group1 = PIXEL1.head;
                        group2 = PIXEL2.head;
                        // if the no. of pixels in PIXELM 1 group is larger than the
                        // no. of pixels in PIXELM 2 group.  Merge PIXELM 2 group to
                        // PIXELM 1 group and find the number of wraps between PIXELM 2
                        // group and PIXELM 1 group to unwrap PIXELM 2 group with respect
                        // to PIXELM 1 group.  the no. of wraps will be added to PIXELM 2
                        // group in the future
                        if (group1.number_of_pixels_in_group >
                            group2.number_of_pixels_in_group)
                        {
                            // merge PIXELM 2 with PIXELM 1 group
                            group1.last.next = group2;
                            group1.last = group2.last;
                            group1.number_of_pixels_in_group =
                                    group1.number_of_pixels_in_group +
                                    group2.number_of_pixels_in_group;
                            incremento =
                                    PIXEL1.increment - _edge.increment - PIXEL2.increment;
                            // merge the other pixels in PIXELM 2 group to PIXELM 1 group
                            while (group2 != null)
                            {
                                group2.head = group1;
                                group2.increment += incremento;
                                group2 = group2.next;
                            }
                        }

                        // if the no. of pixels in PIXELM 2 group is larger than the
                        // no. of pixels in PIXELM 1 group.  Merge PIXELM 1 group to
                        // PIXELM 2 group and find the number of wraps between PIXELM 2
                        // group and PIXELM 1 group to unwrap PIXELM 1 group with respect
                        // to PIXELM 2 group.  the no. of wraps will be added to PIXELM 1
                        // group in the future
                        else
                        {
                            // merge PIXELM 1 with PIXELM 2 group
                            group2.last.next = group1;
                            group2.last = group1.last;
                            group2.number_of_pixels_in_group =
                                    group2.number_of_pixels_in_group +
                                    group1.number_of_pixels_in_group;
                            incremento =
                                    PIXEL2.increment + _edge.increment - PIXEL1.increment;
                            // merge the other pixels in PIXELM 2 group to PIXELM 1 group
                            while (group1 != null)
                            {
                                group1.head = group2;
                                group1.increment += incremento;
                                group1 = group1.next;
                            }  // while

                        }  // else
                    }  // else
                }  // if


            }

            foreach (pixel pixel in pixels)
            {
                if (pixel.i == 0 && pixel.j == 0)
                {
                    int q = 0;
                }
                pixel.image[pixel.i,pixel.j] += TWOPI * ((double)pixel.increment);
            }
            //return result;
        }
    }
}
