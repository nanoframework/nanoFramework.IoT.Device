// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Iot.Device.LoRa;
using Iot.Device.LoRa.Drivers.Sx1262;

using nanoFramework.Hardware.Esp32;

namespace Sx1262Sample
{
    /// <summary>
    /// Sample entry point for the SX1262 LoRa transceiver on ESP32 (SPI1).
    /// </summary>
    public class Program
    {
        // ---- SX1262 pin mapping (HT-VME213) - SPI1 ----
        private const int PinLoraMosi = 10;
        private const int PinLoraClk = 9;
        private const int PinLoraMiso = 11;
        private const int PinLoraCs = 8;
        private const int PinLoraRst = 12;
        private const int PinLoraBusy = 13;
        private const int PinLoraDio1 = 14;

        private static ILoRaDevice _lora;

        /// <summary>
        /// Application entry point: initializes the SX1262, starts RX polling, and sends periodic test frames.
        /// </summary>
        public static void Main()
        {
            GpioController gpio = null;
            SpiDevice loraSpi = null;

            try
            {
                gpio = new GpioController();

                // ---- LoRa ----
                Configuration.SetPinFunction(PinLoraMosi, DeviceFunction.SPI1_MOSI);
                Configuration.SetPinFunction(PinLoraClk, DeviceFunction.SPI1_CLOCK);
                Configuration.SetPinFunction(PinLoraMiso, DeviceFunction.SPI1_MISO);

                SpiConnectionSettings spiSettings = new SpiConnectionSettings(1, PinLoraCs)
                {
                    ClockFrequency = 1000000,
                    Mode = SpiMode.Mode0,
                    DataBitLength = 8
                };
                loraSpi = SpiDevice.Create(spiSettings);

                _lora = new Sx1262(
                    loraSpi,
                    resetPin: PinLoraRst,
                    busyPin: PinLoraBusy,
                    dio1Pin: PinLoraDio1,
                    gpioController: gpio,
                    shouldDispose: false);

                Debug.WriteLine("LoRa SX1262 sample starting...");

                try
                {
                    _lora.Reset();

                    Debug.WriteLine("Initializing LoRa...");
                    _lora.Initialize();

                    // PacketReceived is raised from a background thread, so work can continue here without blocking the main thread.
                    _lora.PacketReceived += OnPacketReceived;

                    Debug.WriteLine("Starting LoRa receive polling...");
                    _lora.StartPolling();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LoRa initialization failed: " + ex.ToString());
                    CleanupLora();
                    loraSpi?.Dispose();
                    loraSpi = null;
                    gpio?.Dispose();
                    gpio = null;
                    while (true)
                    {
                        Thread.Sleep(60000);
                    }
                }

                // Send a message every 10 seconds from the main thread only, to avoid concurrency issues with the SX1262 driver.
                while (true)
                {
                    DoSend();
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Sample startup failed: " + ex.ToString());
                CleanupLora();
                loraSpi?.Dispose();
                gpio?.Dispose();
                while (true)
                {
                    Thread.Sleep(60000);
                }
            }
        }

        private static void CleanupLora()
        {
            if (_lora == null)
            {
                return;
            }

            try
            {
                _lora.PacketReceived -= OnPacketReceived;
                _lora.StopPolling();
                _lora.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoRa cleanup failed: " + ex.ToString());
            }

            _lora = null;
        }

        // Called from main thread only.
        private static void DoSend()
        {
            try
            {
                string message = "Hello from the .NET nanoFramework: " + DateTime.UtcNow;
                Debug.WriteLine(message);
                byte[] payload = Encoding.UTF8.GetBytes(message);

                _lora.Send(payload, 3000);

                Debug.WriteLine("Message sent. Whoo Hoo");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TX failed: " + ex.Message);
            }
        }

        private static void OnPacketReceived(object sender, LoRaMessage msg)
        {
            string text = Encoding.UTF8.GetString(msg.Payload, 0, msg.Payload.Length);
            Debug.WriteLine("RX: '" + text + "' RSSI=" + msg.Rssi + "dBm SNR=" + msg.Snr + "dB");
        }
    }
}
