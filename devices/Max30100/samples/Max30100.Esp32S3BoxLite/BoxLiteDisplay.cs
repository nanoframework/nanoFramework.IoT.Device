// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Drawing;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Presentation.Media;
using nanoFramework.UI;

namespace Iot.Device.Max30100.Samples.Esp32S3BoxLite
{
    // This is based on the .NET nanoFramework Samples repository
    internal sealed class BoxLiteDisplay
    {
        private const int LcdMosi = 6;
        private const int LcdClock = 7;
        private const int LcdMiso = 39;
        private const int LcdChipSelect = 5;
        private const int LcdDataCommand = 4;
        private const int LcdReset = 48;
        private const int LcdBacklight = 45;
        private const int ScreenWidth = 320;
        private const int ScreenHeight = 240;
        private const int FramePixelCount = ScreenWidth * ScreenHeight;
        private const int NativeTransferBufferSize = ScreenWidth * 8 * 2;
        private const int TextBitmapWidth = 300;
        private const int TextBitmapHeight = 32;
        private const int DisplayBufferSize = 128 * 1024;

        private static readonly Color Background = Color.FromArgb(9, 22, 35);
        private static readonly Color Panel = Color.FromArgb(18, 48, 68);
        private static readonly Color Accent = Color.FromArgb(25, 204, 181);
        private static readonly Color Oxygen = Color.FromArgb(70, 160, 255);
        private static readonly Color Muted = Color.FromArgb(156, 178, 192);
        private static readonly Color Warning = Color.FromArgb(255, 190, 64);

        private Bitmap _textBitmap;
        private Font _labelFont;
        private Font _symbolFont;
        private Font _valueFont;
        private ushort[] _frameBuffer;
        private int _layout = -1;
        private string _lastTitle;
        private string _lastAcquisitionStatus;
        private int _lastProgressPercent = -1;
        private int _lastProgressWidth = -1;
        private int _lastAcquisitionQuality = -1;
        private int _lastHeartRate = -1;
        private int _lastOxygenSaturation = -1;
        private bool _lastHasOxygenSaturation;
        private int _lastMeasurementStatus = -1;
        private int _lastMeasurementQuality = -1;
        private bool _frameChanged;

        public void Initialize()
        {
            Configuration.SetPinFunction(LcdMosi, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(LcdClock, DeviceFunction.SPI1_CLOCK);
            Configuration.SetPinFunction(LcdMiso, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(LcdBacklight, DeviceFunction.PWM1);

            SpiConfiguration spiConfiguration = new SpiConfiguration(
                1,
                LcdChipSelect,
                LcdDataCommand,
                LcdReset,
                LcdBacklight);
            ScreenConfiguration screenConfiguration = new ScreenConfiguration(
                0,
                0,
                ScreenWidth,
                ScreenHeight,
                St7789BoxLiteDriver.GetDriver());

            uint allocatedBufferSize = DisplayControl.Initialize(
                spiConfiguration,
                screenConfiguration,
                DisplayBufferSize);
            Debug.WriteLine($"Display buffer allocated: {allocatedBufferSize} bytes.");

            if (allocatedBufferSize < NativeTransferBufferSize)
            {
                throw new System.InvalidOperationException(
                    "The native display buffer is too small for one graphics-driver transfer block.");
            }

            _labelFont = Resource.GetFont(Resource.FontResources.segoeuiregular12);
            _symbolFont = Resource.GetFont(Resource.FontResources.NinaB);
            _valueFont = Resource.GetFont(Resource.FontResources.Consolas23);
            _frameBuffer = new ushort[FramePixelCount];
            _textBitmap = new Bitmap(TextBitmapWidth, TextBitmapHeight);
        }

        public void ShowStarting()
        {
            DrawBackground();
            DrawCenteredText("MAX30100", _valueFont, Color.White, 10, 62, 300, 32, Background);
            DrawCenteredText("Starting sensor...", _labelFont, Accent, 10, 106, 300, 28, Background);
            DrawFooter();
            _layout = 0;
            Flush();
        }

        public void ShowSensorError(byte partId)
        {
            DrawBackground();
            DrawCenteredText("SENSOR ERROR", _valueFont, Color.Red, 10, 62, 300, 32, Background);
            DrawCenteredText($"Unexpected part ID: 0x{partId:X2}", _labelFont, Color.White, 10, 108, 300, 28, Background);
            DrawCenteredText("Check power and I2C wiring", _labelFont, Muted, 10, 138, 300, 28, Background);
            DrawFooter();
            _layout = 3;
            Flush();
        }

        public void ShowSensorDisconnected()
        {
            if (_layout == 4)
            {
                return;
            }

            DrawBackground();
            DrawCenteredText("SENSOR UNPLUGGED", _valueFont, Color.Red, 10, 62, 300, 32, Background);
            DrawCenteredText("Reconnect power, SDA and SCL", _labelFont, Color.White, 10, 108, 300, 28, Background);
            DrawCenteredText("Waiting for MAX30100", _labelFont, Muted, 10, 138, 300, 28, Background);
            DrawFooter();
            _layout = 4;
            Flush();
        }

        public void ShowAcquiring(string title, string status, double progress, double quality)
        {
            bool changed = false;
            if (_layout != 1)
            {
                DrawBackground();
                DrawHeader("PULSE OXIMETER");
                DrawCenteredText(
                    "Keep the sensor covered and your hand relaxed",
                    _labelFont,
                    Muted,
                    10,
                    181,
                    300,
                    28,
                    Background);
                DrawFooter();
                _lastTitle = null;
                _lastAcquisitionStatus = null;
                _lastProgressPercent = -1;
                _lastProgressWidth = -1;
                _lastAcquisitionQuality = -1;
                _layout = 1;
                changed = true;
            }

            if (title != _lastTitle)
            {
                DrawCenteredText(title, _valueFont, Color.White, 10, 62, 300, 32, Background);
                _lastTitle = title;
                changed = true;
            }

            if (status != _lastAcquisitionStatus)
            {
                DrawCenteredText(status, _labelFont, Muted, 10, 100, 300, 28, Background);
                _lastAcquisitionStatus = status;
                changed = true;
            }

            int progressPercent = (int)(progress + 0.5);
            int qualityPercent = (int)(quality + 0.5);
            int progressWidth = (int)(progress * 2.6);
            if (progressWidth < 0)
            {
                progressWidth = 0;
            }
            else if (progressWidth > 260)
            {
                progressWidth = 260;
            }

            if (progressWidth != _lastProgressWidth)
            {
                FillRectangle(30, 136, 260, 12, Panel);
                if (progressWidth > 0)
                {
                    FillRectangle(30, 136, progressWidth, 12, Accent);
                }

                _lastProgressWidth = progressWidth;
                changed = true;
            }

            if (progressPercent != _lastProgressPercent || qualityPercent != _lastAcquisitionQuality)
            {
                string detail = progressPercent > 0
                    ? $"Window {progressPercent}%   Signal {qualityPercent}%"
                    : "Waiting for stable contact";
                Font detailFont = progressPercent > 0 ? _symbolFont : _labelFont;
                DrawCenteredText(detail, detailFont, Color.White, 10, 153, 300, 28, Background);
                _lastProgressPercent = progressPercent;
                _lastAcquisitionQuality = qualityPercent;
                changed = true;
            }

            if (changed)
            {
                Flush();
            }
        }

        public void ShowMeasurements(
            double heartRate,
            bool isHeartRateHeld,
            bool hasOxygenSaturation,
            double oxygenSaturation,
            bool usedWeakSignalOxygenSaturation,
            double quality)
        {
            bool changed = false;
            if (_layout != 2)
            {
                DrawBackground();
                DrawHeader("LIVE MEASUREMENT");
                FillRectangle(10, 50, 145, 125, Panel);
                FillRectangle(165, 50, 145, 125, Panel);
                DrawCenteredText("HEART RATE", _labelFont, Muted, 10, 57, 145, 28, Panel);
                DrawCenteredText("BPM", _labelFont, Color.White, 10, 129, 145, 28, Panel);
                DrawCenteredText("OXYGEN", _labelFont, Muted, 165, 57, 145, 28, Panel);
                DrawFooter();
                _lastHeartRate = -1;
                _lastOxygenSaturation = -1;
                _lastHasOxygenSaturation = true;
                _lastMeasurementStatus = -1;
                _lastMeasurementQuality = -1;
                _layout = 2;
                changed = true;
            }

            int roundedHeartRate = (int)(heartRate + 0.5);
            if (roundedHeartRate != _lastHeartRate)
            {
                DrawCenteredText(roundedHeartRate.ToString(), _valueFont, Accent, 10, 89, 145, 32, Panel);
                _lastHeartRate = roundedHeartRate;
                changed = true;
            }

            int roundedOxygenSaturation = hasOxygenSaturation ? (int)(oxygenSaturation + 0.5) : -1;
            if (roundedOxygenSaturation != _lastOxygenSaturation || hasOxygenSaturation != _lastHasOxygenSaturation)
            {
                DrawCenteredText(
                    hasOxygenSaturation ? roundedOxygenSaturation.ToString() : "--",
                    _valueFont,
                    hasOxygenSaturation ? Oxygen : Muted,
                    165,
                    89,
                    145,
                    32,
                    Panel);
                DrawCenteredText(
                    hasOxygenSaturation ? "% SpO2" : "ACQUIRING",
                    hasOxygenSaturation ? _symbolFont : _labelFont,
                    Color.White,
                    165,
                    129,
                    145,
                    28,
                    Panel);
                _lastOxygenSaturation = roundedOxygenSaturation;
                _lastHasOxygenSaturation = hasOxygenSaturation;
                changed = true;
            }

            int measurementStatus = isHeartRateHeld ? 1 : usedWeakSignalOxygenSaturation ? 2 : 0;
            int qualityPercent = (int)(quality + 0.5);
            if (measurementStatus != _lastMeasurementStatus
                || (measurementStatus == 0 && qualityPercent != _lastMeasurementQuality))
            {
                string status = measurementStatus == 1
                    ? "Pulse held while signal recovers"
                    : measurementStatus == 2
                        ? "Weak optical signal"
                        : $"Signal quality {qualityPercent}%";
                Font statusFont = measurementStatus == 0 ? _symbolFont : _labelFont;
                DrawCenteredText(status, statusFont, Warning, 10, 181, 300, 28, Background);
                _lastMeasurementStatus = measurementStatus;
                _lastMeasurementQuality = qualityPercent;
                changed = true;
            }

            if (changed)
            {
                Flush();
            }
        }

        private void DrawBackground()
        {
            FillRectangle(0, 0, ScreenWidth, ScreenHeight, Background);
        }

        private void DrawHeader(string title)
        {
            FillRectangle(0, 0, ScreenWidth, 40, Panel);
            DrawCenteredText(title, _labelFont, Color.White, 10, 5, 300, 32, Panel);
            FillRectangle(0, 39, ScreenWidth, 1, Accent);
        }

        private void DrawFooter()
        {
            FillRectangle(0, 219, ScreenWidth, 21, Panel);
            DrawCenteredText(
                "EXPERIMENTAL - CALIBRATE BEFORE USE",
                _symbolFont,
                Warning,
                10,
                219,
                300,
                21,
                Panel);
        }

        private void DrawCenteredText(
            string text,
            Font font,
            Color color,
            int x,
            int y,
            int width,
            int height,
            Color background)
        {
            FillRectangle(x, y, width, height, background);
            _textBitmap.FillRectangle(0, 0, width, height, background);
            font.ComputeExtent(text, out int textWidth, out int textHeight);

            int textX = (width - textWidth) / 2;
            if (textX < 0)
            {
                textX = 0;
            }

            int textY = (height - textHeight) / 2;
            if (textY < 0)
            {
                textY = 0;
            }

            _textBitmap.DrawText(text, font, color, textX, textY);

            for (int bitmapY = 0; bitmapY < height; bitmapY++)
            {
                int frameIndex = ((y + bitmapY) * ScreenWidth) + x;
                for (int bitmapX = 0; bitmapX < width; bitmapX++)
                {
                    _frameBuffer[frameIndex++] = _textBitmap.GetPixel(bitmapX, bitmapY).ToBgr565();
                }
            }
        }

        private void FillRectangle(int x, int y, int width, int height, Color color)
        {
            ushort encodedColor = color.ToBgr565();
            int right = x + width;
            int bottom = y + height;
            for (int frameY = y; frameY < bottom; frameY++)
            {
                int frameIndex = (frameY * ScreenWidth) + x;
                int rowEnd = frameIndex + (right - x);
                while (frameIndex < rowEnd)
                {
                    _frameBuffer[frameIndex++] = encodedColor;
                }
            }

            _frameChanged = true;
        }

        private void Flush()
        {
            if (!_frameChanged)
            {
                return;
            }

            DisplayControl.Write(0, 0, ScreenWidth, ScreenHeight, _frameBuffer);
            _frameChanged = false;
        }    }
}
