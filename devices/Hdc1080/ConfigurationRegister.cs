// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hdc1080
{
    /// <summary>
    /// Configuration register, check data sheet, page 15, 8.6.3 section
    /// </summary>
    internal class ConfigurationRegister
    {
        public bool BatteryStatus { get; set; }
        public bool Heater { get; set; }
        public byte HumidityMeasurementResolution { get; set; }
        public bool SeparateReadings { get; set; }
        public bool ReservedAgain { get; set; }
        public bool SoftwareReset { get; set; }
        public byte TemperatureMeasurementResolution { get; set; }

        public byte GetData()
        {
            byte b = new byte();
            b.SetBit(0, HumidityMeasurementResolution.GetBit(0));
            b.SetBit(1, HumidityMeasurementResolution.GetBit(1));
            b.SetBit(2, TemperatureMeasurementResolution.GetBit(0));
            b.SetBit(3, BatteryStatus);
            b.SetBit(4, !SeparateReadings);
            b.SetBit(5, Heater);
            b.SetBit(6, ReservedAgain);
            b.SetBit(7, SoftwareReset);
            return b;
        }

        public void SetData(byte input)
        {
            byte b = new byte();
            b.SetBit(0, input.GetBit(0));
            b.SetBit(1, input.GetBit(1));
            HumidityMeasurementResolution = b;
            TemperatureMeasurementResolution = (byte)(input.GetBit(2) ? 0x01 : 0x00);
            BatteryStatus = input.GetBit(3);
            SeparateReadings = !input.GetBit(4);
            Heater = input.GetBit(5);
            ReservedAgain = input.GetBit(6);
            SoftwareReset = input.GetBit(7);
        }
    }
}