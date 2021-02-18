using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PhaseImageRecorderToupCam
{
    class Settings
    {
        public string device_name;
        public string path;
        public string resolution;
        public int exposition;
        public bool auto_exposition;
        public int x_field;
        public int y_field;
        public int x_frame_size;
        public int y_frame_size;
        public int x_frame_position;
        public int y_frame_position;
        public double wavelength;
        public int color;
        public static Settings readSettings(string path = "settings.xml")
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(path);

            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode xnode in xRoot)
            {

            }
            return null;
        }

    }
}
