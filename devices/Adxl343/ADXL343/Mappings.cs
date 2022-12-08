//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

namespace ADXL343
{
    public enum ActInactCtlMap : byte
    {
        ACT_AC_DC = 0x80,
        ACT_X_ENABLE = 0x40,
        ACT_Y_ENABLE = 0x20,
        ACT_Z_ENABLE = 0x10,
        INACT_AC_DC = 0x08,
        INACT_X_ENABLE = 0x04,
        INACT_Y_ENABLE = 0x02,
        INACT_Z_ENABLE = 0x01
    }

    public enum ActTapStatusMap
    {
        ACT_X_SOURCE = 0x40,
        ACT_Y_SOURCE = 0x20,
        ACT_Z_SOURCE = 0x10,
        ASLEEP = 0x08,
        TAP_X_SOURCE = 0x04,
        TAP_Y_SOURCE = 0x02,
        TAP_Z_SOURCE = 0x01
    }

    public enum DataFormatMap
    {
        SELF_TEST = 0x80,
        SPI = 0x40,
        INT_INVERT = 0x20,
        RESERVED = 0x10,
        FULL_RES = 0x08,
        JUSTIFY = 0x04,
        RANGE = 0x03
    }

    public enum IntEnableMap
    {
        DATA_READY = 0x80,
        SINGLE_TAP = 0x40,
        DOUBLE_TAP = 0x20,
        ACTIVITY = 0x10,
        INACTIVITY = 0x08,
        FREEFALL = 0x04,
        WATERMARK = 0x02,
        OVERRUN = 0x01
    }

    public enum GravityRange
    {
        /// <summary>
        /// Plus/minus 2G.
        /// </summary>
        Range02 = 0x00,

        /// <summary>
        /// Plus/minus 4G.
        /// </summary>
        Range04 = 0x01,

        /// <summary>
        /// Plus/minus 8G.
        /// </summary>
        Range08 = 0x02,

        /// <summary>
        /// Plus/minus 16G.
        /// </summary>
        Range16 = 0x03
    }

    public enum PowerControlMap
    {
        LINK = 0x20,
        AUTO_SLEEP = 0x10,
        MEASURE = 0x08,
        SLEEP = 0x04,
        WAKEUP = 0x03
    }

    public enum BWRateMap
    {
        LOW_POWER = 0x10,
        RATE = 0x0F
    }

    public enum FIFIOStatusMap
    {
        FIFO_TRIG = 0x80,
        ENTRIES = 0x3F
    }

    public enum FifoMode : byte
    {
        BYPASS = 0x00,
        FIFO = 0x01,
        STREAM = 0x02,
        TRIGGER = 0x03,
    }

    public enum FifoControlMap : byte
    {
        MODE = 0xC0,
        TRIGGER = 0x20,
        SAMPLES = 0x0F
    }

    internal enum Register : byte
    {
        DEVID = 0x00,
        THRESH_TAP = 0x1D,
        OFSX = 0x1E,
        OFSY = 0x1F,
        OFSZ = 0x20,
        DUR = 0x21,
        LATENT = 0x22,
        WINDOW = 0x23,
        THRESH_ACT = 0x24,
        THRESH_INACT = 0x25,
        TIME_INACT = 0x26,
        ACT_INACT_CTL = 0x27,
        THRESH_FF = 0x28,
        TIME_FF = 0x29,
        TAP_AXES = 0x2A,
        ACT_TAP_STATUS = 0x2B,
        BW_RATE = 0x2C,
        POWER_CTL = 0x2D,
        INT_ENABLE = 0x2E,
        INT_MAP = 0x2F,
        INT_SOURCE = 0x30,
        DATA_FORMAT = 0x31,
        X0 = 0x32,  //2 bytes
        Y0 = 0x34,  //2 bytes
        Z0 = 0x36,  //2 bytes
        FIFO_CTL = 0x38,
        FIFO_STATUS = 0x39
    }

    public enum TapAxesMap : byte
    {
        SUPPRESS = 0x08,
        TAP_X_ENABLE = 0x04,
        TAP_Y_ENABLE = 0x02,
        TAP_Z_ENABLE = 0x01
    }
}
