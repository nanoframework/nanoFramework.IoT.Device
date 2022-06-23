// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkFunLcd
{
    using System;
    using System.Device.I2c;
    using System.Drawing;
    using System.Threading;
    using Iot.Device.CharacterLcd;

    /// <summary>
    /// LCD library for SparkFun RGB Serial Open LCD display (sizes 20x4 or 16x2) with I2C connection
    /// for product information see https://www.sparkfun.com/products/16398
    /// code based on https://github.com/sparkfun/OpenLCD
    /// </summary>
    public partial class SparkFunLcd
    {
        /// <summary>
        /// OpenLCD device command
        /// </summary>
        private enum OpenLcdCommandEnum : byte
        {
            /// <summary>
            /// Special command message, command follows
            /// </summary>
            SpecialCommand = 0xfe,

            /// <summary>
            /// Settings command message, settings detail follows
            /// </summary>
            SettingCommand = 0x7C,

            /// <summary>
            /// Clear and home the display
            /// </summary>
            ClearCommand = 0x2D,

            /// <summary>
            /// Change the contrast setting
            /// </summary>
            ContrastCommand = 0x18,

            /// <summary>
            /// Change the I2C address
            /// </summary>
            AddressCommand = 0x19,

            /// <summary>
            /// Set backlight RGB value
            /// </summary>
            SetRgbCommand = 0x2B,

            /// <summary>
            /// Enable system messages to be displayed
            /// </summary>
            EnableSystemMessageDisplay = 0x2E,

            /// <summary>
            /// Disable system messages from being displayed
            /// </summary>
            DisableSystemMessageDisplay = 0x2F,

            /// <summary>
            /// Enable splash screen at power on
            /// </summary>
            EnableSplashDisplay = 0x30,

            /// <summary>
            /// Disable splash screen at power on
            /// </summary>
            DisableSpashDisplay = 0x31,

            /// <summary>
            /// Save current text on display as splash screen
            /// </summary>
            SaveCurrentDisplayAsSplash = 0x0A,

            /// <summary>
            /// Return cursor home
            /// </summary>
            ReturnHome = 0x02,

            /// <summary>
            /// Entry mode command
            /// </summary>
            EntryModeSet = 0x04,

            /// <summary>
            /// Display Control Command
            /// </summary>
            DisplayControl = 0x08,

            /// <summary>
            /// Cursor Shift Command
            /// </summary>
            CursorShift = 0x10,

            /// <summary>
            /// Set DDRAM Address
            /// </summary>
            SetDdramAddress = 0x80,

            /// <summary>
            /// Enable right entry auto-scroll
            /// </summary>
            EntryRight = 0x00,

            /// <summary>
            /// Enable left entry auto-scroll
            /// </summary>
            EntryLeft = 0x02,

            /// <summary>
            /// Auto-scroll per character increment command
            /// </summary>
            EntryShiftIncrement = 0x01,

            /// <summary>
            /// Auto-Scroll per character decrement command
            /// </summary>
            EntryShiftDecrement = 0x00,

            /// <summary>
            /// Display ON Command
            /// </summary>
            DisplayOn = 0x04,

            /// <summary>
            /// Display OFF Command
            /// </summary>
            DisplayOff = 0x00,

            /// <summary>
            /// Cursor ON
            /// </summary>
            CursorOn = 0x02,

            /// <summary>
            /// Cursor OFF
            /// </summary>
            CursorOff = 0x00,

            /// <summary>
            /// Cursor blink ON Command
            /// </summary>
            BlinkOn = 0x01,

            /// <summary>
            /// Cursor blink OFF Command
            /// </summary>
            BlinkOff = 0x00,

            /// <summary>
            /// Display Move Command
            /// </summary>
            DisplayMove = 0x08,

            /// <summary>
            /// Cursor Move Command
            /// </summary>
            CursorMove = 0x00,

            /// <summary>
            /// Move Right Command
            /// </summary>
            MoveRight = 0x04,

            /// <summary>
            /// Move Left Command
            /// </summary>
            MoveLeft = 0x00
        }
    }
}
