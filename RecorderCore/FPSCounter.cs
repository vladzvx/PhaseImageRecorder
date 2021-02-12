using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    public class FPSCounter
    {
        private int Window { get; init; } = 50;
        private int counter = 0;
        private object locker = new object();
        private DateTime PrevCountTime = DateTime.MinValue;
        private DateTime LastCountTime = DateTime.MinValue;
        private ConcurrentQueue<double> queue = new ConcurrentQueue<double>();

        public FPSCounter()
        {

        }
        public FPSCounter(int window)
        {
            Window = window;
        }
        public double Count()
        {
            lock (locker)
            {
                PrevCountTime = LastCountTime;
                LastCountTime = DateTime.UtcNow;
                counter++;
                if (counter == 1) return 0;
            }
            queue.Enqueue(LastCountTime.Subtract(PrevCountTime).TotalSeconds);
            while (queue.Count > Window && queue.TryDequeue(out double val)) ;
            double[] array = queue.ToArray();
            return ((double)array.Length) / array.Sum();
        }
    }
}
