// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Tm1637;

Debug.WriteLine("Hello Tm1637!");
using Tm1637 tm1637 = new Tm1637(4, 0);
tm1637.Brightness = 7;
tm1637.ScreenOn = true;
tm1637.ClearDisplay();
// Displays 4 Characters
// If you have a 4 character display, all 4 will be displayed as well as on a 6
Character[] toDisplay = new Character[4]
{
    Character.Digit4, Character.Digit2 | Character.Dot, Character.Digit3, Character.Digit8
};
tm1637.Display(toDisplay);
Thread.Sleep(3000);

// Display a character at a specific segment position
// If you have a 4 display, only the fisrt 4 will be displayed as like as [0123]
// on a 6 segment one, all 6 will be displayed as like as [012345]
tm1637.Display(0, Character.Digit0);
tm1637.Display(1, Character.Digit1);
tm1637.Display(2, Character.Digit2);
tm1637.Display(3, Character.Digit3);
tm1637.Display(4, Character.Digit4);
tm1637.Display(5, Character.Digit5);
Thread.Sleep(3000);

// Changing order of the segments
tm1637.CharacterOrder = new byte[] { 2, 1, 0, 5, 4, 3 };

// Displays couple of raw data
Character[] rawData = new Character[6]
{
    // All led on including the dot
    (Character)0b1111_1111,
    // All led off
    (Character)0b0000_0000,
    // top blanck, right on, turning like this including dot
    (Character)0b1010_1010,
    // top on, right black, turning like this no dot
    (Character)0b0101_0101,
    // half one half off
    Character.SegmentTop | Character.SegmentTopRight | Character.SegmentBottomRight |
    Character.SegmentBottom,
    // half off half on
    Character.SegmentTopLeft | Character.SegmentBottomLeft | Character.SegmentMiddle | Character.Dot,
};
// If you have a 4 display, only the first 4 will be displayed
// on a 6 segment one, all 6 will be displayed
tm1637.Display(rawData);
Thread.Sleep(3000);

var digits = new Character[]
{
    Character.Digit0,
    Character.Digit1,
    Character.Digit2,
    Character.Digit3,
    Character.Digit4,
    Character.Digit5,
    Character.Digit6,
    Character.Digit7,
    Character.Digit8,
    Character.Digit9
};

for (int i = 0; i < 6; i++)
{
    rawData[i] = digits[i];
}

tm1637.Display(rawData);
Thread.Sleep(3000);

// If you have a 4 display, only the first 4 will be displayed, as like as [6549]
// on a 6 segment one, all 6 will be displayed, as like as [654987]
for (int i = 0; i < 6; i++)
{
    tm1637.Display((byte)i, digits[i]);
}

Thread.Sleep(3000);

// Revert order of the segments
tm1637.CharacterOrder = new byte[] { 0, 1, 2, 3, 4, 5 };

// Blink the screen by switching on and off
for (int i = 0; i < 10; i++)
{
    tm1637.ScreenOn = !tm1637.ScreenOn;
    tm1637.Display(rawData);
    Thread.Sleep(500);
}

tm1637.ScreenOn = true;

long bright = 0;
var counter = 0;
while (counter < 100)
{
    var dt = DateTime.UtcNow;
    toDisplay[0] = digits[dt.Minute / 10];
    toDisplay[1] = digits[dt.Minute % 10];
    toDisplay[2] = digits[dt.Second / 10];
    toDisplay[3] = digits[dt.Second % 10];
    tm1637.Brightness = (byte)(bright++ % 8);
    tm1637.Display(toDisplay);
    Thread.Sleep(100);
    counter++;
}

tm1637.ScreenOn = false;
tm1637.ClearDisplay();
