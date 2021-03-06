﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using ToupTek;
using System.Runtime.InteropServices;
using RecorderCore;
using System.Linq;
using System.IO;
using RecorderCore.Images;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PhaseImageRecorderToupCam
{
    public partial class Form1 : Form
    {
        private double subtractiong_value = 0;
        private System.Timers.Timer recordingTomer = new System.Timers.Timer();
        private object locker = new object();
        private Regex pahtCheckRegex = new Regex(@"^\w:\w+.+$");
        private string FolderPath = "images";
        private Settings settings;
        private bool Worked = false;
        private bool Adjusted = false;
        private HilbertPhaseImage2 hpi2;
        private HilbertCalculator2 calculator = new HilbertCalculator2();
        private int selectIndexCombo2 = 3;
        private int selectIndexCombo3 = 3;
        private bool started = false;
        private delegate void DelegateEvent(ToupCam.eEVENT[] ev);
        private ToupCam toupcam_ = null;
        private Bitmap bmp_ = null;
        private DelegateEvent ev_ = null;
        private FPSCounter fPSCounter = new FPSCounter(30);
        private System.Timers.Timer dbsync = new System.Timers.Timer(10000);
        private byte[,,] image;
        private void UpdateSettings()
        {
            if (settings == null)
            {
                settings = new Settings();
            }

            settings.color =(int) numericUpDown2.Value;
            settings.path = FolderPath;
            settings.exposition = trackBar1.Value;
            settings.gain = trackBar2.Value;
            //settings.saturation = trackBar3.Value;
            settings.auto_exposition = checkBox1.Checked;
            settings.resolution = comboBox1.Text;
            settings.framing_enabled = checkBox4.Checked;
            settings.x_frame_position = trackBar4.Value;
            settings.y_frame_position = trackBar5.Value;
            var xframe_size = comboBox2.SelectedIndex >= 0 ? comboBox2.Items[comboBox2.SelectedIndex] : comboBox2.Items[0];
            var yframe_size = comboBox3.SelectedIndex >= 0 ? comboBox3.Items[comboBox3.SelectedIndex] : comboBox3.Items[0];
            settings.x_frame_size = (int)xframe_size;
            settings.y_frame_size = (int)yframe_size;
        }
        private void TimerAction(object sender, System.Timers.ElapsedEventArgs args)
        {
            UpdateSettings();
            settings.SaveSettings();
        }
        private void savefile(IntPtr pData, ref ToupCam.BITMAPINFOHEADER header)
        {
            Bitmap bmp = new Bitmap(header.biWidth, header.biHeight, PixelFormat.Format24bppRgb);
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, header.biWidth, header.biHeight), ImageLockMode.WriteOnly, bmp.PixelFormat);

            ToupCam.CopyMemory(bmpdata.Scan0, pData, header.biSizeImage);

            bmp.UnlockBits(bmpdata);

            bmp.Save("toupcamdemowinformcs2.jpg");
        }

        private void OnEventError()
        {
            if (toupcam_ != null)
            {
                toupcam_.Close();
                toupcam_ = null;
            }
            MessageBox.Show("Error");
        }

        private void OnEventDisconnected()
        {
            if (toupcam_ != null)
            {
                toupcam_.Close();
                toupcam_ = null;
            }
            MessageBox.Show("The camera is disconnected, maybe has been pulled out.");
        }

        private void OnEventExposure()
        {
            if (toupcam_ != null)
            {
                uint nTime = 0;
                if (toupcam_.get_ExpoTime(out nTime))
                {
                    trackBar1.Value = (int)nTime;
                    label1.Text = (nTime / 1000).ToString() + " ms";
                }
            }
        }

        private void OnEventImage()
        {
            try
            {
                if (bmp_ != null)
                {
                    if (this.tabControl1.SelectedIndex != 0) return;
                    uint nWidth = 0, nHeight = 0;
                    if (radioButton2.Checked)
                    {
                        byte[,,] arr = new byte[bmp_.Height, bmp_.Width, 3];
                        IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);

                        toupcam_.PullImage(pointer, 24, out nWidth, out nHeight);
                        int x = trackBar4.Value;
                        int y = trackBar5.Value;
                        int w = int.Parse(comboBox2.Text);
                        int h = int.Parse(comboBox3.Text); ;
                        //Marshal.Copy(pointer, arr2, 0, arr.Length);
                        for (int i = y; i < h + y; i++)
                        {
                            arr[i, x, 1] = 255;
                            arr[i, x + 1, 1] = 255;
                            arr[i, x + 2, 1] = 255;
                            arr[i, x + w, 1] = 255;
                            arr[i, x + w - 1, 1] = 255;
                            arr[i, x + w - 2, 1] = 255;
                        }
                        for (int j = x; j < w + x; j++)
                        {
                            arr[y, j, 1] = 255;
                            arr[y + 1, j, 1] = 255;
                            arr[y + 2, j, 1] = 255;
                            arr[y + h, j, 1] = 255;
                            arr[y + h - 1, j, 1] = 255;
                            arr[y + h - 2, j, 1] = 255;

                        }
                        pictureBox1.Image = new Bitmap((int)nWidth, (int)nHeight, (int)nWidth * 3, PixelFormat.Format24bppRgb,
                           Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0));
                        pictureBox1.Update();
                        label5.Text = Math.Round(fPSCounter.Count(), 2).ToString();
                        label5.Update();
                    }
                    else if (radioButton1.Checked)
                    {
                        if (checkBox4.Checked)
                        {
                            byte[,,] arr1 = new byte[bmp_.Height, bmp_.Width, 3];
                            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(arr1, 0);
                            toupcam_.PullImage(pointer, 24, out nWidth, out nHeight);

                            int x = trackBar4.Value;
                            int y = trackBar5.Value;
                            int w = int.Parse(comboBox2.Text);
                            int h = int.Parse(comboBox3.Text); ;

                            byte[,,] arr = new byte[h, w, 3];

                            for (int i = y; i < h + y; i++)
                            {
                                for (int j = x; j < w + x; j++)
                                {
                                    arr[i - y, j - x, 0] = arr1[i, j, 0];
                                    arr[i - y, j - x, 1] = arr1[i, j, 1];
                                    arr[i - y, j - x, 2] = arr1[i, j, 2];
                                }
                            }
                            //if (checkBox2.Checked)
                            //{
                            calculator.PutImage(arr,
                                level: (int)numericUpDown2.Value,
                                unwrap: checkBox3.Checked,
                                wavelength: settings.wavelength,
                                smooth: checkBox5.Checked,
                                summDepth: checkBox6.Checked ? (int)numericUpDown1.Value : 0,
                                calc:checkBox2.Checked, 
                                delTrend: checkBox7.Checked); 

                            var buff = calculator.GetImage();
                            if (buff != null) hpi2 = buff;
                            if (hpi2 != null)
                            {
                                arr = hpi2._images[0];
                                pictureBox1.Image = new Bitmap((int)w, (int)h, (int)w * 3, PixelFormat.Format24bppRgb,
                                    Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0));
                                pictureBox1.Update();
                                label5.Text = Math.Round(fPSCounter.Count(), 2).ToString();
                                label5.Update();
                            }
                            //}
                            //else
                            //{
                            //    pictureBox1.Image = new Bitmap((int)w, (int)h, (int)w * 3, PixelFormat.Format24bppRgb,
                            //        Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0));
                            //    pictureBox1.Update();
                            //    label5.Text = Math.Round(fPSCounter.Count(), 2).ToString();
                            //    label5.Update();
                            //}
                        }
                        else
                        {
                            byte[,,] arr1 = new byte[bmp_.Height, bmp_.Width, 3];
                            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(arr1, 0);
                            toupcam_.PullImage(pointer, 24, out nWidth, out nHeight);
                            pictureBox1.Image = new Bitmap(bmp_.Width, bmp_.Height, bmp_.Width * 3, PixelFormat.Format24bppRgb,
                                Marshal.UnsafeAddrOfPinnedArrayElement(arr1, 0));
                            pictureBox1.Update();
                            label5.Text = Math.Round(fPSCounter.Count(), 2).ToString();
                            label5.Update();
                        }
                    }


                    //button4_Click(null, null);
                }
            }
            catch { }
            
        }

        private void OnEventStillImage()
        {
            uint nWidth = 0, nHeight = 0;
            if (toupcam_.PullStillImage(IntPtr.Zero, 24, out nWidth, out nHeight))   /* peek the width and height */
            {
                Bitmap sbmp = new Bitmap((int)nWidth, (int)nHeight, PixelFormat.Format24bppRgb);

                BitmapData bmpdata = sbmp.LockBits(new Rectangle(0, 0, sbmp.Width, sbmp.Height), ImageLockMode.WriteOnly, sbmp.PixelFormat);
                toupcam_.PullStillImage(bmpdata.Scan0, 24, out nWidth, out nHeight);
                sbmp.UnlockBits(bmpdata);

                sbmp.Save("toupcamdemowinformcs2.jpg");
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void ApplySettings()
        {
           
            if (Settings.TryLoadSettings(out settings))
            {
                trackBar1.Value = settings.exposition;
                FolderPath = settings.path;
                checkBox1.Checked = settings.auto_exposition;
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(settings.resolution)>=0? comboBox1.Items.IndexOf(settings.resolution):0;
                checkBox4.Checked = settings.framing_enabled;
                trackBar4.Value = settings.x_frame_position;
                trackBar5.Value = settings.y_frame_position;
                comboBox2.SelectedIndex = comboBox2.Items.IndexOf(settings.x_frame_size) >= 0 ? comboBox2.Items.IndexOf(settings.x_frame_size) : 0;
                comboBox3.SelectedIndex = comboBox3.Items.IndexOf(settings.x_frame_size) >= 0 ? comboBox3.Items.IndexOf(settings.x_frame_size) : 0;
                trackBar2.Value = settings.gain;
                //trackBar3.Value = settings.saturation;
                numericUpDown2.Value = settings.color;
                trackBar4.Update();
                trackBar5.Update();
                trackBar1.Update();
                checkBox1.Update();
                comboBox1.Update();
                checkBox4.Update();
                trackBar4.Update();
                trackBar5.Update();
                comboBox2.Update();
                comboBox3.Update();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            recordingTomer.Elapsed += RecordingTimerAction;
            button2.Enabled = false;
            // button3.Enabled = false;
            trackBar1.Enabled = false;
            trackBar2.Enabled = false;
            //trackBar3.Enabled = false;
            checkBox1.Enabled = false;
            comboBox1.Enabled = false;
        }

        private void DelegateOnEvent(ToupCam.eEVENT[] ev)
        {
            switch (ev[0])
            {
                case ToupCam.eEVENT.EVENT_ERROR:
                    OnEventError();
                    break;
                case ToupCam.eEVENT.EVENT_DISCONNECTED:
                    OnEventDisconnected();
                    break;
                case ToupCam.eEVENT.EVENT_EXPOSURE:
                    OnEventExposure();
                    break;
                case ToupCam.eEVENT.EVENT_IMAGE:
                    OnEventImage();
                    break;
                case ToupCam.eEVENT.EVENT_STILLIMAGE:
                    OnEventStillImage();
                    break;
                case ToupCam.eEVENT.EVENT_TEMPTINT:
                    OnEventTempTint();
                    break;
            }
        }

        private void DelegateOnEventCallback(ToupCam.eEVENT ev)
        {
            /* this delegate is call by internal thread of toupcam.dll which is NOT the same of UI thread.
             * Why we use BeginInvoke, Please see:
             * http://msdn.microsoft.com/en-us/magazine/cc300429.aspx
             * http://msdn.microsoft.com/en-us/magazine/cc188732.aspx
             * http://stackoverflow.com/questions/1364116/avoiding-the-woes-of-invoke-begininvoke-in-cross-thread-winform-event-handling
            */
            BeginInvoke(ev_, new ToupCam.eEVENT[1] { ev });
        }

        private void OnStart(object sender, EventArgs e)
        {
            if (toupcam_ != null)
                return;

            ToupCam.Instance[] arr = ToupCam.Enum();
            if (arr.Length <= 0)
            {
                MessageBox.Show("no device");
            }
            else
            {
                toupcam_ = new ToupCam();
                if (!toupcam_.Open(arr[0].id))
                {
                    toupcam_ = null;
                }
                else
                {
                    label16.Enabled = true;
                    textBox1.Enabled = true;
                    checkBox1.Enabled = true;
                    checkBox12.Enabled = true;
                    checkBox2.Enabled = true;
                    checkBox4.Enabled = true;
                    checkBox5.Enabled = true;
                    checkBox6.Enabled = true;
                    checkBox7.Enabled = true;
                    trackBar1.Enabled = true;
                    trackBar2.Enabled = true;
                    //trackBar3.Enabled = true;
                    trackBar4.Enabled = true;
                    trackBar5.Enabled = true;
                    comboBox1.Enabled = true;
                    button2.Enabled = true;
                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                   // button3.Enabled = true;
                    button2.ContextMenuStrip = null;
                    InitSnapContextMenuAndExpoTimeRange();

                    trackBar2.SetRange(0, 255);
                    //trackBar3.SetRange(-100, 255);
                    OnEventTempTint();

                    uint resnum = toupcam_.ResolutionNumber;
                    uint eSize = 0;
                    if (toupcam_.get_eSize(out eSize))
                    {
                        for (uint i = 0; i < resnum; ++i)
                        {
                            int w = 0, h = 0;
                            if (toupcam_.get_Resolution(i, out w, out h))
                                comboBox1.Items.Add(w.ToString() + "*" + h.ToString());
                        }
                        comboBox1.SelectedIndex = (int)eSize;

                        int width = 0, height = 0;
                        if (toupcam_.get_Size(out width, out height))
                        {
                            bmp_ = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                            ev_ = new DelegateEvent(DelegateOnEvent);
                            if (!toupcam_.StartPullModeWithCallback(new ToupTek.ToupCam.DelegateEventCallback(DelegateOnEventCallback)))
                                MessageBox.Show("failed to start device");
                            else
                            {
                                bool autoexpo = true;
                                //toupcam_.get_AutoExpoEnable(out autoexpo);
                                toupcam_.put_AutoExpoEnable(false);
                                checkBox1.Checked = false;// autoexpo;
                                trackBar1.Enabled = !checkBox1.Checked;
                            }
                        }
                        if (toupcam_.get_ExpoAGain(out ushort val))
                        {
                            trackBar2.Value = val;
                            trackBar2.Update();
                        }
                        if (toupcam_.get_Saturation(out int val2))
                        {
                            //trackBar3.Value = val2;
                            //trackBar3.Update();
                        }

                        SetLimits(width, height);
                        started = true;
                    }
                }
            }

            try
            {
                ApplySettings();
                dbsync.Elapsed += TimerAction;
                dbsync.SynchronizingObject = this;
                dbsync.Start();
            }
            catch { }
        }

        private void SnapClickedHandler(object sender, ToolStripItemClickedEventArgs e)
        {
            int k = button2.ContextMenuStrip.Items.IndexOf(e.ClickedItem);
            if (k >= 0)
                toupcam_.Snap((uint)k);
        }

        private void InitSnapContextMenuAndExpoTimeRange()
        {
            if (toupcam_ == null)
                return;

            uint nMin = 0, nMax = 0, nDef = 0;
            if (toupcam_.get_ExpTimeRange(out nMin, out nMax, out nDef))
                trackBar1.SetRange((int)nMin, (int)nMax);
            OnEventExposure();

            if (toupcam_.StillResolutionNumber <= 0)
                return;
            
            button2.ContextMenuStrip = new ContextMenuStrip();
            button2.ContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(this.SnapClickedHandler);

            if (toupcam_.StillResolutionNumber < toupcam_.ResolutionNumber)
            {
                uint eSize = 0;
                if (toupcam_.get_eSize(out eSize))
                {
                    if (0 == eSize)
                    {
                        StringBuilder sb = new StringBuilder();
                        int w = 0, h = 0;
                        toupcam_.get_Resolution(eSize, out w, out h);
                        sb.Append(w);
                        sb.Append(" * ");
                        sb.Append(h);
                        button2.ContextMenuStrip.Items.Add(sb.ToString());
                        return;
                    }
                }
            }

            for (uint i = 0; i < toupcam_.ResolutionNumber; ++i)
            {
                StringBuilder sb = new StringBuilder();
                int w = 0, h = 0;
                toupcam_.get_Resolution(i, out w, out h);
                sb.Append(w);
                sb.Append(" * ");
                sb.Append(h);
                button2.ContextMenuStrip.Items.Add(sb.ToString());
            }
        }

        private void OnSnap(object sender, EventArgs e)
        {
            if (hpi2!=null)
            {
                HilbertPhaseImage2 temp = hpi2;
                Task.Factory.StartNew(() => {
                    string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); ;
                    string folderPath = Path.Combine(folder, FolderPath);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    DateTime dt = DateTime.UtcNow;
                    string path = Path.Combine(folderPath, dt.ToString().Replace('.', '_').Replace(':', '_').Replace(' ', '_'));
                    path += dt.Millisecond.ToString();
                    Bitmap bmp = new Bitmap((int)temp.source_images[0].GetUpperBound(1) + 1, (int)temp.source_images[0].GetUpperBound(0) + 1, (int)(temp.source_images[0].GetUpperBound(1) + 1) * 3, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(temp.source_images[0], 0));
                    bmp.Save(path + ".jpg");
                    temp.Save(path);
                    
                });
            }
            else
            {


                //if (toupcam_ != null)
                //{
                //    if (toupcam_.StillResolutionNumber <= 0)
                //    {
                //        if (bmp_ != null)
                //        {
                //            bmp_.Save(Path.Combine(richTextBox2.Text, "toupcamdemowinformcs2.jpg"));
                //        }
                //    }
                //    else
                //    {
                //        if (button2.ContextMenuStrip != null)
                //            button2.ContextMenuStrip.Show(Cursor.Position);
                //    }
                //}
            }

        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (toupcam_ != null)
            {
                toupcam_.Close();
                toupcam_ = null;
            }
        }

        private void OnSelectResolution(object sender, EventArgs e)
        {
            if (toupcam_ != null)
            {
                uint eSize = 0;
                if (toupcam_.get_eSize(out eSize))
                {
                    if (eSize != comboBox1.SelectedIndex)
                    {
                        button2.ContextMenuStrip = null;

                        toupcam_.Stop();
                        toupcam_.put_eSize((uint)comboBox1.SelectedIndex);

                        InitSnapContextMenuAndExpoTimeRange();
                        OnEventTempTint();

                        int width = 0, height = 0;
                        if (toupcam_.get_Size(out width, out height))
                        {
                            bmp_ = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                            ev_ = new DelegateEvent(DelegateOnEvent);
                            toupcam_.StartPullModeWithCallback(new ToupTek.ToupCam.DelegateEventCallback(DelegateOnEventCallback));
                        }
                        calculator.EreaseWorkingQueues();
                        SetLimits(width, height);
                    }
                }
            }
        }

        private void SetMinFramesize(int width, int height)
        {
            SetComboBoxes();
            int w = int.Parse(comboBox2.Text);
            int h = int.Parse(comboBox3.Text);
            if (w > width && comboBox2.SelectedIndex > 1)
            {
                comboBox2.SelectedIndex -= 1;
                selectIndexCombo2 = comboBox2.SelectedIndex;
                comboBox2.Update();
                SetMinFramesize(width, height);
            }
            if (h > height && comboBox3.SelectedIndex > 1)
            {
                comboBox3.SelectedIndex -= 1;
                selectIndexCombo3 = comboBox3.SelectedIndex;
                comboBox3.Update();
                SetMinFramesize(width, height);
            }
            
        }

        private void SetFrameStartPositionMaxValues(int width, int height)
        {
            int w = int.Parse(comboBox2.Text);
            int h = int.Parse(comboBox3.Text);
            int newWight = width - w - 1;
            int newHeight = height - h - 1;
            if (newWight >= 0)
            {
                if (trackBar4.Value > newWight) trackBar4.Value = newWight;
                trackBar4.Maximum = newWight;
                trackBar4.Update();
            }
            if (newHeight >= 0)
            {
                if (trackBar5.Value > newHeight) trackBar5.Value = newHeight;
                trackBar5.Maximum = newHeight;
                trackBar5.Update();
            }
        }
        private void SetLimits(int width, int height)
        {
            SetMinFramesize(width,height);
            SetFrameStartPositionMaxValues(width, height);
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (toupcam_ != null)
                toupcam_.put_AutoExpoEnable(checkBox1.Checked);
            trackBar1.Enabled = !checkBox1.Checked;
        }

        private void OnExpoValueChange(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                if (toupcam_ != null)
                {
                    uint n = (uint)trackBar1.Value;
                    toupcam_.put_ExpoTime(n);
                    label1.Text = (n / 1000).ToString() + " ms";
                }
            }
        }
        private void OnGainValueChange(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                if (toupcam_ != null)
                {
                    ushort n = (ushort)trackBar2.Value;
                    toupcam_.put_ExpoAGain(n);
                    label2.Text = "Gain: "+n.ToString();
                }
            }
        }

        private void OnSaturationValueChange(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                if (toupcam_ != null)
                {
                    //int n = (int)trackBar3.Value;
                    //toupcam_.put_Saturation(n);
                    //label3.Text = "Saturation: " + n.ToString();
                }
            }
        }
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Width = ClientRectangle.Right - button1.Bounds.Right - 20;
            pictureBox1.Height = ClientRectangle.Height - 8;
        }

        private void OnEventTempTint()
        {
            if (toupcam_ != null)
            {
                int nTemp = 0, nTint = 0;
                if (toupcam_.get_TempTint(out nTemp, out nTint))
                {
                  //  label2.Text = nTemp.ToString();
                 //   label3.Text = nTint.ToString();
                    //trackBar2.Value = nTemp;
                  //  trackBar3.Value = nTint;
                }
            }
        }

        private void OnWhiteBalanceOnePush(object sender, EventArgs e)
        {
            if (toupcam_ != null)
                toupcam_.AwbOnePush(null);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (Worked) Adjusted = true;

            if (radioButton1.Checked)
            {
                checkBox4.Visible = false;
                comboBox1.Visible = false;
                trackBar4.Visible = false;
                trackBar5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                label9.Visible = false;
                label8.Visible = false;
                comboBox2.Visible = false;
                comboBox3.Visible = false;
                SetComboBoxes();
            }
        }

        private void SetComboBoxes()
        {
            if (comboBox2.Text == null || comboBox2.Text.Equals(string.Empty))
            {
                comboBox2.SelectedIndex = selectIndexCombo2;
                comboBox2.Text = comboBox2.Items[selectIndexCombo2].ToString();
                comboBox2.Update();
            }
            if (comboBox3.Text == null || comboBox3.Text.Equals(string.Empty))
            {
                comboBox3.SelectedIndex = selectIndexCombo3;
                comboBox3.Text = comboBox3.Items[selectIndexCombo3].ToString();
                comboBox3.Update();
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Worked = true;
            if (radioButton2.Checked)
            {
                checkBox4.Visible = true;
                label7.Visible = true;
                trackBar4.Visible = true;
                trackBar5.Visible = true;
                label6.Visible = true;
                label9.Visible = true;
                label8.Visible = true;
                comboBox1.Visible = true;
                comboBox2.Visible = true;
                comboBox3.Visible = true;
                SetComboBoxes();
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                numericUpDown2.Enabled = true;
                numericUpDown2.Update();
                checkBox3.Enabled = true;
                checkBox3.Update();
            }
            else
            {
                numericUpDown2.Enabled = false;
                numericUpDown2.Update();
                checkBox3.Checked = false;
                checkBox3.Enabled = false;
                checkBox3.Update();
            }
            
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (started)
                SetLimits(bmp_.Width,bmp_.Height);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (started)
                SetLimits(bmp_.Width, bmp_.Height);
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                FolderPath = FBD.SelectedPath;
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }



        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings set = new Settings();
            set.SaveSettings();

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = checkBox6.Checked;
            numericUpDown1.Update();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            var filePath = string.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Directory.Exists("c:\\") ? "c:\\images" : "c:\\";
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    bmp_ = new Bitmap(filePath);
                    BitmapData bmd = bmp_.LockBits(new Rectangle(0, 0, bmp_.Width, bmp_.Height), ImageLockMode.ReadOnly, bmp_.PixelFormat);
                    byte[] bytedata = new byte[bmp_.Height * bmp_.Width * 3];
                    IntPtr ptr = bmd.Scan0;
                    Marshal.Copy(ptr, bytedata, 0, bmp_.Height * bmp_.Width * 3);
                    image = new byte[bmp_.Height , bmp_.Width , 3];
                    IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(image, 0);
                    Marshal.Copy(bytedata,0, pointer, bmp_.Height * bmp_.Width * 3);
                    hpi2 = new HilbertPhaseImage2(image, (int)numericUpDown3.Value, settings!=null? settings.wavelength:632.8, checkBox10.Checked)
                    {
                        calc = checkBox11.Checked,
                        del_trend = checkBox9.Checked,
                        smooth = checkBox8.Checked,
                        delete_value = subtractiong_value
                    };
                    pictureBox1.Image = new Bitmap(bmp_.Width, bmp_.Height, bmp_.Width * 3, PixelFormat.Format24bppRgb, pointer);
                    pictureBox1.Update();

                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked) execute();
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked) execute();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked) execute();
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked) execute();
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox13.Checked) execute();
        }

        private void button4_Click(object sender, EventArgs e)
        {
           if (!checkBox13.Checked) execute();
        }

        private void execute()
        {
            set_defaults();
            if (radioButton4.Checked)
            {
                int x = trackBar3.Value;
                int y = trackBar6.Value;
                int w = int.Parse(comboBox6.Text);
                int h = int.Parse(comboBox5.Text); ;

                byte[,,] arr = new byte[h, w, 3];

                for (int i = y; i < h + y; i++)
                {
                    for (int j = x; j < w + x; j++)
                    {
                        arr[i - y, j - x, 0] = image[i, j, 0];
                        arr[i - y, j - x, 1] = image[i, j, 1];
                        arr[i - y, j - x, 2] = image[i, j, 2];
                    }
                }
                hpi2 = new HilbertPhaseImage2(arr, (int)numericUpDown3.Value, settings != null ? settings.wavelength : 632.8, checkBox10.Checked)
                {
                    calc = checkBox11.Checked,
                    del_trend = checkBox9.Checked,
                    smooth = checkBox8.Checked,
                    delete_value = subtractiong_value
                };
            }
            else
            {
                hpi2 = new HilbertPhaseImage2(image, (int)numericUpDown3.Value, settings != null ? settings.wavelength : 632.8, checkBox10.Checked)
                {
                    calc = checkBox11.Checked,
                    del_trend = checkBox9.Checked,
                    smooth = checkBox8.Checked,
                    delete_value = subtractiong_value
                };
            }

            hpi2.Convert();
            hpi2.Calc();
            hpi2.Unwrapp();
            hpi2.ReverseConvert();
            framing();

        }

        private void framing()
        {
            set_defaults();
            byte[,,] image_copy = (byte[,,])hpi2._images[0].Clone();
            if (radioButton3.Checked)
            {
                int x = trackBar3.Value;
                int y = trackBar6.Value;
                int w = int.Parse(comboBox6.Text);
                int h = int.Parse(comboBox5.Text); ;
                //Marshal.Copy(pointer, arr2, 0, arr.Length);
                for (int i = y; i < h + y; i++)
                {
                    image_copy[i, x, 1] = 255;
                    image_copy[i, x + 1, 1] = 255;
                    image_copy[i, x + 2, 1] = 255;
                    image_copy[i, x + w, 1] = 255;
                    image_copy[i, x + w - 1, 1] = 255;
                    image_copy[i, x + w - 2, 1] = 255;
                }
                for (int j = x; j < w + x; j++)
                {
                    image_copy[y, j, 1] = 255;
                    image_copy[y + 1, j, 1] = 255;
                    image_copy[y + 2, j, 1] = 255;
                    image_copy[y + h, j, 1] = 255;
                    image_copy[y + h - 1, j, 1] = 255;
                    image_copy[y + h - 2, j, 1] = 255;
                }
            }

            if (radioButton4.Checked)
            {
                int x1 = image_copy.GetUpperBound(1) + 1;
                int y1 = image_copy.GetUpperBound(0) + 1;
                int l1 = x1 * 3;
                pictureBox1.Image = new Bitmap(x1, y1, x1 * 3, PixelFormat.Format24bppRgb,
                    Marshal.UnsafeAddrOfPinnedArrayElement(image_copy, 0));
            }
            else
            {
                pictureBox1.Image = new Bitmap(bmp_.Width, bmp_.Height, bmp_.Width * 3, PixelFormat.Format24bppRgb,
                    Marshal.UnsafeAddrOfPinnedArrayElement(image_copy, 0));
            }

            pictureBox1.Update();
        }
        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            button4.Enabled = !checkBox13.Checked;
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                trackBar3.Enabled = radioButton3.Checked;
                trackBar6.Enabled = radioButton3.Checked;
                comboBox6.Enabled = radioButton3.Checked;
                comboBox5.Enabled = radioButton3.Checked;
                
                if (radioButton3.Checked)
                {
                    set_defaults();

                    trackBar3.SetRange(0, image.GetUpperBound(1)- int.Parse(comboBox5.Text));
                    trackBar3.Update();
                    trackBar6.SetRange(0, image.GetUpperBound(0) - int.Parse(comboBox6.Text));
                    trackBar6.Update();
                }
                
            }
            catch { }
        }

        private void trackBar3_Scroll_1(object sender, EventArgs e)
        {
            framing();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            framing();
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            framing();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            framing();
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                trackBar3.Enabled = radioButton3.Checked;
                trackBar6.Enabled = radioButton3.Checked;
                comboBox6.Enabled = radioButton3.Checked;
                comboBox5.Enabled = radioButton3.Checked;

                if (radioButton3.Checked)
                {
                    if (comboBox6.Text == null || comboBox6.Text == string.Empty)
                    {
                        comboBox6.SelectedIndex = 1;
                        comboBox5.SelectedIndex = 1;
                        comboBox6.Text = comboBox6.Items[1].ToString();
                        comboBox5.Text = comboBox6.Items[1].ToString();
                    }

                    trackBar3.SetRange(0, image.GetUpperBound(1) - int.Parse(comboBox5.Text));
                    trackBar3.Update();
                    trackBar6.SetRange(0, image.GetUpperBound(0) - int.Parse(comboBox6.Text));
                    trackBar6.Update();
                }
                framing();
            }
            catch { }
        }
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            framing();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            execute();
        }

        private void set_defaults()
        {
            if (comboBox6.Text == null || comboBox6.Text == string.Empty)
            {
                comboBox6.SelectedIndex = 1;
                comboBox6.Text = comboBox6.Items[1].ToString();
            }

            if (comboBox5.Text == null || comboBox5.Text == string.Empty)
            {
                comboBox5.SelectedIndex = 1;
                comboBox5.Text = comboBox6.Items[1].ToString();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox12_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox12.Checked)
            {
                if (double.TryParse(textBox1.Text, out double val))
                {
                    recordingTomer.Interval = val*1000;
                    recordingTomer.Start();
                }
            }
            else recordingTomer.Stop();
        }

        private void RecordingTimerAction(object sender, System.Timers.ElapsedEventArgs args)
        {
            OnSnap(null, null);
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textBox2.Text,out double val))
            {
                subtractiong_value = val;
            }
        }

        private void button5_Click_2(object sender, EventArgs e)
        {

            //var graphic1 = ImageSource.draw(100, 300, null);
            //pictureBox2.Image = new Bitmap(graphic1.GetUpperBound(1) + 1, graphic1.GetUpperBound(0) + 1, 3 * (graphic1.GetUpperBound(1) + 1),
            //    System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(graphic1, 0));
            //pictureBox2.Update();



            //var graphic2 = ImageSource.draw(100, 300, null);

            //pictureBox3.Image = new Bitmap(graphic2.GetUpperBound(1) + 1, graphic2.GetUpperBound(0) + 1, 3 * (graphic2.GetUpperBound(1) + 1),
            //    System.Drawing.Imaging.PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(graphic2, 0));
            //pictureBox3.Update();


        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); ;
            string folderPath = Path.Combine(folder, FolderPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            DateTime dt = DateTime.UtcNow;
            string path = Path.Combine(folderPath, dt.ToString().Replace('.', '_').Replace(':', '_').Replace(' ', '_'));
            path += dt.Millisecond.ToString();
            hpi2.Save(path);
        }
    }
}
