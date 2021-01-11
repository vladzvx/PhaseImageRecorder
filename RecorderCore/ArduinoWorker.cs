using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using ArduinoDriver;

namespace RecorderCore
{
    public class ArduinoWorker
    {
        ArduinoDriver.ArduinoDriver driver;     
        private void init()
        {
            driver = new ArduinoDriver.ArduinoDriver(ArduinoUploader.Hardware.ArduinoModel.NanoR3, autoBootstrap: true);
            for (int i = 2; i <= 10; i++)
                driver.Send(new ArduinoDriver.SerialProtocol.PinModeRequest((byte)i, ArduinoDriver.SerialProtocol.PinMode.Output));
            driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)(8), DigitalValue.High));
        }
        public void Action(int k)
        {
            if (driver == null) init();
            for (int i = 2; i <= 10; i++)
                driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)i, DigitalValue.Low));

            driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)(k + 2), DigitalValue.High));
        }
    }
}
