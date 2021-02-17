using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseImageRecorderToupCam
{
    public class Settings
    {
        public int Id { get; set; }
        public DateTime time { get; set; } = DateTime.UtcNow;
        public string device_name { get; set; }
        public string work_folder { get; set; }
        public int x_resolution { get; set; }
        public int y_resolution { get; set; }
        public int exposition { get; set; }
        public int x_frame_size { get; set; }
        public int y_frame_size { get; set; }
        public int x_frame_position { get; set; }
        public int y_frame_position { get; set; }
        public int selectIndexCombo2 { get; set; }
        public int selectIndexCombo3 { get; set; }
        public bool auto_exposition { get; set; }

        public void Update(Settings upd)
        {
            this.time = upd.time;
            this.device_name = upd.device_name;
            this.work_folder = upd.work_folder;
            this.x_resolution = upd.x_resolution;
            this.y_resolution = upd.y_resolution;
            this.exposition = upd.exposition;
            this.x_frame_size = upd.x_frame_size;
            this.y_frame_size = upd.y_frame_size;
            this.x_frame_position = upd.x_frame_position;
            this.y_frame_position = upd.y_frame_position;
            this.selectIndexCombo2 = upd.selectIndexCombo2;
            this.selectIndexCombo3 = upd.selectIndexCombo3;
            this.auto_exposition = upd.auto_exposition;
        }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Settings> Settings { get; set; }
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=176.119.156.220;Port=5432;Database=settingsdb;Username=postgres;Password=qw12cv90");
        }
    }
}
