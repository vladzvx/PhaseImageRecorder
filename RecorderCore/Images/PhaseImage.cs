using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RecorderCore
{
    public abstract class PhaseImage
    {
        List<string> lines = new List<string>();
        public void Save(object _path)
        {
            string path = _path.ToString();
            try
            {
                for (int i = 0; i < Image.GetUpperBound(0) + 1; i++)
                {
                    string line = "";
                    for (int j = 0; j < Image.GetUpperBound(1) + 1; j++)
                    {
                        line += Math.Round(Image[i, j], 2).ToString() + ";";
                    }
                    lines.Add(line);
                }
                File.WriteAllLines(path, lines);
            }
            catch (Exception ex)
            {

            }

        }
        internal static class NativeMethods
        {
            [DllImport(@"uwr.dll", EntryPoint = "unwrap2D")]
            internal static extern void unwrap(
                IntPtr wrappedImagePointer, IntPtr unwrappedImagePointer, IntPtr maskPointer, int image_width, int image_height,
                  int wrap_around_x, int wrap_around_y, char user_seed, uint seed);

        }

        #region fields
        public static double[,] _GetArrayFromMat(Mat mat, bool Dispose = true)
        {
            Image<Rgb, double> image = mat.ToImage<Rgb, double>();
            int Dim0 = image.Data.GetUpperBound(0) + 1;
            int Dim1 = image.Data.GetUpperBound(1) + 1;
            double[,] ForReturn = new double[Dim0, Dim1];
            for (int i = 0; i < Dim0; i++)
            {
                for (int j = 0; j < Dim1; j++)
                {
                    ForReturn[i, j] = image.Data[i, j, 0];
                }
            }
            if (Dispose)
            {
                mat.Dispose();
                image.Dispose();
            }
            return ForReturn;
        }
        public void GetArrayFromMat(Mat mat, bool Dispose = true)
        {
            Image<Rgb, double> image = mat.ToImage<Rgb, double>();
            int Dim0 = image.Data.GetUpperBound(0) + 1;
            int Dim1 = image.Data.GetUpperBound(1) + 1;
            //Image = (double[,])image.Data.Clone();// new double[Dim0, Dim1];
            Image = new double[Dim0, Dim1];
            ImageForUI = new byte[Dim0, Dim1, image.Data.GetUpperBound(2) + 1];
            
            for (int i = 0; i < Dim0; i++)
            {
                for (int j = 0; j < Dim1; j++)
                {
                    Image[i, j] = image.Data[i, j, 0];
                    //ImageForUI[i, j, 0] = (byte)image.Data[i, j, 0];
                    //ImageForUI[i, j, 1] = (byte)image.Data[i, j, 1];
                    //ImageForUI[i, j, 2] = (byte)image.Data[i, j, 2];
                }
            }
            if (Dispose)
            {
                mat.Dispose();
                image.Dispose();
            }

        }
        public void SetUIMatrix()
        {
            int Dim0 = Image.GetUpperBound(0) + 1;
            int Dim1 = Image.GetUpperBound(1) + 1;
            ImageForUI = new byte[Dim0, Dim1, 3];
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
        }
        public void GetArrayFromMat(double[,] image)
        {
            int Dim0 = image.GetUpperBound(0) + 1;
            int Dim1 = image.GetUpperBound(1) + 1;
            Image = image;
            ImageForUI = new byte[Dim0, Dim1, 3];
            for (int i = 0; i < Dim0; i++)
            {
                for (int j = 0; j < Dim1; j++)
                {
                    ImageForUI[i, j, 0] = (byte)image[i, j];
                    ImageForUI[i, j, 1] = (byte)image[i, j];
                    ImageForUI[i, j, 2] = (byte)image[i, j];
                }
            }
        }
        public SettingsContainer.ProcessingStep status { get; private set; }
        public SettingsContainer.ProcessingStep MaxProcessingStep { get; set; }
        public DateTime RecordingTime { get; private set; }
        public double[,] Image { get; internal set; }
        public byte[,,] ImageForUI { get; internal set; }
       // public Bitmap bitmap { get; internal set; }
        #endregion
        public PhaseImage(Mat image)
        {
            RecordingTime = DateTime.UtcNow;
            status = SettingsContainer.ProcessingStep.Interferogramm;
            GetArrayFromMat(image, true);
        }
        public PhaseImage(double[,] image)
        {
            Image = image;
            RecordingTime = DateTime.UtcNow;
            status = SettingsContainer.ProcessingStep.Interferogramm;
            //GetArrayFromMat(image);
        }
        public virtual void CalculatePhaseImage()
        {
            if (status <= SettingsContainer.ProcessingStep.Interferogramm)
            {
                status = SettingsContainer.ProcessingStep.WrappedPhaseImage;
            }

        }
        public virtual void Unwrap()
        {
            try
            {
                if (MaxProcessingStep < SettingsContainer.ProcessingStep.UnwrappedPhaseImage) return;
                double[,] matrix = new double[Image.GetUpperBound(0) + 1, Image.GetUpperBound(1) + 1];
                byte[,] mask = new byte[Image.GetUpperBound(0) + 1, Image.GetUpperBound(1) + 1];
                NativeMethods.unwrap(Marshal.UnsafeAddrOfPinnedArrayElement(Image, 0),
                    Marshal.UnsafeAddrOfPinnedArrayElement(matrix, 0),
                    Marshal.UnsafeAddrOfPinnedArrayElement(mask, 0), Image.GetUpperBound(1) + 1, Image.GetUpperBound(0) + 1, 0, 0, (char)0, (uint)1);
                
                Image = matrix;
                if (status <= SettingsContainer.ProcessingStep.WrappedPhaseImage)
                {
                    status = SettingsContainer.ProcessingStep.UnwrappedPhaseImage;
                }
            }
            catch (Exception ex)
            {

            }

        }
        public virtual void Process()
        {
            SetUIMatrix();
            if (status <= SettingsContainer.ProcessingStep.UnwrappedPhaseImage)
            {
                status = SettingsContainer.ProcessingStep.ProcessedImage;
            }
        }
    }

}
