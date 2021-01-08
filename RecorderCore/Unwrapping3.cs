using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public class Unwrapping3
    {
        double PI = Math.PI / 2;
        double TWOPI;
        pixel[,] pixels;

        int size0;
        int size1;
        edge[] _edges;
        double[] edges_reliabilities;


        public Unwrapping3(double[,] image)
        {
            TWOPI = 2 * PI;
            UpdateParams(image);
        }
        public void UpdateParams(double[,] image)
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
            List<edge> edges = new List<edge>() { new edge() { pixel1 = pixels[0, 0], pixel2 = pixels[0, 1] } ,
                new edge() { pixel1 = pixels[0, 0], pixel2 = pixels[1, 0] }};

            for (int i = 1; i <= size0; i++)
            {
                for (int j = 1; j <= size1; j++)
                {
                    edges.Add(new edge() { pixel1 = pixels[i - 1, j], pixel2 = pixels[i, j] });
                    edges.Add(new edge() { pixel1 = pixels[i, j - 1], pixel2 = pixels[i, j] });
                }
            }
            _edges = edges.ToArray();
            edges_reliabilities = (from e in _edges select e.reaibility).ToArray();
        }
        internal class pixel
        {
            public void Refresh()
            {
                this.head = this;
                this.last = this;
                this.next = null;
                number_of_pixels_in_group = 0;
            }
            public pixel()
            {
                this.head = this;
                this.last = this;
            }
            public double value;
            public pixel head;
            public pixel next;
            public pixel last;
            public double reliability;
            public int i;
            public int j;

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

        // gamma function in the paper
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double gamma(double pixel_value)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int find_wrap(double pixelL_value, double pixelR_value)
        {
            double difference = pixelL_value - pixelR_value;

            if (difference > PI)
                return -1;
            else if (difference < -PI)
                return 1;
            else
                return 0;
        }

        public void Unwrap(double[,] image, out UwrReport score)//todo проверки выходов за диапазоны
        {
            score = new UwrReport();
            DateTime dt1 = DateTime.UtcNow;
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

            for (int i = 1; i < size0; i++)//left and right borders
            {
                int j = 0;

                double H = gamma(image[i, size1 - 1] - image[i, j]) - gamma(image[i, j] - image[i, j + 1]);
                double V = gamma(image[i - 1, j] - image[i, j]) - gamma(image[i, j] - image[i + 1, j]);
                double D1 = gamma(image[i - 1, size1 - 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, j + 1]);
                double D2 = gamma(image[i - 1, j + 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, size1 - 1]);
                pixels[i, 0].reliability = (H * H + V * V + D1 * D1 + D2 * D2);

                j = size1;
                H = gamma(image[i, j - 1] - image[i, j]) - gamma(image[i, j] - image[i, 0]);
                V = gamma(image[i - 1, j] - image[i, j]) - gamma(image[i, j] - image[i + 1, j]);
                D1 = gamma(image[i - 1, j - 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, 0]);
                D2 = gamma(image[i - 1, 0] - image[i, j]) - gamma(image[i, j] - image[i + 1, j - 1]);
                pixels[i, size1].reliability = (H * H + V * V + D1 * D1 + D2 * D2);
            }

            for (int j = 1; j < size1; j++)
            {
                int i = 0;
                double H = gamma(image[i, j - 1] - image[i, j]) - gamma(image[i, j] - image[i, j + 1]);
                double V = gamma(image[size0, j] - image[i, j]) - gamma(image[i, j] - image[i + 1, j]);
                double D1 = gamma(image[size0, j - 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, j + 1]);
                double D2 = gamma(image[size0, j + 1] - image[i, j]) - gamma(image[i, j] - image[i + 1, j - 1]);
                pixels[0, j].reliability = (H * H + V * V + D1 * D1 + D2 * D2);
                //pixels[i, j].value = image[i, j];

                i = size0;
                H = gamma(image[i, j - 1] - image[i, j]) - gamma(image[i, j] - image[i, j + 1]);
                V = gamma(image[i - 1, j] - image[i, j]) - gamma(image[i, j] - image[0, j]);
                D1 = gamma(image[i - 1, j - 1] - image[i, j]) - gamma(image[i, j] - image[0, j + 1]);
                D2 = gamma(image[i - 1, j + 1] - image[i, j]) - gamma(image[i, j] - image[0, j - 1]);
                pixels[size0, j].reliability = (H * H + V * V + D1 * D1 + D2 * D2);
            }

            score.GammasCalc = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            dt1 = DateTime.UtcNow;
            for (int i = 0; i < _edges.Length; i++)
            {
                _edges[i].increment = find_wrap(image[_edges[i].pixel1.i, _edges[i].pixel1.j], image[_edges[i].pixel2.i, _edges[i].pixel2.j]);
                edges_reliabilities[i] = Math.Round(_edges[i].pixel1.reliability + _edges[i].pixel2.reliability, 0);
            }
            score.EdgesCalc = DateTime.UtcNow.Subtract(dt1).TotalSeconds;

            dt1 = DateTime.UtcNow;
            //Array.Sort(_edges, new edgeComparer());

            Sortings.ParallelQuickSort(edges_reliabilities, _edges);

            score.Sorting = DateTime.UtcNow.Subtract(dt1).TotalSeconds;

            dt1 = DateTime.UtcNow;
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

            score.PathFinding = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            dt1 = DateTime.UtcNow;



            foreach (pixel pixel in pixels)
            {
                image[pixel.i, pixel.j] += TWOPI * ((double)pixel.increment);
            }

            //Parallel.For(0, size0+1, (i) => 
            //{ 
            //    for (int j = 0; j <= size1; j++)
            //    {
            //        image[pixels[i, j].i, pixels[i, j].j] += TWOPI * ((double)pixels[i, j].increment);
            //    }
            //});


            score.Unwrap = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
            dt1 = DateTime.UtcNow;
            foreach (pixel pixel in pixels)
            {
                pixel.Refresh();
            }
            score.Refresh = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
        }
    }

}
