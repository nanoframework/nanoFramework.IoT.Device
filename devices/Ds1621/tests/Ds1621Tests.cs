using Iot.Device.Common;
using Iot.Device.Ds1621;
using nanoFramework.Hardware.Esp32;
using nanoFramework.TestFramework;
using System;
using System.Device.I2c;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.NFUnitTest
{
    [TestClass]
    public class Ds1621Tests
    {
        static I2cDevice _i2cDevice;
        static Ds1621.Ds1621 _thermometer;

        private SpanByte ReadAllBuffers()
        {
            // Read all Ds1621 registers.
            SpanByte readBuffer = new byte[31];

            _i2cDevice.WriteByte((byte)Register.Temperature);
            _i2cDevice.Read(readBuffer);

            return readBuffer;
        }

        [Setup]
        public void SetupDs1621Tests()
        {
            try
            {
                Debug.WriteLine("Please adjust for your own usage. If you need another hardware, please add the proper nuget and adjust as well");

                // Setup ESP32 I2C port.
                Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);
                Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);

                // Setup Mcp7940m device. 
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Ds1621.Ds1621.DefaultI2cAddress);
                _i2cDevice = new I2cDevice(i2cSettings);
                _thermometer = new Ds1621.Ds1621(_i2cDevice);
            }
            catch
            {
                Assert.SkipTest("I2C port not supported in this platform or not properly configured");
            }
        }

        [TestMethod]
        public void Temperatures_Are_Rounded_To_Nearest_Half_Degree_Over_Valid_Temperature_Range()
        {
            for (double inputTemperature = -55; inputTemperature < 125; inputTemperature += 0.1d)
            {
                byte[] packResult = Ds1621.Ds1621.PackTemperature(Temperature.FromDegreesCelsius(inputTemperature));
                Temperature outputTemperature = Ds1621.Ds1621.UnpackTemperature(packResult[0], packResult[1]);

                double wholePortion = (int)inputTemperature;

                // Get fraction portion of temperature.
                double fractionalPortion = inputTemperature - wholePortion;

                if (fractionalPortion == 0 || fractionalPortion == 0.1d || fractionalPortion == 0.2d || fractionalPortion == 0.3d)
                {
                    Assert.Equal(wholePortion, outputTemperature.DegreesCelsius);
                }
                else if (fractionalPortion == 0.4d || fractionalPortion == 0.5d || fractionalPortion == 0.6d)
                {
                    Assert.Equal(wholePortion + 0.5d, outputTemperature.DegreesCelsius);
                }
                else
                {
                    Assert.Equal(wholePortion + 1, outputTemperature.DegreesCelsius);
                }
            }
        }
    }
}
