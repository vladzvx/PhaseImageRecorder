#define DLL_EXPORT __declspec(dllexport) // обязательно определять функции, которы могут быть экспортированы
#ifndef UWR_LIBRARY_H
#define UWR_LIBRARY_H

void DLL_EXPORT unwrap2D(double *wrapped_image, double *UnwrappedImage,
              unsigned char *input_mask, int image_width, int image_height,
              int wrap_around_x, int wrap_around_y,
              char use_seed, unsigned int seed);

#endif //UWR_LIBRARY_H
