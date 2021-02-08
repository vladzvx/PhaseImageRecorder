using RecorderCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFT_demo
{
    public partial class Form1 : Form
    {
        private const int size0 = 512;
        private const int size1 = 1024;
        private ImageSource imageSource = new ImageSource(size0, size1);
        private double[,] image=new double[size0, size1];
        private double[,] filter_image=new double[size0, size1];
        private double[,] image_res=new double[size0, size1];
        Unwrapping3 unwrapper = new Unwrapping3(new double[size0, size1]);
        Complex[,] fft_image = new Complex[size0, size1];
        private void plot(double[,] image)
        {
            byte[,,] im = Convert(image);
            pictureBox1.Image = new Bitmap(im.GetUpperBound(1) + 1, im.GetUpperBound(0) + 1, 3 * (im.GetUpperBound(1) + 1),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(im, 0));
            pictureBox1.Update();
        }
        private byte[,,] Convert(double[,] image)
        {
            ImageSource.subtract_min(image);
            double max = ImageSource.max(image);
            int size0 = image.GetUpperBound(0)+1;
            int size1 = image.GetUpperBound(1)+1;
            byte[,,] im = new byte[size0, size1, 3];
            for (int i = 0; i < size0; i++)
            {
                for (int j = 0; j < size1; j++)
                {
                    byte value = (byte)Math.Round(image[i, j] / (max) * 255, 0);
                    im[i, j, 0] = value;
                    im[i, j, 1] = value;
                    im[i, j, 2] = value;
                }
            }
            return im;
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tmp = TestImageGenerator.GetTestPair(size0, size1, 5, 15);
            image = tmp.Item1.images[0];
            image_res = tmp.Item2;
            plot(image_res);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Complex.ApplyFilter(fft_image, filter_image);
            plot(Tools.GetABS(fft_image));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            plot(image);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fft_image = Complex.CreateComplexArray(image);
            FourierTransform.FFT2(fft_image, FourierTransform.Direction.Forward);
            plot(Tools.GetABS(fft_image));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CreateHilbertFilter();
            plot(filter_image);
        }

        private void CreateHilbertFilter()
        {
            switch (this.comboBox1.Text)
            {
                case "1":
                    {
                        filter_image = Tools.CreateHilbertFilter1(image.GetUpperBound(0) + 1, image.GetUpperBound(1) + 1);
                        break;
                    }
                case "2":
                    {
                        filter_image = Tools.CreateHilbertFilter2(image.GetUpperBound(0) + 1, image.GetUpperBound(1) + 1);
                        break;
                    }
                case "3":
                    {
                        filter_image = Tools.CreateHilbertFilter3(image.GetUpperBound(0) + 1, image.GetUpperBound(1) + 1);
                        break;
                    }
                case "4":
                    {
                        filter_image = Tools.CreateHilbertFilter4(image.GetUpperBound(0) + 1, image.GetUpperBound(1) + 1);
                        break;
                    }
                case "5":
                    {
                        filter_image = Tools.CreateHilbertFilter5(image.GetUpperBound(0) + 1, image.GetUpperBound(1) + 1);
                        break;
                    }
                case "6":
                    {
                        filter_image = Tools.CreateHilbertFilter6(image.GetUpperBound(0) + 1, image.GetUpperBound(1) + 1);
                        break;
                    }
            }
            
        }
        private void button6_Click(object sender, EventArgs e)
        {
            FourierTransform.FFT2(fft_image, FourierTransform.Direction.Backward);
            image =  Tools.CalculatePhaseImageByHilbert(fft_image);
            plot(image);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            unwrapper.UpdateParamsIfNeed(image);
            unwrapper.UnwrapParallel(image, out var scr);
            plot(image);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ImageSource.subtract_min(image);
            ImageSource.subtract_min(image_res);
            var d = ImageSource.diff(image,image_res);
            label1.Text = Math.Round(ImageSource.max(d), 5).ToString(); ;
            label2.Text = Math.Round(ImageSource.min(d), 5).ToString(); ;
            label3.Text = Math.Round(ImageSource.std(image,image_res), 5).ToString(); ;
            label1.Update();
            label2.Update();
            label3.Update();
            plot(d);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var tmp = TestImageGenerator.GetTestPair(size0, size1, 5, 15);
            image = tmp.Item1.images[0];
            image_res = tmp.Item2;
            fft_image = Complex.CreateComplexArray(image);
            FourierTransform.FFT2(fft_image, FourierTransform.Direction.Forward);
            CreateHilbertFilter();
            Complex.ApplyFilter(fft_image, filter_image);
            FourierTransform.FFT2(fft_image, FourierTransform.Direction.Backward);
            image = Tools.CalculatePhaseImageByHilbert(fft_image);
            unwrapper.UpdateParamsIfNeed(image);
            unwrapper.UnwrapParallel(image, out var scr);
            ImageSource.subtract_min(image);
            ImageSource.subtract_min(image_res);
            var d = ImageSource.diff(image, image_res);
            label1.Text = Math.Round(ImageSource.max(d), 5).ToString(); ;
            label2.Text = Math.Round(ImageSource.min(d), 5).ToString(); ;
            label3.Text = Math.Round(ImageSource.std(image, image_res), 5).ToString(); ;
            label1.Update();
            label2.Update();
            label3.Update();
            plot(d);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
