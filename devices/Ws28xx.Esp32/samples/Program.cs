// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Ws28xx.Esp32;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Drawing;

// Configure the count of pixels
const int Count = 10;
// Adjust the pin number
const int Pin = 15;

// Uncomment for WS2008
// Ws28xx neo = new Ws2808(Pin, Count);
// Uncomment for WS2812B
// Ws28xx neo = new Ws2812b(Pin, Count);
// Uncomment for WS2812C
// Ws28xx neo = new Ws2812c(Pin, Count);
// Comment if you are using one of the previous one
Ws28xx neo = new Sk6812(Pin, Count);

// BenchmarkClearPixel(); uncomment to benchmark

while (true)
{
    // Uncomment to run this test as well:
    // ColorFade(neo, Count);
    ColorWipe(neo, Color.White, Count);
    ColorWipe(neo, Color.Red, Count);
    ColorWipe(neo, Color.Green, Count);
    ColorWipe(neo, Color.Blue, Count);

    TheatreChase(neo, Color.White, Count);
    TheatreChase(neo, Color.Red, Count);
    TheatreChase(neo, Color.Green, Count);
    TheatreChase(neo, Color.Blue, Count);

    Rainbow(neo, Count);
    RainbowCycle(neo, Count);
    TheaterChaseRainbow(neo, Count);
}

void ColorWipe(Ws28xx neo, Color color, int count)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < count; i++)
    {
        img.SetPixel(i, 0, color);
        neo.Update();
    }
}

void ColorFade(Ws28xx neo, int count)
{
    BitmapImage img = neo.Image;

    // White
    for (byte iteration = 0; iteration < 255; iteration++)
    {
        for (var pixel = 0; pixel < count; pixel++)
        {
            img.SetPixel(pixel, 0, iteration, iteration, iteration);
        }
        neo.Update();
        System.Threading.Thread.Sleep(10);
    }

    // Red
    for (byte iteration = 0; iteration < 255; iteration++)
    {
        for (var pixel = 0; pixel < count; pixel++)
        {
            img.SetPixel(pixel, 0, iteration, 0, 0);
        }
        neo.Update();
        System.Threading.Thread.Sleep(10);
    }

    // Green
    for (byte iteration = 0; iteration < 255; iteration++)
    {
        for (var pixel = 0; pixel < count; pixel++)
        {
            img.SetPixel(pixel, 0, 0, iteration, 0);
        }
        neo.Update();
        System.Threading.Thread.Sleep(10);
    }

    // Blue
    for (byte iteration = 0; iteration < 255; iteration++)
    {
        for (var pixel = 0; pixel < count; pixel++)
        {
            img.SetPixel(pixel, 0, 0, 0, iteration);
        }
        neo.Update();
        System.Threading.Thread.Sleep(10);
    }
}

void TheatreChase(Ws28xx neo, Color color, int count, int iterations = 10)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < iterations; i++)
    {
        for (var j = 0; j < 3; j++)
        {
            for (var k = 0; k < count; k += 3)
            {
                if (j + k < count)
                {
                    img.SetPixel(j + k, 0, color);
                }
            }

            neo.Update();
            System.Threading.Thread.Sleep(100);
            for (var k = 0; k < count; k += 3)
            {
                if (j + k < count)
                {
                    img.SetPixel(j + k, 0, Color.Black);
                }
            }
        }
    }
}

Color Wheel(int position)
{
    if (position < 85)
    {
        return Color.FromArgb(position * 3, 255 - position * 3, 0);
    }
    else if (position < 170)
    {
        position -= 85;
        return Color.FromArgb(255 - position * 3, 0, position * 3);
    }
    else
    {
        position -= 170;
        return Color.FromArgb(0, position * 3, 255 - position * 3);
    }
}

void Rainbow(Ws28xx neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel((i + j) & 255));
        }

        neo.Update();
    }
}

void RainbowCycle(Ws28xx neo, int count, int iterations = 1)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255 * iterations; i++)
    {
        for (var j = 0; j < count; j++)
        {
            img.SetPixel(j, 0, Wheel(((j * 255 / count) + i) & 255));
        }

        neo.Update();
    }
}

void TheaterChaseRainbow(Ws28xx neo, int count)
{
    BitmapImage img = neo.Image;
    for (var i = 0; i < 255; i++)
    {
        for (var j = 0; j < 3; j++)
        {
            for (var k = 0; k < count; k += 3)
            {
                if (k + j < count)
                {
                    img.SetPixel(k + j, 0, Wheel((k + i) % 255));
                }
            }

            neo.Update();
            System.Threading.Thread.Sleep(100);

            for (var k = 0; k < count; k += 3)
            {
                if (k + j < count)
                {
                    img.SetPixel(k + j, 0, Color.Black);
                }
            }
        }
    }
}

void BenchmarkClearPixel()
{
    Stopwatch sw = new Stopwatch();

    sw.Start();
    for (int i = 0; i < 1000; i++)
    {
        neo.Image.Clear();
        neo.Update();
    }

    sw.Stop();
    Debug.WriteLine("Clear all" + sw.Elapsed.ToString());

    Stopwatch sw2 = new Stopwatch();

    sw2.Start();
    for (int i = 0; i < 1000; i++)
    {
        for (int y = 0; y < neo.Image.Height; y++)
        {
            for (int x = 0; x < neo.Image.Width; x++)
            {
                neo.Image.Clear(x, y);
            }
        }

        neo.Update();
    }

    sw2.Stop();
    Debug.WriteLine("Clear pixel by pixel " + sw2.Elapsed.ToString());
}