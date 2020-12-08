using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("RecorderCoreTests")]

namespace RecorderCore
{
    public class ModelImageCapture:BaseCapture
    {
        Random rnd = new Random();
        private ImageSource imageSource = new ImageSource(1000, 1500);
        private double MaxFPS = 25;
        public void SetMaxFPS(double FPS)
        {
            lock (ReadSettingsLocker)
            {
                this.MaxFPS = FPS;
                double sleep = 1000 / MaxFPS;
                AfterCaptureSleeping = sleep >= 0 ? sleep : 0;
            }
        }
        internal override void CycleEnded()
        {
            base.CycleEnded();
            if (AfterCaptureSleeping == 0) return;
            double cyc;
            lock (DiagnosticLocker)
                cyc = ActionAverageTimespan;
            lock (ReadSettingsLocker)
            {
                double sleep = 1000 / MaxFPS - cyc;
                AfterCaptureSleeping = sleep >= 0 ? sleep : 0;
            }

        }
        public ModelImageCapture(): base()
        {
            imageSource.CreateImagesForStepMethod(1, 4);


        }

        internal override void sleep(double sleep)
        {
            if (sleep < 1) return;
            DateTime dt = DateTime.UtcNow;
            while (DateTime.UtcNow.Subtract(dt).TotalMilliseconds < sleep) ;
        }



        public ModelImageCapture(int Height, int Width, bool DiagnosticEnable = true) : base(DiagnosticEnable)
        {
            imageSource = new ImageSource(Height, Width);
            imageSource.CreateImagesForStepMethod(1, 4);
        }
        internal override double[,] GetImage()
        {
            //return null;
            return imageSource.GetNextImage();
        }
    }
}
