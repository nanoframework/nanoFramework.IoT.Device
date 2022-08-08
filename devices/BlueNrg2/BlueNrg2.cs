// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.BlueNrg2.Aci;
using Iot.Device.BlueNrg2.Aci.Events;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;

namespace Iot.Device.BlueNrg2
{
    /// <summary>
    /// The BlueNrg2 device.
    /// </summary>
    public class BlueNrg2
    {
        private readonly IHardwareInterface _hardwareInterface;
#if DEBUG
        private readonly ILogger _logger;
#endif
        private readonly bool _shouldDispose;

        /// <summary>
        /// Contains all ATT commands.
        /// </summary>
        public readonly Att Att;

        /// <summary>
        /// Contains all GAP commands.
        /// </summary>
        public readonly Gap Gap;

        /// <summary>
        /// Contains all GATT commands.
        /// </summary>
        public readonly Gatt Gatt;

        /// <summary>
        /// Contains all HAL commands.
        /// </summary>
        public readonly Hal Hal;

        /// <summary>
        /// Contains all HCI commands.
        /// </summary>
        public readonly Hci Hci;

        /// <summary>
        /// Contains all L2CAP commands.
        /// </summary>
        public readonly L2Cap L2Cap;

        /// <summary>
        /// Contains all events.
        /// </summary>
        public readonly EventProcessor EventProcessor;

        private bool _isRunning;
        private bool _running;

        /// <summary>
        ///     Creates Instance of BlueNrg2 class
        /// </summary>
        /// <param name="spiConnectionSettings">information about the SPI connection</param>
        /// <param name="extIPin">the pin to use for BLE interrupts</param>
        /// <param name="resetPin">the pin to use for resetting</param>
        /// <param name="controller">optional GpioController if one has been instantiated before</param>
        /// <param name="shouldDispose">If the instance should be disposed of</param>
        /// <exception cref="ArgumentException">thrown when invalid values are used for the pins</exception>
        public BlueNrg2(
            SpiConnectionSettings spiConnectionSettings,
            int extIPin,
            int resetPin,
            GpioController controller = null,
            bool shouldDispose = false)
        {
#if DEBUG
            _logger = this.GetCurrentClassLogger();
#endif
            var controller1 = controller ?? new GpioController();
            var pinCs = spiConnectionSettings.ChipSelectLine;
            spiConnectionSettings.ChipSelectLine = -1;
            var pinReset = resetPin >= 0
                ? resetPin
                : throw new ArgumentException($"value of pin {nameof(resetPin)} can only be positive!");
            var pinExtI = extIPin >= 0
                ? extIPin
                : throw new ArgumentException($"value of pin {nameof(extIPin)} can only be positive!");
            var spiDevice = new SpiDevice(spiConnectionSettings);
            _shouldDispose = shouldDispose || controller is null;
            _hardwareInterface = new SpiInterface(
                spiDevice,
                controller1,
                pinReset,
                pinExtI,
                pinCs
#if DEBUG
                ,
                _logger
#endif

            );
            EventProcessor = new EventProcessor();
            var transportLayer = new TransportLayer(EventProcessor, _hardwareInterface);
            Att = new Att(transportLayer);
            Hci = new Hci(transportLayer);
            Gap = new Gap(transportLayer);
            Gatt = new Gatt(transportLayer);
            Hal = new Hal(transportLayer);
            L2Cap = new L2Cap(transportLayer);

#if DEBUG
            _logger.LogInformation("BlueNRG-2 Application");
#endif

            Hci.Init();

            Thread.Sleep(2000);

            StartBluetoothThread();

            byte bluetoothDeviceAddressDataLength = 6;
            var bluetoothDeviceAddress = new byte[]
            {
                0xc0,
                0x00,
                0x00,
                0x00,
                0x00,
                0x01
            };
            Hal.WriteConfigData(
                Offset.BluetoothPublicAddress,
                bluetoothDeviceAddressDataLength,
                bluetoothDeviceAddress
            );
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _running = false;
            while (_isRunning)
                Thread.Sleep(1);

            if (_shouldDispose)
                _hardwareInterface.Dispose();
        }

        /// <summary>
        /// Resets BlueNrg2
        /// </summary>
        public void Reset()
        {
            Hci.Reset();
        }

        /// <summary>
        /// Starts the bluetooth thread.
        /// </summary>
        public void StartBluetoothThread()
        {
            _running = true;
            new Thread(
                () =>
                {
                    _isRunning = true;
                    while (_running)
                        Hci.UserEventProcess();

                    _isRunning = false;
                }
            ).Start();
        }
    }
}
