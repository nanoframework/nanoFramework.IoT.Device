// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkfunLCD
{
    using System;
    using System.Device.I2c;
    using System.Threading;
    using nanoFramework.Hardware.Esp32;

    /// <summary>
    /// LCD library for SparkFun RGB Serial Open LCD display (sizes 20x4 or 16x2) with I2C connection
    /// for product information see https://www.sparkfun.com/products/16398
    /// code based on https://github.com/sparkfun/OpenLCD
    /// </summary>
    public class SparkfunLCD : IDisposable
    {
        /// <summary>
        /// Default I2C address
        /// </summary>
        public const byte DEFAULTDISPLAYADDRESS = 0x72;

        /// <summary>
        /// OpenLCD special command message, command follows
        /// </summary>
        private const byte SPECIALCOMMAND = 254;

        /// <summary>
        /// OpenLCD settings command message, settings detail follows
        /// </summary>
        private const byte SETTINGCOMMAND = 0x7C;

        /// <summary>
        /// OpenLCD command: clear and home the display
        /// </summary>
        private const byte CLEARCOMMAND = 0x2D;

        /// <summary>
        /// OpenLCD command: change the contrast setting
        /// </summary>
        private const byte CONTRASTCOMMAND = 0x18;

        /// <summary>
        /// OpenLCD command: change the I2C address
        /// </summary>
        private const byte ADDRESSCOMMAND = 0x19;

        /// <summary>
        /// OpenLCD command: set backlight RGB value
        /// </summary>
        private const byte SETRGBCOMMAND = 0x2B;

        /// <summary>
        /// OpenLCD command: enable system messages to be displayed
        /// </summary>
        private const byte ENABLESYSTEMMESSAGEDISPLAY = 0x2E;

        /// <summary>
        /// OpenLCD command: disable system messages from being displayed
        /// </summary>
        private const byte DISABLESYSTEMMESSAGEDISPLAY = 0x2F;

        /// <summary>
        /// OpenLCD command: enable splash screen at power on
        /// </summary>
        private const byte ENABLESPLASHDISPLAY = 0x30;

        /// <summary>
        /// OpenLCD command: disable splash screen at power on
        /// </summary>
        private const byte DISABLESPLASHDISPLAY = 0x31;

        /// <summary>
        /// OpenLCD command: save current text on display as splash screen
        /// </summary>
        private const byte SAVECURRENTDISPLAYASSPLASH = 0x0A;

        /// <summary>
        /// OpenLCD command: return cursor home
        /// </summary>
        private const byte LCDRETURNHOME = 0x02;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDENTRYMODESET = 0x04;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDDISPLAYCONTROL = 0x08;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDCURSORSHIFT = 0x10;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDSETDDRAMADDR = 0x80;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDENTRYRIGHT = 0x00;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDENTRYLEFT = 0x02;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDENTRYSHIFTINCREMENT = 0x01;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDENTRYSHIFTDECREMENT = 0x00;

        /// <summary>
        /// OpenLCD command: LCD display ON
        /// </summary>
        private const byte LCDDISPLAYON = 0x04;

        /// <summary>
        /// OpenLCD command: LCD display OFF
        /// </summary>
        private const byte LCDDISPLAYOFF = 0x00;

        /// <summary>
        /// OpenLCD command: Cursor ON
        /// </summary>
        private const byte LCDCURSORON = 0x02;

        /// <summary>
        /// OpenLCD command: Cursor OFF
        /// </summary>
        private const byte LCDCURSOROFF = 0x00;

        /// <summary>
        /// OpenLCD command: Cursor blink ON
        /// </summary>
        private const byte LCDBLINKON = 0x01;

        /// <summary>
        /// OpenLCD command: Cursor blink OFF
        /// </summary>
        private const byte LCDBLINKOFF = 0x00;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDDISPLAYMOVE = 0x08;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDCURSORMOVE = 0x00;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDMOVERIGHT = 0x04;

        /// <summary>
        /// OpenLCD command: 
        /// </summary>
        private const byte LCDMOVELEFT = 0x00;

        /// <summary>
        /// I2C connection to display
        /// </summary>
        private I2cDevice i2cDevice; // I2C connection

        /// <summary>
        /// display status
        /// </summary>
        private byte displayControl = LCDDISPLAYON | LCDCURSOROFF | LCDBLINKOFF;

        /// <summary>
        /// display auto-scrolling and flow state
        /// </summary>
        private byte displayMode = LCDENTRYLEFT | LCDENTRYSHIFTDECREMENT;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkfunLCD" /> class
        /// </summary>
        /// <param name="displaySize">display size</param>
        /// <param name="dataPin">ESP32 I2C data pin</param>
        /// <param name="clockPin">ESP32 I2C clock pin</param>
        public SparkfunLCD(DISPLAYSIZE displaySize = DISPLAYSIZE.SIZE20X4, int dataPin = 18, int clockPin = 19)
    : this(displaySize, busId: 1, deviceAddress: SparkfunLCD.DEFAULTDISPLAYADDRESS, i2cBusSpeed: I2cBusSpeed.StandardMode, dataPin, clockPin)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkfunLCD" /> class
        /// </summary>
        /// <param name="displaySize">display size</param>
        /// <param name="busId">I2C bus ID</param>
        /// <param name="deviceAddress">I2C address of LCD display</param>
        /// <param name="i2cBusSpeed">I2C bus speed</param>
        /// <param name="dataPin">ESP32 I2C data pin</param>
        /// <param name="clockPin">ESP32 I2C clock pin</param>
        public SparkfunLCD(DISPLAYSIZE displaySize = DISPLAYSIZE.SIZE20X4, int busId = 1, int deviceAddress = SparkfunLCD.DEFAULTDISPLAYADDRESS, I2cBusSpeed i2cBusSpeed = I2cBusSpeed.StandardMode, int dataPin = 18, int clockPin = 19)
            : this(new I2cConnectionSettings(busId, deviceAddress, i2cBusSpeed), displaySize, dataPin, clockPin)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkfunLCD" /> class
        /// </summary>
        /// <param name="settings">I2C settings for connection</param>
        /// <param name="displaySize">display size</param>
        /// <param name="dataPin">ESP32 I2C data pin</param>
        /// <param name="clockPin">ESP32 I2C clock pin</param>
        public SparkfunLCD(I2cConnectionSettings settings, DISPLAYSIZE displaySize = DISPLAYSIZE.SIZE20X4, int dataPin = 18, int clockPin = 19)
        {
            Configuration.SetPinFunction(dataPin, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(clockPin, DeviceFunction.I2C1_CLOCK);

            this.i2cDevice = I2cDevice.Create(settings);
            this.Init(displaySize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkfunLCD" /> class
        /// </summary>
        /// <param name="i2cDevice">existing I2C connection to display</param>
        /// <param name="displaySize">display size</param>
        public SparkfunLCD(I2cDevice i2cDevice, DISPLAYSIZE displaySize = DISPLAYSIZE.SIZE20X4)
        {
            this.i2cDevice = i2cDevice;
            this.Init(displaySize);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SparkfunLCD" /> class
        /// </summary>
        ~SparkfunLCD()
        {
            this.Dispose();
        }

        /// <summary>
        /// Display size
        /// </summary>
        public enum DISPLAYSIZE
        {
            /// <summary>
            /// Display size 20 x 4
            /// </summary>
            SIZE20X4,

            /// <summary>
            /// Display size 16 x 2
            /// </summary>
            SIZE16X2
        }

        /// <summary>
        /// Gets display maximum rows
        /// </summary>
        public byte MAXROWS { get; private set; }

        /// <summary>
        /// Gets display maximum columns
        /// </summary>
        public byte MAXCOLUMNS { get; private set; }

        /// <summary>
        /// Disposes of an instance of the class, implements <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            if (this.i2cDevice != null)
            {
                this.i2cDevice.Dispose();
                this.i2cDevice = null;
            }
        }

        /// <summary>
        /// Clear display and force cursor to beginning
        /// </summary>
        /// <seealso cref="Home"/>
        public void ClearScreen()
        {
            this.Command(CLEARCOMMAND);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Return cursor to beginning of the display, without clearing the display
        /// </summary>
        /// <seealso cref="ClearScreen"/>
        public void Home()
        {
            this.SpecialCommand(LCDRETURNHOME);
        }

        /// <summary>
        /// Set the cursor position to a particular column and row
        /// </summary>
        /// <param name="col">column 0 to 19</param>
        /// <param name="row">row 0 to 3</param>
        public void SetCursor(byte col, byte row)
        {
            int[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };

            // keep variables in bounds
            row = (byte)Math.Min(row, this.MAXROWS - 1); // row cannot be greater than max rows

            // send the command
            this.SpecialCommand((byte)(LCDSETDDRAMADDR | (col + row_offsets[row])));
        }

        /// <summary>
        /// Send request to create a customer character
        /// </summary>
        /// <param name="location">character number 0 to 7</param>
        /// <param name="charmap">byte array for character</param>
        public void CreateChar(byte location, byte[] charmap)
        {
            location &= 0x7; // we only have 8 locations 0-7
            this.Transmit(SETTINGCOMMAND); // Put LCD into setting mode
            this.Transmit((byte)(27 + location));
            for (int i = 0; i < 8; i++)
            {
                this.Transmit(charmap[i]);
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
            this.Command((byte)(35 + location));
        }

        /// <summary>
        /// Write a byte to the display
        /// </summary>
        /// <param name="b">byte to write</param>
        public void Write(byte b)
        {
            this.Transmit(b);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Write a character buffer to the display
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        public void Write(char[] buffer)
        {
            this.Write(buffer, buffer.Length);
        }

        /// <summary>
        /// Write a character buffer to the display
        /// </summary>
        /// <param name="buffer">buffer to write</param>
        /// <param name="bufferSize">size of buffer to write</param>
        public void Write(char[] buffer, int bufferSize)
        {
            int writeSize = Math.Min(bufferSize, buffer.Length);
            for (int i = 0; i < writeSize; i++)
            {
                this.Transmit((byte)buffer[i]);
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
            this.SetCursor(col, row);
            this.Write(str);
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
                this.Write(buffer, buffer.Length);
            }
        }

        /// <summary>
        /// Change state of display
        /// </summary>
        /// <param name="enable"><c>true</c> to enable display, else <c>false</c> to disable</param>
        public void DisplayState(bool enable)
        {
            if (enable)
            {
                this.displayControl |= LCDDISPLAYON;
            }
            else
            {
                byte tmp = LCDDISPLAYON;
                this.displayControl &= (byte)~tmp;
            }

            this.SpecialCommand((byte)(LCDDISPLAYCONTROL | this.displayControl));
        }

        /// <summary>
        /// Set state of underline cursor
        /// </summary>
        /// <param name="enable"><c>true</c> to enable cursor, else <c>false</c> to disable</param>
        public void CursorState(bool enable)
        {
            if (enable)
            {
                this.displayControl |= LCDCURSORON;
            }
            else
            {
                byte tmp = LCDCURSORON;
                this.displayControl &= (byte)~tmp;
            }

            this.SpecialCommand((byte)(LCDDISPLAYCONTROL | this.displayControl));
        }

        /// <summary>
        /// Set the state of the blink cursor
        /// </summary>
        /// <param name="enable"><c>true</c> to enable cursor blink, else <c>false</c> to disable</param>
        public void CursorBlinkState(bool enable)
        {
            if (enable)
            {
                this.displayControl |= LCDBLINKON;
            }
            else
            {
                byte tmp = LCDBLINKON;
                this.displayControl &= (byte)~tmp;
            }

            this.SpecialCommand((byte)(LCDDISPLAYCONTROL | this.displayControl));
        }

        /// <summary>
        /// Scroll the display multiple characters to the left, without changing the text
        /// </summary>
        /// <param name="count">number of characters to scroll</param>
        public void ScrollDisplayLeft(byte count = 1)
        {
            this.SpecialCommand(LCDCURSORSHIFT | LCDDISPLAYMOVE | LCDMOVELEFT, count);
        }

        /// <summary>
        /// Scroll the display multiple characters to the right, without changing the text
        /// </summary>
        /// <param name="count">number of characters to scroll</param>
        public void ScrollDisplayRight(byte count = 1)
        {
            this.SpecialCommand(LCDCURSORSHIFT | LCDDISPLAYMOVE | LCDMOVERIGHT, count);
        }

        /// <summary>
        /// Move the cursor multiple characters to the left
        /// </summary>
        /// <param name="count">number of characters to move</param>
        public void MoveCursorLeft(byte count = 1)
        {
            this.SpecialCommand(LCDCURSORSHIFT | LCDCURSORMOVE | LCDMOVELEFT, count);
        }

        /// <summary>
        /// Move the cursor multiple characters to the right
        /// </summary>
        /// <param name="count">number of characters to move</param>
        public void MoveCursorRight(byte count = 1)
        {
            this.SpecialCommand(LCDCURSORSHIFT | LCDCURSORMOVE | LCDMOVERIGHT, count);
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

            this.SetBacklight(r, g, b);
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
            byte red = (byte)(128 + this.Map(r, 0, 255, 0, 29));
            byte green = (byte)(158 + this.Map(g, 0, 255, 0, 29));
            byte blue = (byte)(188 + this.Map(b, 0, 255, 0, 29));

            // Turn display off to hide confirmation messages
            this.DisplayState(false);

            // Set the red, green and blue values
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(red);
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(green);
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(blue);

            // Turn display back on
            this.DisplayState(true);
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

            this.SetBacklight(r, g, b);
        }

        /// <summary>
        /// Set backlight color
        /// </summary>
        /// <param name="r">Red intensity</param>
        /// <param name="g">Green intensity</param>
        /// <param name="b">Blue intensity</param>
        public void SetBacklight(byte r, byte g, byte b)
        {
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(SETRGBCOMMAND);
            this.Transmit(r);
            this.Transmit(g);
            this.Transmit(b);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Change state of system messages
        /// </summary>
        /// <param name="state"><c>true</c> to enable system messages, else <c>false</c> to disable</param>
        public void SystemMessagesState(bool state)
        {
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(state ? ENABLESYSTEMMESSAGEDISPLAY : DISABLESYSTEMMESSAGEDISPLAY);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Change state of splash screen at power on, setting takes effect at next power on
        /// </summary>
        /// <param name="state"><c>true</c> to enable splash screen at power on, else <c>false</c> to disable</param>
        public void SplashScreenState(bool state)
        {
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(state ? ENABLESPLASHDISPLAY : DISABLESPLASHDISPLAY);
            Thread.Sleep(10);
        }

        /// <summary>
        /// Save the current display as the splash screen at power on, setting takes effect at next power on
        /// </summary>
        /// <seealso cref="SplashScreenState"/>
        public void SplashScreenSave()
        {
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(SAVECURRENTDISPLAYASSPLASH);
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
                this.displayMode |= LCDENTRYLEFT;
            }
            else
            {
                byte tmp = LCDENTRYLEFT;
                this.displayMode &= (byte)~tmp;
            }

            this.SpecialCommand((byte)(LCDENTRYMODESET | this.displayMode));
        }

        /// <summary>
        /// Change state of auto-scrolling, when enabled text will be right justified
        /// </summary>
        /// <param name="state"><c>true</c> to enable auto-scrolling, else <c>false</c> to disable</param>
        public void AutoScrollingState(bool state)
        {
            if (state)
            {
                this.displayMode |= LCDENTRYSHIFTINCREMENT;
            }
            else
            {
                byte tmp = LCDENTRYSHIFTINCREMENT;
                this.displayMode &= (byte)~tmp;
            }

            this.SpecialCommand((byte)(LCDENTRYMODESET | this.displayMode));
        }

        /// <summary>
        /// Set display contrast, default value is 120
        /// </summary>
        /// <param name="contrastValue">new contrast value</param>
        public void SetContrast(byte contrastValue)
        {
            this.Transmit(SETTINGCOMMAND);
            this.Transmit(CONTRASTCOMMAND);
            this.Transmit(contrastValue);

            Thread.Sleep(10);
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        /// <param name="displaySize">display size</param>
        private void Init(DISPLAYSIZE displaySize = DISPLAYSIZE.SIZE20X4)
        {
            switch (displaySize)
            {
                default:
                case DISPLAYSIZE.SIZE20X4:
                    this.MAXROWS = 20;
                    this.MAXCOLUMNS = 4;
                    break;
                case DISPLAYSIZE.SIZE16X2:
                    this.MAXROWS = 16;
                    this.MAXCOLUMNS = 2;
                    break;
            }

            this.Transmit(SPECIALCOMMAND); // Send special command character
            this.Transmit((byte)(LCDDISPLAYCONTROL | this.displayControl)); // Send the display command
            this.Transmit(SPECIALCOMMAND); // Send special command character
            this.Transmit((byte)(LCDENTRYMODESET | this.displayMode)); // Send the entry mode command
            this.Transmit(SETTINGCOMMAND); // Put LCD into setting mode
            this.Transmit(CLEARCOMMAND); // Send clear display command
            Thread.Sleep(50);
        }

        /// <summary>
        /// Send data to the device
        /// </summary>
        /// <param name="data">data to send</param>
        private void Transmit(byte data)
        {
            if (this.i2cDevice != null)
            {
                this.i2cDevice.WriteByte(data);
            }
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="command">command to send</param>
        private void Command(byte command)
        {
            this.Transmit(SETTINGCOMMAND); // Put LCD into setting mode
            this.Transmit(command); // Send the command code
            Thread.Sleep(10);
        }

        /// <summary>
        /// Send a special command to the display
        /// </summary>
        /// <param name="command">command to send</param>
        private void SpecialCommand(byte command)
        {
            this.Transmit(SPECIALCOMMAND); // Send special command character
            this.Transmit(command); // Send the command code
            Thread.Sleep(50);
        }

        /// <summary>
        /// Send multiple special commands to the display
        /// </summary>
        /// <param name="command">command to send</param>
        /// <param name="count">number of times to send</param>
        private void SpecialCommand(byte command, byte count)
        {
            for (int i = 0; i < count; i++)
            {
                this.Transmit(SPECIALCOMMAND); // Send special command character
                this.Transmit(command); // Send command code
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
