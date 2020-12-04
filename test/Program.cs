using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace test
{
    class Program
    {
        static int SpinsBymillisecond(int NIters)
        {
            SpinWait sw = new SpinWait();
            DateTime dt1 = DateTime.UtcNow;
            DateTime dt2 = DateTime.UtcNow;
            List<double> temp = new List<double>();
            int i = 0;
            while (i< NIters)
            {
                dt1 = DateTime.UtcNow;
                while (sw.Count < 10000)
                {
                    sw.SpinOnce(-1);
                }
                dt2 = DateTime.UtcNow;
                sw.Reset();
                temp.Add(dt2.Subtract(dt1).TotalMilliseconds);
                i++;
            }
            return (int)(10000/(temp.Sum() / temp.Count));
        }

        static void Main(string[] args)
        {
            int s1 = SpinsBymillisecond(1000);
            int s2 = SpinsBymillisecond(100);
            int s3= SpinsBymillisecond(300);
            int s4 = SpinsBymillisecond(10);
            int s5 = SpinsBymillisecond(100);

            SpinWait sw = new SpinWait();
            DateTime dt2 = DateTime.UtcNow;
            DateTime dt1 = DateTime.UtcNow;
            dt1 = DateTime.UtcNow;
            while (sw.Count < s1)
            {
                sw.SpinOnce(-1);
            }
            dt2 = DateTime.UtcNow;
            double d = dt2.Subtract(dt1).TotalMilliseconds;


        }
    }
}
