// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft6xx6x
{
    internal enum Register
    {
        Mode_Switch = 0x00,
        TD_STATUS = 0x02,
        P1_XH = 0x03,
        P1_XL = 0x04,
        P1_YH = 0x05,
        P1_YL = 0x06,
        P1_WEIGHT = 0x07,
        P1_MISC = 0x08,
        P2_XH = 0x09,
        P2_XL = 0x0A,
        P2_YH = 0x0B,
        P2_YL = 0x0C,
        P2_WEIGHT = 0x0D,
        P2_MISC = 0x0E,
        ID_G_THGROUP = 0x80,
        ID_G_THDIFF = 0x85,
        ID_G_CTRL = 0x86,
        ID_G_TIMEENTERMONITOR = 0x87,
        ID_G_PERIODACTIVE = 0x88,
        ID_G_PERIODMONITOR = 0x89,
        ID_G_FREQ_HOPPING_EN = 0x8B,
        ID_G_TEST_MODE_FILTER = 0x96,
        ID_G_CIPHER_MID = 0x9F,
        ID_G_CIPHER_LOW = 0xA0,
        ID_G_LIB_VERSION_H = 0xA1,
        ID_G_LIB_VERSION_L = 0xA2,
        ID_G_CIPHER_HIGH = 0xA3,
        ID_G_MODE = 0xA4,
        ID_G_PMODE = 0xA5,
        ID_G_FIRMID = 0xA6,
        ID_G_FOCALTECH_ID = 0xA8,
        ID_G_VIRTUAL_KEY_THRES = 0xA9,
        ID_G_IS_CALLING = 0xAD,
        ID_G_FACTORY_MODE = 0xAE,
        ID_G_RELEASE_CODE_ID = 0xAF,
        ID_G_FACE_DEC_MODE = 0xB0,
        ID_G_STATE = 0xBC,
        ID_G_SPEC_GESTURE_ENABLE = 0xD0,
    }
}
