using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecorderCore
{
    public class CameraImage : PhaseImage
    {
        public CameraImage(Mat image) : base(image)
        {

        }

        public CameraImage(double[,] image) : base(image)
        {

        }
    }
}
