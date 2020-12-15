using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
namespace RecorderCore
{
    public static class Unwrapping2
    {


        static double PI = Math.PI;
        static double TWOPI = (2 * PI);
        static Random rnd = new Random();

        // TODO: remove global variables
        // TODO: make thresholds independent

        static bool NOMASK = false;
        static bool MASK = true;


        public class params_t
        {
            public double mod;
            public int x_connectivity;
            public int y_connectivity;
            public int no_of_edges;
        }


        // PIXELM information
        public class PIXELM
        {
            public int increment;  // No. of 2pi to add to the pixel to unwrap it
            public int number_of_pixels_in_group;  // No. of pixel in the pixel group
            public double value;  // value of the pixel
            public double reliability;
            public bool input_mask;  // 0 pixel is masked. NOMASK pixel is not masked
            public bool extended_mask;  // 0 pixel is masked. NOMASK pixel is not masked
            public int group;  // group No.
            public int new_group;
            public PIXELM head;  // pointer to the first pixel in the group in the linked
                                 // list
            public PIXELM last;  // pointer to the last pixel in the group
            public PIXELM next;  // pointer to the next pixel in the group
        }


        // the EDGE is the line that connects two pixels.
        // if we have S pixels, then we have S horizontal edges and S vertical edges
        public class EDGE
        {
            public double reliab;  // reliabilty of the edge and it depends on the two pixels
            public PIXELM pointer_1;  // pointer to the first pixel
            public PIXELM pointer_2;  // pointer to the second pixel
            public int increment;  // No. of 2pi to add to one of the pixels to
                            // unwrap it with respect to the second
        }


        //---------------start quicker_sort algorithm --------------------------------

        public class edgeComparer : IComparer<EDGE>
        {
            public int Compare(EDGE x, EDGE y)
            {
                if (x.reliab < y.reliab)
                {
                    return 1;
                }
                else if (x.reliab > y.reliab)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        //--------------end quicker_sort algorithm -----------------------------------

        //--------------------start initialize pixels ----------------------------------
        // initialize pixels. See the explination of the pixel class above.
        // initially every pixel is assumed to belong to a group consisting of only
        // itself
        static void initialisePIXELs(double[,] wrapped_image, bool[,] input_mask, bool[,] extended_mask, PIXELM pixel, 
            int image_width, int image_height)
            {
            PIXELM pixel_pointer = pixel;
            int i, j;


            for (i = 0; i < image_height; i++)
            {
                for (j = 0; j < image_width; j++)
                {
                    pixel_pointer.increment = 0;
                    pixel_pointer.number_of_pixels_in_group = 1;
                    pixel_pointer.value = wrapped_image[i, j];
                    pixel_pointer.reliability = rnd.NextDouble();
                    pixel_pointer.input_mask = input_mask[i, j];
                    pixel_pointer.extended_mask = extended_mask[i, j];
                    pixel_pointer.head = pixel_pointer;
                    pixel_pointer.last = pixel_pointer;
                    pixel_pointer.next = null;
                    pixel_pointer.new_group = 0;
                    pixel_pointer.group = -1;
                }
            }
        }
//-------------------end initialize pixels -----------

// gamma function in the paper
        static double wrap(double pixel_value)
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

// pixelL_value is the left pixel,  pixelR_value is the right pixel
        static int find_wrap(double pixelL_value, double pixelR_value)
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


        static void calculate_reliability(double[,] image, PIXELM[,] pixels, int image_width,
                           int image_height, params_t _params)
        {


            for (int i = 1; i < image_height - 1; ++i)
            {
                for (int j = 1; j < image_width - 1; ++j)
                {
                    PIXELM pixel = pixels[i, j];
                    if (pixel.extended_mask == NOMASK)
                    {
                        double H = wrap(image[i - 1, j] - image[i, j]) - wrap(image[i, j] - image[i + 1, j]);
                        double V = wrap(image[i, j - 1] - image[i, j]) - wrap(image[i, j] - image[i, j + 1]);
                        double D1 = wrap(image[i - 1, j - 1] - image[i, j]) - wrap(image[i, j] - image[i + 1, j + 1]);
                        double D2 = wrap(image[i - 1, j + 1] - image[i, j]) - wrap(image[i, j] - image[i + 1, j - 1]);
                        double R = (H * H + V * V + D1 * D1 + D2 * D2);
                        pixels[i - 1, j - 1].reliability = R;
                    }
                }
            }
        }

        /*
        if (_params.x_connectivity == 1) {
                // calculating the reliability for the left border of the image
                PIXELM pixel_pointer = pixel + image_width;
        double WIP = wrappedImage + image_width;

        for (i = 1; i < image_height - 1; ++i)
        {
            if (pixel_pointer.extended_mask == NOMASK)
            {
                H = wrap((WIP + image_width - 1) - WIP) - wrap(WIP - (WIP + 1));
                V = wrap((WIP - image_width) - WIP) -
                    wrap(WIP - (WIP + image_width));
                D1 = wrap((WIP - 1) - WIP) -
                     wrap(WIP - (WIP + image_width_plus_one));
                D2 = wrap((WIP - image_width_minus_one) - WIP) -
                     wrap(WIP - (WIP + 2  image_width - 1));
                pixel_pointer.reliability = H  H + V  V + D1  D1 + D2  D2;
            }
            pixel += image_width;
            WIP += image_width;
        }

        // calculating the reliability for the right border of the image
        pixel_pointer = pixel + 2  image_width - 1;
        WIP = wrappedImage + 2  image_width - 1;

        for (i = 1; i < image_height - 1; ++i)
        {
            if (pixel_pointer.extended_mask == NOMASK)
            {
                H = wrap((WIP - 1) - WIP) -
                    wrap(WIP - (WIP - image_width_minus_one));
                V = wrap((WIP - image_width) - WIP) -
                    wrap(WIP - (WIP + image_width));
                D1 = wrap((WIP - image_width_plus_one) - WIP) -
                     wrap(WIP - (WIP + 1));
                D2 = wrap((WIP - 2  image_width - 1) - WIP) -
                     wrap(WIP - (WIP + image_width_minus_one));
                pixel_pointer.reliability = H  H + V  V + D1  D1 + D2  D2;
            }
            pixel_pointer += image_width;
            WIP += image_width;
        }
    }

    if (params.y_connectivity == 1) {
        // calculating the reliability for the top border of the image
        PIXELM pixel_pointer = pixel + 1;
        double WIP = wrappedImage + 1;

        for (i = 1; i < image_width - 1; ++i)
        {
            if (pixel_pointer.extended_mask == NOMASK)
            {
                H = wrap((WIP - 1) - WIP) - wrap(WIP - (WIP + 1));
                V = wrap((WIP + image_width  (image_height - 1)) - WIP) -
                    wrap(WIP - (WIP + image_width));
                D1 = wrap((WIP + image_width  (image_height - 1) - 1) - WIP) -
                     wrap(WIP - (WIP + image_width_plus_one));
                D2 = wrap((WIP + image_width  (image_height - 1) + 1) - WIP) -
                     wrap(WIP - (WIP + image_width_minus_one));
                pixel_pointer.reliability = H  H + V  V + D1  D1 + D2  D2;
            }
            pixel_pointer++;
            WIP++;
        }

        // calculating the reliability for the bottom border of the image
        pixel_pointer = pixel + (image_height - 1)  image_width + 1;
        WIP = wrappedImage + (image_height - 1)  image_width + 1;

        for (i = 1; i < image_width - 1; ++i)
        {
            if (pixel_pointer.extended_mask == NOMASK)
            {
                H = wrap((WIP - 1) - WIP) - wrap(WIP - (WIP + 1));
                V = wrap((WIP - image_width) - WIP) -
                    wrap(WIP - (WIP - (image_height - 1)  (image_width)));
                D1 = wrap((WIP - image_width_plus_one) - WIP) -
                     wrap(WIP - (WIP - (image_height - 1)  (image_width) + 1));
                D2 = wrap((WIP - image_width_minus_one) - WIP) -
                     wrap(WIP - (WIP - (image_height - 1)  (image_width) - 1));
                pixel_pointer.reliability = H  H + V  V + D1  D1 + D2  D2;
            }
            pixel_pointer++;
            WIP++;
        }
        
    


        // calculate the reliability of the horizontal edges of the image
        // it is calculated by adding the reliability of pixel and the relibility of
        // its right-hand neighbour
        // edge is calculated between a pixel and its next neighbour
        static void horizontalEDGEs(PIXELM pixel, EDGE edge, int image_width,
                             int image_height, params_tparams)
        {
            int i, j;
            EDGE edge_pointer = edge;
            PIXELM pixel_pointer = pixel;
            int no_of_edges = params.no_of_edges;

            for (i = 0; i < image_height; i++)
            {
                for (j = 0; j < image_width - 1; j++)
                {
                    if (pixel_pointer.input_mask == NOMASK &&
                        (pixel_pointer + 1).input_mask == NOMASK)
                    {
                        edge_pointer.pointer_1 = pixel_pointer;
                        edge_pointer.pointer_2 = (pixel_pointer + 1);
                        edge_pointer.reliab =
                                pixel_pointer.reliability + (pixel_pointer + 1).reliability;
                        edge_pointer.increment =
                                find_wrap(pixel_pointer.value, (pixel_pointer + 1).value);
                        edge_pointer++;
                        no_of_edges++;
                    }
                    pixel_pointer++;
                }
                pixel_pointer++;
            }
            // construct edges at the right border of the image
            if (params.x_connectivity == 1) {
            pixel_pointer = pixel + image_width - 1;
            for (i = 0; i < image_height; i++)
            {
                if (pixel_pointer.input_mask == NOMASK &&
                    (pixel_pointer - image_width + 1).input_mask == NOMASK)
                {
                    edge_pointer.pointer_1 = pixel_pointer;
                    edge_pointer.pointer_2 = (pixel_pointer - image_width + 1);
                    edge_pointer.reliab = pixel_pointer.reliability +
                                           (pixel_pointer - image_width + 1).reliability;
                    edge_pointer.increment = find_wrap(
                            pixel_pointer.value, (pixel_pointer - image_width + 1).value);
                    edge_pointer++;
                    no_of_edges++;
                }
                pixel_pointer += image_width;
            }
        }
            params.no_of_edges = no_of_edges;
        }

        // calculate the reliability of the vertical edges of the image
        // it is calculated by adding the reliability of pixel and the relibility of
        // its lower neighbour in the image.
        static void verticalEDGEs(PIXELM pixel, EDGE edge, int image_width, int image_height,
                           params_tparams)
        {
            int i, j;
            int no_of_edges = params.no_of_edges;
            PIXELM pixel_pointer = pixel;
            EDGE edge_pointer = edge + no_of_edges;

            for (i = 0; i < image_height - 1; i++)
            {
                for (j = 0; j < image_width; j++)
                {
                    if (pixel_pointer.input_mask == NOMASK &&
                        (pixel_pointer + image_width).input_mask == NOMASK)
                    {
                        edge_pointer.pointer_1 = pixel_pointer;
                        edge_pointer.pointer_2 = (pixel_pointer + image_width);
                        edge_pointer.reliab = pixel_pointer.reliability +
                                               (pixel_pointer + image_width).reliability;
                        edge_pointer.increment = find_wrap(
                                pixel_pointer.value, (pixel_pointer + image_width).value);
                        edge_pointer++;
                        no_of_edges++;
                    }
                    pixel_pointer++;
                }  // j loop
            }  // i loop

            // construct edges that connect at the bottom border of the image
            if (params.y_connectivity == 1) {
            pixel_pointer = pixel + image_width  (image_height - 1);
            for (i = 0; i < image_width; i++)
            {
                if (pixel_pointer.input_mask == NOMASK &&
                    (pixel_pointer - image_width  (image_height - 1)).input_mask ==
                    NOMASK)
                {
                    edge_pointer.pointer_1 = pixel_pointer;
                    edge_pointer.pointer_2 =
                            (pixel_pointer - image_width  (image_height - 1));
                    edge_pointer.reliab =
                            pixel_pointer.reliability +
                            (pixel_pointer - image_width  (image_height - 1)).reliability;
                    edge_pointer.increment = find_wrap(
                            pixel_pointer.value,
                            (pixel_pointer - image_width  (image_height - 1)).value);
                    edge_pointer++;
                    no_of_edges++;
                }
                pixel_pointer++;
            }
        }
            params.no_of_edges = no_of_edges;
        }

        // gather the pixels of the image into groups
        static void gatherPIXELs(EDGE edge, params_tparams)
        {
            int k;
            PIXELM PIXEL1;
            PIXELM PIXEL2;
            PIXELM group1;
            PIXELM group2;
            EDGE pointer_edge = edge;
            int incremento;

            for (k = 0; k < params.no_of_edges; k++) {
            PIXEL1 = pointer_edge.pointer_1;
            PIXEL2 = pointer_edge.pointer_2;

            // PIXELM 1 and PIXELM 2 belong to different groups
            // initially each pixel is a group by it self and one pixel can construct a
            // group
            // no else or else if to this if
            if (PIXEL2.head != PIXEL1.head)
            {
                // PIXELM 2 is alone in its group
                // merge this pixel with PIXELM 1 group and find the number of 2 pi to add
                // to or subtract to unwrap it
                if ((PIXEL2.next == NULL) && (PIXEL2.head == PIXEL2))
                {
                    PIXEL1.head.last.next = PIXEL2;
                    PIXEL1.head.last = PIXEL2;
                    (PIXEL1.head.number_of_pixels_in_group)++;
                    PIXEL2.head = PIXEL1.head;
                    PIXEL2.increment = PIXEL1.increment - pointer_edge.increment;
                }

                // PIXELM 1 is alone in its group
                // merge this pixel with PIXELM 2 group and find the number of 2 pi to add
                // to or subtract to unwrap it
                else if ((PIXEL1.next == NULL) && (PIXEL1.head == PIXEL1))
                {
                    PIXEL2.head.last.next = PIXEL1;
                    PIXEL2.head.last = PIXEL1;
                    (PIXEL2.head.number_of_pixels_in_group)++;
                    PIXEL1.head = PIXEL2.head;
                    PIXEL1.increment = PIXEL2.increment + pointer_edge.increment;
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
                                PIXEL1.increment - pointer_edge.increment - PIXEL2.increment;
                        // merge the other pixels in PIXELM 2 group to PIXELM 1 group
                        while (group2 != NULL)
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
                                PIXEL2.increment + pointer_edge.increment - PIXEL1.increment;
                        // merge the other pixels in PIXELM 2 group to PIXELM 1 group
                        while (group1 != NULL)
                        {
                            group1.head = group2;
                            group1.increment += incremento;
                            group1 = group1.next;
                        }  // while

                    }  // else
                }  // else
            }  // if
            pointer_edge++;
        }
        }

        // unwrap the image
        static void unwrapImage(PIXELM pixel, int image_width, int image_height)
        {
            int i;
            int image_size = image_width  image_height;
            PIXELM pixel_pointer = pixel;

            for (i = 0; i < image_size; i++)
            {
                pixel_pointer.value += TWOPI  (double)(pixel_pointer.increment);
                pixel_pointer++;
            }
        }

        // set the masked pixels (mask = 0) to the minimum of the unwrapper phase
        static void maskImage(PIXELM pixel, unsigned char input_mask, int image_width,
                       int image_height)
        {
            int image_width_plus_one = image_width + 1;
            int image_height_plus_one = image_height + 1;
            int image_width_minus_one = image_width - 1;
            int image_height_minus_one = image_height - 1;

            PIXELM pointer_pixel = pixel;
            unsigned char IMP = input_mask;  // input mask pointer
            double min = DBL_MAX;
            int i;
            int image_size = image_width  image_height;

            // find the minimum of the unwrapped phase
            for (i = 0; i < image_size; i++)
            {
                if ((pointer_pixel.value < min) && (IMP == NOMASK))
                    min = pointer_pixel.value;

                pointer_pixel++;
                IMP++;
            }

            pointer_pixel = pixel;
            IMP = input_mask;

            // set the masked pixels to minimum
            for (i = 0; i < image_size; i++)
            {
                if ((IMP) == MASK)
                {
                    pointer_pixel.value = min;
                }
                pointer_pixel++;
                IMP++;
            }
        }

        // the input to this unwrapper is an array that contains the wrapped
        // phase map.  copy the image on the buffer passed to this unwrapper to
        // over-write the unwrapped phase map on the buffer of the wrapped
        // phase map.
        static void returnImage(PIXELM pixel, double unwrapped_image, int image_width,
                         int image_height)
        {
            int i;
            int image_size = image_width  image_height;
            double unwrapped_image_pointer = unwrapped_image;
            PIXELM pixel_pointer = pixel;

            for (i = 0; i < image_size; i++)
            {
                unwrapped_image_pointer = pixel_pointer.value;
                pixel_pointer++;
                unwrapped_image_pointer++;
            }
        }

        // the main function of the unwrapper
        static  void unwrap2D(double wrapped_image, double UnwrappedImage,
                      unsigned char input_mask, int image_width, int image_height,
                      int wrap_around_x, int wrap_around_y,
                      char use_seed, unsigned int seed) {
            params_t params = { TWOPI, wrap_around_x, wrap_around_y, 0};
            unsigned char extended_mask;
            PIXELM pixel;
            EDGE edge;
            int image_size = image_height  image_width;
            int No_of_Edges_initially = 2  image_width  image_height;

            extended_mask = (unsigned char)calloc(image_size, sizeof(unsigned char));
            pixel = (PIXELM)calloc(image_size, sizeof(PIXELM));
            edge = (EDGE)calloc(No_of_Edges_initially, sizeof(EDGE));

            extend_mask(input_mask, extended_mask, image_width, image_height, &params);
            initialisePIXELs(wrapped_image, input_mask, extended_mask, pixel, image_width,
                             image_height, use_seed, seed);
            calculate_reliability(wrapped_image, pixel, image_width, image_height,
                                  &params);
            horizontalEDGEs(pixel, edge, image_width, image_height, &params);
            verticalEDGEs(pixel, edge, image_width, image_height, &params);

            if (params.no_of_edges != 0) {
                // sort the EDGEs depending on their reiability. The PIXELs with higher
                // relibility (small value) first
                quicker_sort(edge, edge + params.no_of_edges - 1);
            }
            // gather PIXELs into groups
            gatherPIXELs(edge, &params);

            unwrapImage(pixel, image_width, image_height);
            maskImage(pixel, input_mask, image_width, image_height);

            // copy the image from PIXELM structure to the unwrapped phase array
            // passed to this function
            // TODO: replace by (cython?) function to directly write into numpy array ?
            returnImage(pixel, UnwrappedImage, image_width, image_height);


        }




    }
}
*/