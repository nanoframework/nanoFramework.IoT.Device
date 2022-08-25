// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using Iot.Device.Ds1621;
using nanoFramework.Hardware.Esp32;
using nanoFramework.TestFramework;
using System;
using System.Device.Gpio;
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

        [Setup]
        public void SetupDs1621Tests()
        {
            try
            {
                Debug.WriteLine("Please adjust for your own usage. If you need another hardware, please add the proper nuget and adjust as well");

                // Setup ESP32 I2C port.
                Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

                // Setup Ds1621 device. 
                I2cConnectionSettings i2cSettings = new I2cConnectionSettings(1, Ds1621.Ds1621.DefaultI2cAddress);
                _i2cDevice = new I2cDevice(i2cSettings);
                _thermometer = new Ds1621.Ds1621(_i2cDevice, MeasurementMode.Single);
            }
            catch
            {
                Assert.SkipTest("I2C port not supported in this platform or not properly configured");
            }
        }

        [TestMethod]
        public void Constructor_Cannot_Create_With_Null_I2C_Device()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new Ds1621.Ds1621(null));
            Assert.Throws(typeof(ArgumentNullException), () => new Ds1621.Ds1621(null, MeasurementMode.Single));
        }

        #region Temperature Conversion

        // Check PackTemperature against the example data provided in the datasheet.
        [TestMethod]
        public void PackTemperature_Outputs_Correct_Values()
        {
            Temperature temperature;
            byte[] packResult;

            temperature = Temperature.FromDegreesCelsius(125);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0x7D, packResult[0]);
            Assert.Equal((byte)0x00, packResult[1]);

            temperature = Temperature.FromDegreesCelsius(25);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0x19, packResult[0]);
            Assert.Equal((byte)0x00, packResult[1]);

            temperature = Temperature.FromDegreesCelsius(0.5);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0x00, packResult[0]);
            Assert.Equal((byte)0x80, packResult[1]);

            temperature = Temperature.FromDegreesCelsius(0);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0x00, packResult[0]);
            Assert.Equal((byte)0x00, packResult[1]);

            temperature = Temperature.FromDegreesCelsius(-0.5);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0xFF, packResult[0]);
            Assert.Equal((byte)0x80, packResult[1]);

            temperature = Temperature.FromDegreesCelsius(-25);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0xE7, packResult[0]);
            Assert.Equal((byte)0x00, packResult[1]);

            temperature = Temperature.FromDegreesCelsius(-55);
            packResult = Ds1621.Ds1621.PackTemperature(temperature);
            Assert.Equal((byte)0xC9, packResult[0]);
            Assert.Equal((byte)0x00, packResult[1]);
        }

        // Check UnpackTemperature against the example data provided in the datasheet.
        [TestMethod]
        public void UnpackTemperature_Outputs_Correct_Values()
        {
            byte temperatureMSB;
            byte temperatureLSB;
            Temperature unpackResult;

            temperatureMSB = 0x7D;
            temperatureLSB = 0x00;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(125, unpackResult.DegreesCelsius);

            temperatureMSB = 0x19;
            temperatureLSB = 0x00;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(25, unpackResult.DegreesCelsius);

            temperatureMSB = 0x00;
            temperatureLSB = 0x80;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(0.5, unpackResult.DegreesCelsius);

            temperatureMSB = 0x00;
            temperatureLSB = 0x00;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(0, unpackResult.DegreesCelsius);

            temperatureMSB = 0xFF;
            temperatureLSB = 0x80;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(-0.5, unpackResult.DegreesCelsius);

            temperatureMSB = 0xE7;
            temperatureLSB = 0x00;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(-25, unpackResult.DegreesCelsius);

            temperatureMSB = 0xC9;
            temperatureLSB = 0x00;
            unpackResult = Ds1621.Ds1621.UnpackTemperature(temperatureMSB, temperatureLSB);
            Assert.Equal(-55, unpackResult.DegreesCelsius);
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
                double expectedTemperature;

                if (inputTemperature >= 0)
                {
                    if (fractionalPortion >= 0.75)
                    {
                        // Round temperature up away from zero.
                        expectedTemperature = wholePortion + 1;
                    }
                    else if (fractionalPortion <= 0.25)
                    {
                        // Round temperature down toward zero.
                        expectedTemperature = wholePortion;
                    }
                    else
                    {
                        // Round temperature to nearest half degree.
                        expectedTemperature = wholePortion + 0.5d;
                    }
                }
                else
                {
                    if (fractionalPortion >= -0.25)
                    {
                        // Round temperature down away from zero.
                        expectedTemperature = wholePortion;
                    }
                    else if (fractionalPortion <= -0.75)
                    {
                        // Round temperature up toward zero.
                        expectedTemperature = wholePortion - 1;
                    }
                    else
                    {
                        // Round temperature to nearest half degree.
                        expectedTemperature = wholePortion - 0.5d;
                    }
                }

                Assert.Equal(expectedTemperature, outputTemperature.DegreesCelsius);
            }
        }

        #endregion

        #region Low Temperature Alarm

        [TestMethod]
        public void Get_And_Set_LowTemperatureAlarm_Property()
        {
            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(-55);
            Assert.Equal(-55, _thermometer.LowTemperatureAlarm.DegreesCelsius);

            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(-25);
            Assert.Equal(-25, _thermometer.LowTemperatureAlarm.DegreesCelsius);

            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(-0.5);
            Assert.Equal(-0.5, _thermometer.LowTemperatureAlarm.DegreesCelsius);

            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(0);
            Assert.Equal(0, _thermometer.LowTemperatureAlarm.DegreesCelsius);

            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(0.5);
            Assert.Equal(0.5, _thermometer.LowTemperatureAlarm.DegreesCelsius);

            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(25);
            Assert.Equal(25, _thermometer.LowTemperatureAlarm.DegreesCelsius);

            _thermometer.LowTemperatureAlarm = Temperature.FromDegreesCelsius(125);
            Assert.Equal(125, _thermometer.LowTemperatureAlarm.DegreesCelsius);
        }

        #endregion

        #region High Temperature Alarm

        [TestMethod]
        public void Get_And_Set_HighTemperatureAlarm_Property()
        {
            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(-55);
            Assert.Equal(-55, _thermometer.HighTemperatureAlarm.DegreesCelsius);

            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(-25);
            Assert.Equal(-25, _thermometer.HighTemperatureAlarm.DegreesCelsius);

            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(-0.5);
            Assert.Equal(-0.5, _thermometer.HighTemperatureAlarm.DegreesCelsius);

            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(0);
            Assert.Equal(0, _thermometer.HighTemperatureAlarm.DegreesCelsius);

            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(0.5);
            Assert.Equal(0.5, _thermometer.HighTemperatureAlarm.DegreesCelsius);

            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(25);
            Assert.Equal(25, _thermometer.HighTemperatureAlarm.DegreesCelsius);

            _thermometer.HighTemperatureAlarm = Temperature.FromDegreesCelsius(125);
            Assert.Equal(125, _thermometer.HighTemperatureAlarm.DegreesCelsius);
        }

        #endregion

        #region Output Polarity

        [TestMethod]
        public void OutputPolarity_Property_Only_Changes_Relevant_Flag()
        {
            _thermometer.OutputPolarity = PinValue.High;
            byte before = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Configuration);

            _thermometer.OutputPolarity = PinValue.Low;
            byte after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Configuration);

            // Verify only OutputPolarity flag has been altered in configuration register.
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.OutputPolarityMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, (byte)RegisterMask.OutputPolarityMask);
        }

        [TestMethod]
        public void OutputPolarity_Property_Correctly_Gets_And_Sets_Flag()
        {
            _thermometer.OutputPolarity = PinValue.High;

            // Verify flag matches function return.
            PinValue pinValue = _thermometer.OutputPolarity;
            Assert.True(pinValue == PinValue.High);
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OutputPolarityMask));

            _thermometer.OutputPolarity = PinValue.Low;

            // Verify flag matches function return.
            pinValue = _thermometer.OutputPolarity;
            Assert.True(pinValue == PinValue.Low);
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OutputPolarityMask));
        }

        #endregion

        #region Measurement Mode

        [TestMethod]
        public void MeasurementMode_Property_Only_Changes_Relevant_Flag()
        {
            _thermometer.MeasurementMode = MeasurementMode.Single;
            byte before = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Configuration);

            _thermometer.MeasurementMode = MeasurementMode.Continuous;
            byte after = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Configuration);

            // Verify only MeasurementMode flag has been altered in configuration register.
            TestHelper.AssertMaskedRegistersEqual(before, after, (byte)~RegisterMask.OneShotConversionModeMask);
            TestHelper.AssertMaskedRegistersNotEqual(before, after, (byte)RegisterMask.OneShotConversionModeMask);
        }

        [TestMethod]
        public void MeasurementMode_Property_Correctly_Gets_And_Sets_Flag()
        {
            _thermometer.MeasurementMode = MeasurementMode.Single;

            // Verify flag matches function return.
            MeasurementMode mode = _thermometer.MeasurementMode;
            Assert.True(mode == MeasurementMode.Single);
            Assert.True(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OneShotConversionModeMask));

            _thermometer.MeasurementMode = MeasurementMode.Continuous;

            // Verify flag matches function return.
            mode = _thermometer.MeasurementMode;
            Assert.True(mode == MeasurementMode.Continuous);
            Assert.False(RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OneShotConversionModeMask));
        }

        #endregion
    }
}
