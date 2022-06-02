// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.CharacterLcd;

namespace CharacterLcd.Samples
{
    internal static class LargeValueSample
    {
        /// <summary>
        /// This demonstrates the use of a large font on a 20x4 display. This is useful especially when showing values that should be readable from farther away,
        /// such as the time, or a temperature.
        /// </summary>
        /// <param name="lcd">The display</param>
        public static void LargeValueDemo(Hd44780 lcd)
        {
            LcdValueUnitDisplay value = new LcdValueUnitDisplay(lcd, "en");
            value.InitForRom("A00");
            value.Clear();
            Debug.WriteLine("Big clock test");
            int count = 0;
            while (count++ < 50)
            {
                value.DisplayTime(DateTime.UtcNow, "T");
                Thread.Sleep(200);
            }
            
            Debug.WriteLine("Showing fake temperature");
            value.DisplayValue("24.2 °C", "Temperature");
            Thread.Sleep(2000);

            Debug.WriteLine("Now showing a text");
            CancellationTokenSource src = new CancellationTokenSource();
            value.DisplayBigTextAsync("The quick brown fox jumps over the lazy dog at 10.45PM on May, 3rd", TimeSpan.FromMilliseconds(500), src.Token);
            Thread.Sleep(2000);
            src.Cancel();
        }
    }
}
