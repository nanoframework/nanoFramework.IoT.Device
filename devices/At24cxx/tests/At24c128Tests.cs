// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.At24cxx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.TestFramework;
using System;
using System.Device.I2c;
using System.Diagnostics;

namespace Iot.Device.NFUnitTest
{
    [TestClass]
    public class At24c128Tests
    {
        static I2cDevice _i2cDevice;
        static At24c128 _eeprom;
        static Random _random;

        // Expected device metrics for At24c128 from datasheet (page 16 section 6. Memory Organization).
        // https://ww1.microchip.com/downloads/aemDocuments/documents/OTH/ProductDocuments/DataSheets/AT24C128C-AT24C256C-Data-Sheet-DS20006270B.pdf
        const int _expectedSize = 16384;
        const int _expectedPageSize = 64;
        const int _expectedPageCount = 256;

        [Setup]
        public void SetupTests()
        {
            try
            {
                Debug.WriteLine("Please adjust for your own usage. If you need another hardware, please add the proper nuget and adjust as well");

                // Setup ESP32 I2C port.
                Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

                // Setup device. 
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, At24c128.DefaultI2cAddress);
                _i2cDevice = new I2cDevice(i2cSettings);
                _eeprom = new At24c128(_i2cDevice);

                // Setup random number generator.
                _random = new Random();
            }
            catch
            {
                Assert.SkipTest("I2C port not supported in this platform or not properly configured");
            }
        }

        [TestMethod]
        public void Constructor_Throws_Exception_When_Null_I2C_Device_Is_Passed_As_Parameter()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new At24c128(null));
        }

        [TestMethod]
        public void Constructor_Has_Correct_Device_Metrics()
        {
            Assert.Equal(_expectedSize, _eeprom.Size);
            Assert.Equal(_expectedPageSize, _eeprom.PageSize);
            Assert.Equal(_expectedPageCount, _eeprom.PageCount);
        }

        #region Write Byte

        [TestMethod]
        public void WriteByte_Writes_To_Device_Correctly_Within_Addressable_Range_Of_Device()
        {
            int[] address = new int[16];

            // Generate some random valid addresses to test.
            for (int i = 0; i < address.Length; i++)
            {
                address[i] = _random.Next(_eeprom.Size);
            }

            // Test writing a byte to each address.
            for (int i = 0; i < address.Length; i += 8)
            {
                byte randomValue = (byte)_random.Next(byte.MaxValue);

                // Verify write byte returns the expected number of bytes written.
                uint bytesWritten = _eeprom.WriteByte(address[i], randomValue);
                Assert.Equal(1, bytesWritten, $"WriteByte reports writing {bytesWritten} byte(s) to address 0x{address[i]:X2}, expected 1 byte to be written.");

                // Validate data was correctly written.
                byte valueRead = _eeprom.ReadByte(address[i]);
                Assert.Equal(randomValue, valueRead, $"ReadByte reports reading {valueRead} from address 0x{address[i]:X2}, expected {randomValue}.");
            }
        }

        [TestMethod]
        public void WriteByte_Throws_Exception_When_Address_Less_Than_Zero()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.WriteByte(-1, 0));
        }

        [TestMethod]
        public void WriteByte_Throws_Exception_When_Address_Greater_Than_Eeprom_Size()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.WriteByte(_eeprom.Size + 1, 0));
        }

        #endregion

        #region Write

        [TestMethod]
        public void Write_Writes_To_Device_Correctly_Within_Addressable_Range_Of_Device()
        {
            int address = 0;

            // Generate a set of random data.
            byte[] randomData = new byte[_eeprom.PageSize];

            for (int j = 0; j < randomData.Length; j++)
            {
                randomData[j] = (byte)_random.Next(byte.MaxValue);
            }

            // Verify write returns the expected number of bytes written.
            uint bytesWritten = _eeprom.Write(address, randomData);
            Assert.Equal(randomData.Length, bytesWritten, $"WriteByte reports writing {bytesWritten} byte(s) at address 0x{address:X2}, expected {randomData.Length} byte(s) to be written.");

            // Validate data was correctly written.
            byte[] dataRead = _eeprom.Read(address, randomData.Length);

            for (int k = 0; k < dataRead.Length; k++)
            {
                Assert.Equal(randomData[k], dataRead[k], $"{k}");
            }
        }

        [TestMethod]
        public void Write_Throws_Exception_When_Address_Less_Than_Zero()
        {
            byte[] data = new byte[16];

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Write(-1, data));
        }

        [TestMethod]
        public void Write_Throws_Exception_When_Address_Greater_Than_Eeprom_Size()
        {
            byte[] data = new byte[16];

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Write(_eeprom.Size + 1, data));
        }

        [TestMethod]
        public void Write_Throws_Exception_When_Data_Length_Greater_Than_Eeprom_Page_Size()
        {
            byte[] data = new byte[_eeprom.PageSize + 1];

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Write(0, data));
        }

        #endregion

        #region Read Byte

        [TestMethod]
        public void ReadByte_Throws_Exception_When_Address_Less_Than_Zero()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.ReadByte(-1));
        }

        [TestMethod]
        public void ReadByte_Throws_Exception_When_Address_Greater_Than_Eeprom_Size()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.ReadByte(_eeprom.Size + 1));
        }

        #endregion

        #region Read

        [TestMethod]
        public void Read_Throws_Exception_When_Address_Less_Than_Zero()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Read(-1, 1));
        }

        [TestMethod]
        public void Read_Throws_Exception_When_Address_Greater_Than_Eeprom_Size()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Read(_eeprom.Size + 1, 1));
        }

        [TestMethod]
        public void Read_Throws_Exception_When_Length_Is_Zero()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Read(0, 0));
        }

        [TestMethod]
        public void Read_Throws_Exception_When_Length_Is_Less_Than_Zero()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Read(0, -1));
        }

        [TestMethod]
        public void Read_Throws_Exception_When_Length_Causes_Read_To_Exceed_Addressable_Range()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => _eeprom.Read(0, _eeprom.Size + 1));
        }

        #endregion
    }
}
