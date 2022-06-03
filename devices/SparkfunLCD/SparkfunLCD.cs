// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkfunLCD
{
    using System;
    using System.Device.I2c;
    using System.Threading;
    using Iot.Device.CharacterLcd;

    /// <summary>
    /// LCD library for SparkFun RGB Serial Open LCD display (sizes 20x4 or 16x2) with I2C connection
    /// for product information see https://www.sparkfun.com/products/16398
    /// code based on https://github.com/sparkfun/OpenLCD
    /// </summary>
    public class SparkfunLcd : ICharacterLcd
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const byte DefaultI2cAddress = 0x72;

        /// <summary>
        /// I2C connection to display
        /// </summary>
        private readonly I2cDevice _i2cDevice; // I2C connection

        /// <summary>
        /// display status
        /// </summary>
        private OpenLcdCommandEnum _displayControl = OpenLcdCommandEnum.DisplayOn | OpenLcdCommandEnum.CursorOff | OpenLcdCommandEnum.BlinkOff;

        /// <summary>
        /// display auto-scrolling and flow state
        /// </summary>
        private OpenLcdCommandEnum _displayMode = OpenLcdCommandEnum.EntryLeft | OpenLcdCommandEnum.EntryShiftDecrement;

        /// <summary>
        /// backing store for <see cref="DisplayOn"/>
        /// </summary>
        private bool _displayOn = true;

        /// <summary>
        /// backing store for <see cref="UnderlineCursorVisible"/>
        /// </summary>
        private bool _underlineCursorVisible = false;

        /// <summary>
        /// backing store for <see cref="BlinkingCursorVisible"/>
        /// </summary>
        private bool _blinkingCursorVisible = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkfunLcd" /> class
        /// </summary>
        /// <param name="i2cDevice">existing I2C connection to display</param>
        /// <param name="displaySize">display size</param>
        public SparkfunLcd(I2cDevice i2cDevice, DisplaySizeEnum displaySize = DisplaySizeEnum.Size20x4)
        {
            _i2cDevice = i2cDevice;
            Init(displaySize);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SparkfunLcd" /> class
        /// </summary>
        ~SparkfunLcd()
        {
            Dispose();
        }

        /// <summary>
        /// Display size
        /// </summary>
        public enum DisplaySizeEnum
        {
            /// <summary>
            /// Display size 20 x 4
            /// </summary>
            Size20x4,

            /// <summary>
            /// Display size 16 x 2
            /// </summary>
            Size16x2
        }

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

        /// <summary>
        /// Gets the size of the display
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the display is turned on
        /// </summary>
        public bool DisplayOn
        {
            get { return _displayOn; }
            set { SetDisplayState(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the underline cursor is enabled
        /// </summary>
        public bool UnderlineCursorVisible
        {
            get { return _underlineCursorVisible; }
            set { SetCursorUnderlineState(value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor is blinking
        /// </summary>
        public bool BlinkingCursorVisible
        {
            get { return _blinkingCursorVisible; }
            set { SetCursorBlinkState(value); }
        }

        /// <summary>
        /// Gets the number of custom character supported
        /// </summary>
        public int NumberOfCustomCharactersSupported
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the backlight is turned on
        /// </summary>
        public bool BacklightOn { get; set; }

        /// <summary>
        /// Disposes of an instance of the class
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Clear display and force cursor to beginning
        /// </summary>
        /// <seealso cref="Home"/>
        public void Clear()
        {
            Command(OpenLcdCommandEnum.ClearCommand);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Return cursor to beginning of the display, without clearing the display
        /// </summary>
        /// <seealso cref="Clear"/>
        public void Home()
        {
            TransmitSpecialCommand(OpenLcdCommandEnum.ReturnHome);
        }

        /// <summary>
        /// Set the cursor position to a particular column and row
        /// </summary>
        /// <param name="col">column 0 to 19</param>
        /// <param name="row">row 0 to 3</param>
        public void SetCursorPosition(int col, int row)
        {
            SetCursorPosition((byte)col, (byte)row);
        }

        /// <summary>
        /// Set the cursor position to a particular column and row
        /// </summary>
        /// <param name="col">column 0 to 19</param>
        /// <param name="row">row 0 to 3</param>
        public void SetCursorPosition(byte col, byte row)
        {
            int[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };

            // keep variables in bounds
            row = (byte)Math.Min(row, Size.Width - 1); // row cannot be greater than max rows

            // send the command
            TransmitSpecialCommand(OpenLcdCommandEnum.SetDdramAddress | (OpenLcdCommandEnum)(col + row_offsets[row]));
        }

        /// <summary>
        /// Send request to create a customer character
        /// </summary>
        /// <param name="location">character number 0 to 7</param>
        /// <param name="charmap">byte array for character</param>
        public void CreateChar(byte location, byte[] charmap)
        {
            location &= 0x7; // we only have 8 locations 0-7
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit((byte)(27 + location));
            for (int i = 0; i < 8; i++)
            {
                Transmit(charmap[i]);
            }

            Thread.Sleep(50);
        }

        /// <summary>
        /// Write a customer character to the display
        /// </summary>
        /// <param name="location">character number 0 to 7</param>
        public void WriteChar(byte location)
        {
            location &= 0x7; // we only have 8 locations 0-7
            Command((OpenLcdCommandEnum)(35 + location));
        }

        /// <summary>
        /// Write a byte to the display
        /// </summary>
        /// <param name="b">byte to write</param>
        public void Write(byte b)
        {
            Transmit(b);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Write a character buffer to the display
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        /// <seealso cref="Write(char[])"/>
        /// <seealso cref="Write(char[], int)"/>
        /// <seealso cref="Write(SpanChar)"/>
        public void Write(SpanChar buffer)
        {
            Write(buffer.ToArray());
        }

        /// <summary>
        /// Write a character buffer to the display
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        /// <seealso cref="Write(char[])"/>
        /// <seealso cref="Write(char[], int)"/>
        /// <seealso cref="Write(SpanChar)"/>
        public void Write(char[] buffer)
        {
            Write(buffer, buffer.Length);
        }

        /// <summary>
        /// Write a character buffer to the display
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        /// <param name="bufferSize">size of buffer to write</param>
        /// <seealso cref="Write(char[])"/>
        /// <seealso cref="Write(char[], int)"/>
        /// <seealso cref="Write(SpanChar)"/>
        public void Write(char[] buffer, int bufferSize)
        {
            int writeSize = Math.Min(bufferSize, buffer.Length);
            for (int i = 0; i < writeSize; i++)
            {
                Transmit((byte)buffer[i]);
            }

            Thread.Sleep(10);
        }

        /// <summary>
        /// Set cursor position and write string
        /// </summary>
        /// <param name="col">column 0 to 19</param>
        /// <param name="row">row 0 to 3</param>
        /// <param name="str">string to write</param>
        public void Write(byte col, byte row, string str)
        {
            SetCursorPosition(col, row);
            Write(str);
        }

        /// <summary>
        /// Write a string to the display
        /// </summary>
        /// <param name="str">string to write</param>
        public void Write(string str)
        {
            if (string.IsNullOrEmpty(str) == false)
            {
                char[] buffer = str.ToCharArray();
                Write(buffer, buffer.Length);
            }
        }

        /// <summary>
        /// Change state of display
        /// </summary>
        /// <param name="enable"><c>true</c> to enable display, else <c>false</c> to disable</param>
        public void SetDisplayState(bool enable)
        {
            if (enable)
            {
                _displayControl |= OpenLcdCommandEnum.DisplayOn;
            }
            else
            {
                OpenLcdCommandEnum tmp = OpenLcdCommandEnum.DisplayOn;
                _displayControl &= ~tmp;
            }

            TransmitSpecialCommand(OpenLcdCommandEnum.DisplayControl | _displayControl);

            _displayOn = enable;
        }

        /// <summary>
        /// Set state of cursor
        /// </summary>
        /// <param name="enable"><c>true</c> to enable cursor, else <c>false</c> to disable</param>
        public void SetCursorUnderlineState(bool enable)
        {
            if (enable)
            {
                _displayControl |= OpenLcdCommandEnum.CursorOn;
            }
            else
            {
                OpenLcdCommandEnum tmp = OpenLcdCommandEnum.CursorOn;
                _displayControl &= ~tmp;
            }

            TransmitSpecialCommand(OpenLcdCommandEnum.DisplayControl | _displayControl);

            _underlineCursorVisible = enable;
        }

        /// <summary>
        /// Set the state of the blink cursor
        /// </summary>
        /// <param name="enable"><c>true</c> to enable cursor blink, else <c>false</c> to disable</param>
        public void SetCursorBlinkState(bool enable)
        {
            if (enable)
            {
                _displayControl |= OpenLcdCommandEnum.BlinkOn;
            }
            else
            {
                OpenLcdCommandEnum tmp = OpenLcdCommandEnum.BlinkOn;
                _displayControl &= ~tmp;
            }

            TransmitSpecialCommand(OpenLcdCommandEnum.DisplayControl | _displayControl);

            _blinkingCursorVisible = enable;
        }

        /// <summary>
        /// Scroll the display multiple characters to the left, without changing the text
        /// </summary>
        /// <param name="count">number of characters to scroll</param>
        public void ScrollDisplayLeft(byte count = 1)
        {
            TransmitSpecialCommand(OpenLcdCommandEnum.CursorShift | OpenLcdCommandEnum.DisplayMove | OpenLcdCommandEnum.MoveLeft, count);
        }

        /// <summary>
        /// Scroll the display multiple characters to the right, without changing the text
        /// </summary>
        /// <param name="count">number of characters to scroll</param>
        public void ScrollDisplayRight(byte count = 1)
        {
            TransmitSpecialCommand(OpenLcdCommandEnum.CursorShift | OpenLcdCommandEnum.DisplayMove | OpenLcdCommandEnum.MoveRight, count);
        }

        /// <summary>
        /// Move the cursor multiple characters to the left
        /// </summary>
        /// <param name="count">number of characters to move</param>
        public void MoveCursorLeft(byte count = 1)
        {
            TransmitSpecialCommand(OpenLcdCommandEnum.CursorShift | OpenLcdCommandEnum.CursorMove | OpenLcdCommandEnum.MoveLeft, count);
        }

        /// <summary>
        /// Move the cursor multiple characters to the right
        /// </summary>
        /// <param name="count">number of characters to move</param>
        public void MoveCursorRight(byte count = 1)
        {
            TransmitSpecialCommand(OpenLcdCommandEnum.CursorShift | OpenLcdCommandEnum.CursorMove | OpenLcdCommandEnum.MoveRight, count);
        }

        /*
        /// <summary>
        /// Use a standard hex rgb value (0x00000000 to 0x00FFFFFF) to set the backlight color.
        /// The encoded long value has form (0x00RRGGBB) where RR, GG and BB
        /// are red, green, blue byte values in hex.The remaining two most
        /// significant bytes of the long value are ignored.
        /// </summary>
        /// <param name="rgb">hex encoded rgb value</param>
        public void SetBacklight(ulong rgb)
        {
            ulong tmp = (rgb >> 16) & 0x0000FF;
            byte r = (byte)tmp;
            tmp = (rgb >> 8) & 0x0000FF;
            byte g = (byte)tmp;
            tmp = rgb & 0x0000FF;
            byte b = (byte)tmp;

            SetBacklight(r, g, b);
        }

        /// <summary>
        /// Set backlight color
        /// </summary>
        /// <param name="r">Red intensity</param>
        /// <param name="g">Green intensity</param>
        /// <param name="b">Blue intensity</param>
        public void SetBacklight(byte r, byte g, byte b)
        {
            // map the byte value range to backlight command range
            byte red = (byte)(128 + Map(r, 0, 255, 0, 29));
            byte green = (byte)(158 + Map(g, 0, 255, 0, 29));
            byte blue = (byte)(188 + Map(b, 0, 255, 0, 29));

            // Turn display off to hide confirmation messages
            DisplayState(false);

            // Set the red, green and blue values
            Transmit(SETTINGCOMMAND);
            Transmit(red);
            Transmit(SETTINGCOMMAND);
            Transmit(green);
            Transmit(SETTINGCOMMAND);
            Transmit(blue);

            // Turn display back on
            DisplayState(true);
            Thread.Sleep(50);
        }
        */

        /// <summary>
        /// Set backlight color.
        /// The encoded long value has form (0x00RRGGBB) where RR, GG and BB
        /// are red, green, blue byte values in hex. The remaining two most
        /// significant bytes of the long value are ignored.
        /// </summary>
        /// <param name="rgb">hex encoded rgb value</param>
        public void SetBacklight(ulong rgb)
        {
            ulong tmp = (rgb >> 16) & 0x0000FF;
            byte r = (byte)tmp;
            tmp = (rgb >> 8) & 0x0000FF;
            byte g = (byte)tmp;
            tmp = rgb & 0x0000FF;
            byte b = (byte)tmp;

            SetBacklight(r, g, b);
        }

        /// <summary>
        /// Set backlight color
        /// </summary>
        /// <param name="r">Red intensity</param>
        /// <param name="g">Green intensity</param>
        /// <param name="b">Blue intensity</param>
        public void SetBacklight(byte r, byte g, byte b)
        {
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit(OpenLcdCommandEnum.SetRgbCommand);
            Transmit(r);
            Transmit(g);
            Transmit(b);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Change state of system messages
        /// </summary>
        /// <param name="state"><c>true</c> to enable system messages, else <c>false</c> to disable</param>
        public void SystemMessagesState(bool state)
        {
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit(state ? OpenLcdCommandEnum.EnableSystemMessageDisplay : OpenLcdCommandEnum.DisableSystemMessageDisplay);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Change state of splash screen at power on, setting takes effect at next power on
        /// </summary>
        /// <param name="state"><c>true</c> to enable splash screen at power on, else <c>false</c> to disable</param>
        public void SplashScreenState(bool state)
        {
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit(state ? OpenLcdCommandEnum.EnableSplashDisplay : OpenLcdCommandEnum.DisableSpashDisplay);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Save the current display as the splash screen at power on, setting takes effect at next power on
        /// </summary>
        /// <seealso cref="SplashScreenState"/>
        public void SplashScreenSave()
        {
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit(OpenLcdCommandEnum.SaveCurrentDisplayAsSplash);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Set state of text flow direction, note that left to right is the direction common to most Western languages.
        /// </summary>
        /// <param name="state"><c>true</c> to set flow from left to right, else <c>false</c> to set to right to left</param>
        public void TextFlowDirectionState(bool state = true)
        {
            if (state)
            {
                _displayMode |= OpenLcdCommandEnum.EntryLeft;
            }
            else
            {
                OpenLcdCommandEnum tmp = OpenLcdCommandEnum.EntryLeft;
                _displayMode &= ~tmp;
            }

            TransmitSpecialCommand(OpenLcdCommandEnum.EntryModeSet | _displayMode);
        }

        /// <summary>
        /// Change state of auto-scrolling, when enabled text will be right justified
        /// </summary>
        /// <param name="state"><c>true</c> to enable auto-scrolling, else <c>false</c> to disable</param>
        public void AutoScrollingState(bool state)
        {
            if (state)
            {
                _displayMode |= OpenLcdCommandEnum.EntryShiftIncrement;
            }
            else
            {
                OpenLcdCommandEnum tmp = OpenLcdCommandEnum.EntryShiftIncrement;
                _displayMode &= ~tmp;
            }

            TransmitSpecialCommand(OpenLcdCommandEnum.EntryModeSet | _displayMode);
        }

        /// <summary>
        /// Set display contrast, default value is 120
        /// </summary>
        /// <param name="contrastValue">new contrast value</param>
        public void SetContrast(byte contrastValue)
        {
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit(OpenLcdCommandEnum.ContrastCommand);
            Transmit(contrastValue);

            Thread.Sleep(10);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="characterMap"></param>
        public void CreateCustomCharacter(int location, byte[] characterMap)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="characterMap"></param>
        public void CreateCustomCharacter(int location, SpanByte characterMap)
        {
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        /// <param name="displaySize">display size</param>
        private void Init(DisplaySizeEnum displaySize = DisplaySizeEnum.Size20x4)
        {
            switch (displaySize)
            {
                default:
                case DisplaySizeEnum.Size20x4:
                    Size = new Size() { Width = 20, Height = 4 };
                    break;
                case DisplaySizeEnum.Size16x2:
                    Size = new Size() { Width = 16, Height = 2 };
                    break;
            }

            Transmit(OpenLcdCommandEnum.SpecialCommand);
            Transmit((byte)(OpenLcdCommandEnum.DisplayControl | _displayControl));
            Transmit(OpenLcdCommandEnum.SpecialCommand);
            Transmit((byte)(OpenLcdCommandEnum.EntryModeSet | _displayMode));
            Transmit(OpenLcdCommandEnum.SettingCommand);
            Transmit(OpenLcdCommandEnum.ClearCommand);
            Thread.Sleep(50);
        }

        /// <summary>
        /// Send data to the device
        /// </summary>
        /// <param name="data">data to send</param>
        private void Transmit(byte data)
        {
            if (_i2cDevice != null)
            {
                _i2cDevice.WriteByte(data);
            }
        }

        /// <summary>
        /// Send data to the device
        /// </summary>
        /// <param name="data">data to send</param>
        private void Transmit(OpenLcdCommandEnum data)
        {
            Transmit((byte)data);
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">command to send</param>
        private void Command(OpenLcdCommandEnum command)
        {
            Transmit(OpenLcdCommandEnum.SettingCommand); // Put LCD into setting mode
            Transmit(command); // Send the command code
            Thread.Sleep(10);
        }

        /// <summary>
        /// Send a special command to the display
        /// </summary>
        /// <param name="command">command to send</param>
        private void TransmitSpecialCommand(OpenLcdCommandEnum command)
        {
            Transmit(OpenLcdCommandEnum.SpecialCommand); // Send special command character
            Transmit(command); // Send the command code
            Thread.Sleep(50);
        }

        /// <summary>
        /// Send multiple special commands to the display
        /// </summary>
        /// <param name="command">command to send</param>
        /// <param name="count">number of times to send</param>
        private void TransmitSpecialCommand(OpenLcdCommandEnum command, byte count)
        {
            for (int i = 0; i < count; i++)
            {
                Transmit(OpenLcdCommandEnum.SpecialCommand); // Send special command character
                Transmit(command); // Send command code
            }

            Thread.Sleep(50);
        }

        /// <summary>
        /// Scale output linearly according to input and output ranges
        /// </summary>
        /// <param name="input">input value</param>
        /// <param name="in_min">minimum range of input value</param>
        /// <param name="in_max">maximum range of input value</param>
        /// <param name="out_min">minimum range of output value</param>
        /// <param name="out_max">maximum range of output value</param>
        /// <returns>returns scaled value</returns>
        private long Map(byte input, int in_min, int in_max, int out_min, int out_max)
        {
            return (((input - in_min) * (out_max - out_min)) / (in_max - in_min)) + out_min;
        }
    }
}
