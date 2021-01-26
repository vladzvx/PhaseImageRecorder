using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using ArduinoDriver;

namespace RecorderCore
{
    public class ArduinoWorker
    {
        private object locker = new object();
        ArduinoDriver.ArduinoDriver driver;
        
        public void Stop()
        {
            lock (locker)
            {
                if (driver != null)
                    driver.Dispose();
            }
        }
        public ArduinoWorker()
        {
            try
            {
                init();
            }
            catch (Exception ex)
            {
                driver = null;
            }

            
        }
        public void init()
        {
            try
            {
                lock (locker)
                {
                    if (driver != null)
                    {
                        driver = new ArduinoDriver.ArduinoDriver(ArduinoUploader.Hardware.ArduinoModel.NanoR3, autoBootstrap: true);
                        for (int i = 2; i <= 10; i++)
                            driver.Send(new ArduinoDriver.SerialProtocol.PinModeRequest((byte)i, ArduinoDriver.SerialProtocol.PinMode.Output));
                        driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)(8), DigitalValue.High));
                    }
                }
            }
            catch { }


        }
        public void Action(int k)
        {
            //if (driver == null) init();
            if (driver == null) return;
            lock (locker)
            {
                for (int i = 2; i <= 10; i++)
                    driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)i, DigitalValue.Low));
                driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)(k + 2), DigitalValue.High));
            }
        }
    }
}
