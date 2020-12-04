using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecorderCore
{
    public class ModelImageCapture:BaseCapture
    {
        private ImageSource imageSource = new ImageSource(1000, 2000);
        
        public ModelImageCapture(): base()
        {
            imageSource.CreateImagesForStepMethod(5, 4);
        }
        internal override double[,] GetImage()
        {
            return imageSource.GetNextImage();
        }
    }
}
