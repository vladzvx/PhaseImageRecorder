using Emgu.CV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RecorderCore
{
    public class StepPhaseImage : PhaseImage
    {

        public int StepNumber { get; set; } = 0;
        private List<double[,]> images;
        private List<DateTime> times;
        public StepPhaseImage(Mat image) : base(image)
        {
            images = new List<double[,]>() { };
            images.Add((double[,])Image.Clone());
            times = new List<DateTime>() { };
            StepNumber++;
        }

        public StepPhaseImage(double[,] image) : base(image)
        {
            images = new List<double[,]>() { };
            images.Add((double[,])Image.Clone());
            times = new List<DateTime>() { };
            StepNumber++;
        }

        public void AddStep(Mat image)
        {
            images.Add(_GetArrayFromMat(image, true));
            times.Add(DateTime.UtcNow);
            StepNumber++;
        }

        public void AddStep(double[,] image)
        {
            images.Add(image);
            times.Add(DateTime.UtcNow);
            StepNumber++;
        }

        public void FullSave(object _path)
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
                File.WriteAllLines(path+".csv", lines);

                int count = 0;
                foreach (var image in images)
                {
                    lines = new List<string>();
                    for (int i = 0; i < image.GetUpperBound(0) + 1; i++)
                    {
                        string line = "";
                        for (int j = 0; j < image.GetUpperBound(1) + 1; j++)
                        {
                            line += Math.Round(image[i, j], 2).ToString() + ";";
                        }
                        lines.Add(line);
                    }
                    File.WriteAllLines(path + count.ToString()+ ".csv", lines);
                    count++;
                }


            }
            catch (Exception ex)
            {

            }

        }

        public override void CalculatePhaseImage()
        {
            DateTime dt1 = DateTime.UtcNow;

            //np.arctan((image1-2*image2+image3)/(image1-image3))
            if (StepNumber == 3)
            {
                for (int i = 0; i <= Image.GetUpperBound(0); i++)
                {
                    for (int j = 0; j <= Image.GetUpperBound(1); j++)
                    {
                        double val1 = Math.Atan((images[0][i, j] - 2 * images[1][i, j] + images[2][i, j]) / (images[0][i, j] - images[2][i, j]));
                        Image[i, j] = val1;
                        //ImageForUI[i, j, 0] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        //ImageForUI[i, j, 1] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                       // ImageForUI[i, j, 2] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                    }
                }
                //images = new List<double[,]>();
            }
            else if (StepNumber == 4)
            {
                //np.arctan((image4 - image2) / (image1 - image3))
                for (int i = 0; i <= Image.GetUpperBound(0); i++)
                {
                    for (int j = 0; j <= Image.GetUpperBound(1); j++)
                    {
                        double val1 = Math.Atan((images[3][i, j] - images[1][i, j]) / (images[0][i, j] - images[2][i, j]));
                        Image[i, j] = val1;
                        //ImageForUI[i, j, 0] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        //ImageForUI[i, j, 1] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                        //ImageForUI[i, j, 2] = (byte)(255 * ((val1 + 2 * Math.PI) / Math.PI / 2));
                    }
                }
               //images = new List<double[,]>();

            }
            Calculating = DateTime.UtcNow.Subtract(dt1).TotalSeconds;
        }
    }

}
