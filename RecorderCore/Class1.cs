using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderCore
{
    
    public class HilbertCalculator
    {
        private ConcurrentQueue<byte[,,]> convertingQueue = new ConcurrentQueue<byte[,,]>();
    }
}
