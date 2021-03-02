using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PhaseImageRecorderToupCam
{
    [XmlRootAttribute("Settings")]
    public class Settings
    {
        public string device_name { get; set; } = "Axiovert";
        public string path { get; set; } = "images";
        public string resolution { get; set; } = "1024*1024";
        public int exposition { get; set; } = 85;
        public int gain { get; set; } = 85;
        public int saturation { get; set; } = 85;
        public bool auto_exposition { get; set; } = false;
        public int x_field { get; set; } = 12;
        public int y_field { get; set; } = 12;
        public bool framing_enabled { get; set; } = true;
        public int x_frame_size { get; set; } = 512;
        public int y_frame_size { get; set; } = 512;
        public int x_frame_position { get; set; } = 0;
        public int y_frame_position { get; set; } = 0;
        public double wavelength { get; set; } = 632.8;
        public int color { get; set; } = 0;

        public void SaveSettings(string filename = "settings.xml")
        {
            try
            {
                string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); ;
                string path = Path.Combine(folder, filename);
                XmlSerializer formatter = new XmlSerializer(typeof(Settings));
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, this);
                }
            }
            catch { }
        }

        public static bool TryLoadSettings(out Settings settings, string filename = "settings.xml")
        {
            settings = null;
            try
            {
                string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); ;
                string path = Path.Combine(folder, filename);
                XmlSerializer formatter = new XmlSerializer(typeof(Settings));
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    var temp = formatter.Deserialize(fs) as Settings;
                    if (temp != null)
                    {
                        settings = temp;
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                int q =0;
            }
            return false;
        }
    }
}
