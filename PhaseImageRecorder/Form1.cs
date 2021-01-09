using Emgu.CV;
using Emgu.CV.Structure;
using RecorderCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PhaseImageRecorder
{
    public partial class Form1 : Form
    {
        DateTime StartDt;
        double FrameCounter = 0;
        private System.Timers.Timer timer;
        private RecordingDriver recordingDriver = new RecordingDriver(1);
        PhaseImage phaseImage;
        private SettingsContainer SettingsContainer = new SettingsContainer();
        object locker = new object();
        bool imagePlotted = false;
        int SaveCount = 0;
        private void SetStepSettings()
        {
            if (this.checkedListBox2.GetItemChecked(0))
            {
                this.SettingsContainer.maxProcessingStep = SettingsContainer.ProcessingStep.UnwrappedPhaseImage;
            }
            else if (this.checkedListBox2.GetItemChecked(1))
            {
                this.SettingsContainer.maxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
            }
            else
            {
                this.SettingsContainer.maxProcessingStep = SettingsContainer.ProcessingStep.WrappedPhaseImage;
            }
            SettingsContainer.recordingType = SettingsContainer.RecordingType.Step;
            SettingsContainer.MaximumSteps = int.Parse(this.comboBox1.SelectedItem.ToString());
            SettingsContainer.FramePause = int.Parse(this.comboBox2.SelectedItem.ToString());
        }

        private void SetCameraSettings()
        {
            SettingsContainer.recordingType = SettingsContainer.RecordingType.Camera;
        }
        private void SetHilbertSettings()
        {
            if (this.checkedListBox1.GetItemChecked(0))
            {
                this.SettingsContainer.maxProcessingStep = SettingsContainer.ProcessingStep.UnwrappedPhaseImage;
            }
            else if (this.checkedListBox1.GetItemChecked(1))
            {
                this.SettingsContainer.maxProcessingStep = SettingsContainer.ProcessingStep.ProcessedImage;
            }
            else
            {
                this.SettingsContainer.maxProcessingStep = SettingsContainer.ProcessingStep.WrappedPhaseImage;
            }
            SettingsContainer.recordingType = SettingsContainer.RecordingType.Hilbert;
        }

        private void UpdateSettings()
        {
            switch (this.tab.SelectedTab.Text.ToLower())
            {
                case "hilbert":
                    {
                        SetHilbertSettings();
                        break;
                    }
                case "camera":
                    {
                        SetCameraSettings();
                        break;
                    }
                case "step":
                    {

                        SetStepSettings();
                        break;
                    }

            }

            recordingDriver.UpdateSettings(SettingsContainer);
        }
        private void TabSelectedHndler(object sender, TabControlEventArgs e)
        {
            UpdateSettings();
        }
        public Form1()
        {
            this.timer = new System.Timers.Timer();
            this.timer.Elapsed += action;

            this.timer.Interval = 10;
            this.timer.SynchronizingObject = this;
            InitializeComponent();
            tab.Selected += TabSelectedHndler;
            this.Text = "Phase image recorder";
            UpdateSettings();
            this.recordingDriver.AddImageReciever(this.UpdateImage);
            this.timer.Enabled = true;
        }

        private void plot(byte[,,] im)
        {
            pictureBox1.Image = new Bitmap(im.GetUpperBound(1) + 1, im.GetUpperBound(0) + 1, 3 * (im.GetUpperBound(1) + 1),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(im, 0));
            pictureBox1.Update();
        }
        private void action(object sender, ElapsedEventArgs e)
        {
            // bool is_is_locked = false;
            //Monitor.Enter(locker, ref is_is_locked);
            PhaseImage ph;
            lock (locker)
            {
                ph = phaseImage;
               // phaseImage = null;
            }
                try
                {
                    if (imagePlotted || ph == null)
                    {
                        return;
                    }

                    try
                    {
                    plot(phaseImage.ImageForUI);
                    imagePlotted = true;
                            //imagePlotted = true;
                    FrameCounter++;
                    TimeSpan timeSpan = DateTime.UtcNow.Subtract(StartDt);

                    label4.Text = Math.Round(FrameCounter / timeSpan.TotalSeconds, 2).ToString();
                    label4.Update();
                    if (timeSpan.TotalMinutes > 0.5)
                    {
                        StartDt = DateTime.UtcNow;
                        FrameCounter = 0;
                    }

                    label9.Text = phaseImage.Creation.ToString();
                    label10.Text = phaseImage.Calculating.ToString();
                    label11.Text = phaseImage.Unwrapping.ToString();
                    label12.Text = phaseImage.Processing.ToString();

                    label9.Update();
                    label10.Update();
                    label11.Update();
                    label12.Update();

                    if (phaseImage.report != null)
                    {
                        label19.Text = phaseImage.report.GammasCalc.ToString();
                        label20.Text = phaseImage.report.EdgesCalc.ToString();
                        label21.Text = phaseImage.report.Sorting.ToString();
                        label22.Text = phaseImage.report.PathFinding.ToString();
                        label23.Text = phaseImage.report.Unwrap.ToString();
                        label24.Text = phaseImage.report.Refresh.ToString();
                        label26.Text = (phaseImage.Creation + phaseImage.Calculating + phaseImage.Unwrapping + phaseImage.Processing).ToString();

                        label19.Update();
                        label20.Update();
                        label21.Update();
                        label22.Update();
                        label23.Update();
                        label24.Update();
                        label26.Update();
                    }
                    }
                    catch { }

                    /*
                    Bitmap bitmap = new Bitmap(phaseImage.ImageForUI.GetUpperBound(1) + 1, phaseImage.ImageForUI.GetUpperBound(0) + 1);
                    for (int i=0;i< phaseImage.ImageForUI.GetUpperBound(1) + 1; i++)
                    {
                        for (int j = 0; j < phaseImage.ImageForUI.GetUpperBound(0) + 1;j++)
                        {
                            int val = (int)phaseImage.ImageForUI[j, i];
                            bitmap.SetPixel(j, i, Color.FromArgb(val, val, val));
                        }
                    }
                    pictureBox1.Image = bitmap;

                    pictureBox1.Update();
                    */
                    }
                    catch
                {

                }

           // }
            //  Monitor.Exit(locker);


        }


        public void UpdateImage(PhaseImage phaseImage)
        {
            lock (locker)
            {
                this.phaseImage = phaseImage;
                imagePlotted = false;
                /*
                if (phaseImage as StepPhaseImage != null)
                {
                    Image<Rgb, byte> data = phaseImage.Matrix.ToImage<Rgb, byte>();
                    bitmap = new Bitmap(data.Width, data.Height);
                    for (int w = 0; w < data.Width; w++)
                    {
                        for (int h = 0; h < data.Height; h++)
                        {
                            int r = (int)data.Data[h, w, 0];
                            int g = (int)data.Data[h, w, 1];
                            int b = (int)data.Data[h, w, 2];
                            bitmap.SetPixel(w, h, Color.FromArgb(r, g, b));
                        }
                    }
                }
                else*/
                //phaseImage.Matrix.Dispose();
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                this.richTextBox1.Text = FBD.SelectedPath;

            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettings();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettings();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettings();
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSettings();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.SettingsContainer.model = this.checkBox1.Checked;
            UpdateSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lock (locker)
            {
                string path = Path.Combine(this.richTextBox1.Text, SaveCount.ToString() + "_" + DateTime.UtcNow.ToString().Replace('.', '_').Replace(':', '_').Replace(' ', '_') + ".csv");
                Task.Factory.StartNew(phaseImage.Save, path);
                //phaseImage.Save(path);
                SaveCount++;
            }
        }

        private void tab_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
          //  var t =TestImageGenerator.GetTestPair(1000, 1000,);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }
    }
}
