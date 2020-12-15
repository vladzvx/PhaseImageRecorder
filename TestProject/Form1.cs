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
using RecorderCore;

namespace TestProject
{
    public partial class Form1 : Form
    {
        public byte[,,] GetUIMatrix(double[,] Image)
        {
            int Dim0 = Image.GetUpperBound(0) + 1;
            int Dim1 = Image.GetUpperBound(1) + 1;
            byte[,,] ImageForUI = new byte[Dim0, Dim1, 3];
            double max = 0;
            double min = 0;
            for (int i = 0; i <= Image.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= Image.GetUpperBound(1); j++)
                {
                    double val1 = Image[i, j];
                    if (val1 < min) min = val1;
                    if (val1 > max) max = val1;

                }
            }
            for (int i = 0; i <= Image.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= Image.GetUpperBound(1); j++)
                {
                    byte val1 = (byte)(255 * (Image[i, j] - min) / (max - min));
                    ImageForUI[i, j, 0] = val1;
                    ImageForUI[i, j, 1] = val1;
                    ImageForUI[i, j, 2] = val1;
                }
            }
            return ImageForUI;
        }

        private void plot(byte[,,] im)
        {
            pictureBox1.Image = new Bitmap(im.GetUpperBound(1) + 1, im.GetUpperBound(0) + 1, 3 * (im.GetUpperBound(1) + 1),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(im, 0));
            pictureBox1.Update();
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var t = TestImageGenerator.GetTestPair(400, 400,4*Math.PI);
            this.label1.Text = Math.Round(ImageSource.max(t.Item1.Image), 2).ToString();
            label1.Update();
            this.label2.Text = Math.Round(ImageSource.min(t.Item1.Image), 2).ToString();
            label2.Update();
            plot(GetUIMatrix(t.Item2));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var t = TestImageGenerator.GetTestPair(400, 400, 4 * Math.PI);
            t.Item1.CalculatePhaseImage();
            t.Item1.Unwrap();
            this.label1.Text = Math.Round(ImageSource.max(t.Item1.Image), 2).ToString();
            label1.Update();
            this.label2.Text = Math.Round(ImageSource.min(t.Item1.Image), 2).ToString();
            label2.Update();
            plot(GetUIMatrix(t.Item1.Image));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var t = TestImageGenerator.GetTestPair(400, 400, 4 * Math.PI);
            t.Item1.CalculatePhaseImage();
            Unwrapping.Unwrap(t.Item1.Image);

            this.label1.Text =Math.Round( ImageSource.max(t.Item1.Image),2).ToString();
            label1.Update();
            this.label2.Text =Math.Round( ImageSource.min(t.Item1.Image),2).ToString();
            label2.Update();
            plot(GetUIMatrix(t.Item1.Image));
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
