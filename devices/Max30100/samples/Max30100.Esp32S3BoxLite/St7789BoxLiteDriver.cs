// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.UI;

namespace Iot.Device.Max30100.Samples.Esp32S3BoxLite
{
    // This is coming from the .NET nanoFramework Samples repository
    internal static class St7789BoxLiteDriver
    {
        public static GraphicDriver GetDriver()
        {
            return new GraphicDriver()
            {
                MemoryWrite = (byte)St7789Register.MemoryWrite,
                SetColumnAddress = (byte)St7789Register.ColumnAddressSet,
                SetRowAddress = (byte)St7789Register.RowAddressSet,
                InitializationSequence = new byte[]
                {
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.SoftwareReset,
                    (byte)GraphicDriverCommandType.Sleep, 5,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.PixelFormatSet, 0x55,
                    (byte)GraphicDriverCommandType.Command, 6, (byte)St7789Register.PorchSetting, 0x0C, 0x0C, 0x00, 0x33, 0x33,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.GateControl, 0x35,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.VcomsSetting, 0x2B,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.LcmControl, 0x2C,
                    (byte)GraphicDriverCommandType.Command, 3, (byte)St7789Register.VdvVrhCommandEnable, 0x01, 0xFF,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.VrhSet, 0x11,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.VdvSet, 0x20,
                    (byte)GraphicDriverCommandType.Command, 2, (byte)St7789Register.FrameRateControl, 0x0F,
                    (byte)GraphicDriverCommandType.Command, 3, (byte)St7789Register.PowerControl1, 0xA4, 0xA1,
                    (byte)GraphicDriverCommandType.Command, 15, (byte)St7789Register.PositiveVoltageGamma, 0xD0, 0x00, 0x05, 0x0E, 0x15, 0x0D, 0x37, 0x43, 0x47, 0x09, 0x15, 0x12, 0x16, 0x19,
                    (byte)GraphicDriverCommandType.Command, 15, (byte)St7789Register.NegativeVoltageGamma, 0xD0, 0x00, 0x05, 0x0D, 0x0C, 0x06, 0x2D, 0x44, 0x40, 0x0E, 0x1C, 0x18, 0x16, 0x19,
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.InversionOn,
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.SleepOut,
                    (byte)GraphicDriverCommandType.Sleep, 2,
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.DisplayOn,
                    (byte)GraphicDriverCommandType.Sleep, 2,
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.NoOperation,
                    (byte)GraphicDriverCommandType.Sleep, 2,
                },
                OrientationLandscape = new byte[]
                {
                    (byte)GraphicDriverCommandType.Command,
                    2,
                    (byte)St7789Register.MemoryAccessControl,
                    (byte)(St7789Orientation.RowColumnSwap | St7789Orientation.RowOrder | St7789Orientation.BgrOrder),
                },
                PowerModeNormal = new byte[]
                {
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.SleepOut,
                },
                PowerModeSleep = new byte[]
                {
                    (byte)GraphicDriverCommandType.Command, 1, (byte)St7789Register.SleepIn,
                },
                DefaultOrientation = DisplayOrientation.Landscape,
                Brightness = (byte)St7789Register.WriteDisplayBrightness,
                SetWindowType = SetWindowType.X16bitsY16Bit,
            };
        }

        private enum St7789Register
        {
            NoOperation = 0x00,
            SoftwareReset = 0x01,
            SleepIn = 0x10,
            SleepOut = 0x11,
            InversionOn = 0x21,
            DisplayOn = 0x29,
            ColumnAddressSet = 0x2A,
            RowAddressSet = 0x2B,
            MemoryWrite = 0x2C,
            MemoryAccessControl = 0x36,
            PixelFormatSet = 0x3A,
            WriteDisplayBrightness = 0x51,
            PorchSetting = 0xB2,
            GateControl = 0xB7,
            VcomsSetting = 0xBB,
            LcmControl = 0xC0,
            VdvVrhCommandEnable = 0xC2,
            VrhSet = 0xC3,
            VdvSet = 0xC4,
            FrameRateControl = 0xC6,
            PowerControl1 = 0xD0,
            PositiveVoltageGamma = 0xE0,
            NegativeVoltageGamma = 0xE1,
        }

        [System.Flags]
        private enum St7789Orientation
        {
            BgrOrder = 0x08,
            RowColumnSwap = 0x20,
            RowOrder = 0x80,
        }
    }
}
