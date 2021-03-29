using RecorderCore;
using RecorderCore.Images;
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
        HilbertPhaseImage2 hpi;
        private const int size0 = 512;
        private const int size1 = 1024;
        private ImageSource imageSource = new ImageSource(size0, size1);
        private byte[,,] image0=new byte[size0, size1,3];
        private double[,] image=new double[size0, size1];
        private double[,] filter_image=new double[size0, size1];
        private double[,] image_res=new double[size0, size1];
        Unwrapping3 unwrapper = new Unwrapping3(new double[size0, size1]);
        Complex[,] fft_image = new Complex[size0, size1];
        private void plot(double[,] image)
        {
            byte[,,] im = RecorderCore.ColorConverter.ConvertTluck(image);
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
                    im[i, j, 0] = value;//blue
                    im[i, j, 1] = value;//green;
                    im[i, j, 2] = value;//red
                }
            }
            return im;
        }



        private byte CustomConvertor(double value)
        {
            if (value < 0) return 0;
            else if (value > 255) return 255;
            else return (byte)value;
        }
        private byte Red(double value)
        {
            return CustomConvertor(-370 + 2.5 * value);
        }
        private byte Blue(double value)
        {
            if (value<50)
                return CustomConvertor(105 + 3*value);
            else
                return CustomConvertor(325 - 1.5 * value);
        }
        private byte Green(double value)
        {
            return CustomConvertor(-150 + 3 * value);
        }
        private byte[,,] ConvertJet(double[,] image)
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



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tmp = TestImageGenerator.GetTestPair2(size0, size1, 5, 15);
            image0 = tmp.Item1;
            image_res = tmp.Item2;
            plot(image_res);
            hpi = new HilbertPhaseImage2(image0,0,1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Complex.ApplyFilter(fft_image, filter_image);
            plot(Tools.GetABS(fft_image));
        }

        private void button2_Click(object sender, EventArgs e)
        {

            hpi.Convert();
            plot(hpi.images[0]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fft_image = Complex.CreateComplexArray(hpi.images[0]);
            FourierTransform.FFT2(fft_image, FourierTransform.Direction.Forward);
            plot(Tools.GetABS(fft_image));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            filter_image = hpi.CreateFilterIfNeed(size1,size0);
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
            hpi.Calc();   
            plot(hpi.images[0]);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            hpi.Unwrapp();
            plot(hpi.images[0]);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ImageSource.subtract_min(hpi.images[0]);
            ImageSource.subtract_min(image_res);
            var d = ImageSource.diff(hpi.images[0], image_res);
            label1.Text = Math.Round(ImageSource.max(d), 5).ToString(); ;
            label2.Text = Math.Round(ImageSource.min(d), 5).ToString(); ;
            label3.Text = Math.Round(ImageSource.std(hpi.images[0], image_res), 5).ToString(); ;
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

        private void button10_Click(object sender, EventArgs e)
        {
            plot(ImageSource.GetPlane(256, 256, 10));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            double[,] test1 = ImageSource.GetPlane(256, 256, 10);
            //double[,] test2 = ImageSource.GetPlane(1024, 1024,
            //    new ImageSource.point() { x = 100, y = 100, z = test1[100, 100] },
            //    new ImageSource.point() { x = 100, y = 900, z = test1[100, 900] },
            //    new ImageSource.point() { x = 900, y = 512, z = test1[900, 512] });
            double[,] test2 = ImageSource.GetTrendPlane(test1);
            plot(test2);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            double[,] test1 = ImageSource.GetPlane(256, 256, 10);
            double[,] test2 = ImageSource.GetTrendPlane(test1);
            //double[,] test2 = ImageSource.GetPlane(1024, 1024,
            //    new ImageSource.point() { x = 100, y = 100, z = test1[100, 100] },
            //    new ImageSource.point() { x = 100, y = 900, z = test1[100, 900] },
            //    new ImageSource.point() { x = 900, y = 512, z = test1[900, 512] });

            plot(ImageSource.diff(test1, test2));
        }
    }
}
