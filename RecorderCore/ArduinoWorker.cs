using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using ArduinoDriver;

namespace RecorderCore
{
    public class ArduinoWorker
    {
        ArduinoDriver.ArduinoDriver driver = new ArduinoDriver.ArduinoDriver(ArduinoUploader.Hardware.ArduinoModel.Mega1284);
        public void Action(int k)
        {
            for (int i = 2; i <= 10; i++)
                driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)i, DigitalValue.Low));

            driver.Send(new ArduinoDriver.SerialProtocol.DigitalWriteRequest((byte)(k + 2), DigitalValue.High));
        }
    }
}
