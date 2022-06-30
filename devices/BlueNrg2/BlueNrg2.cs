// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using BlueNrg2;
using Iot.Device.BlueNrg2.Aci;
using Microsoft.Extensions.Logging;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using nanoFramework.Logging;

namespace Iot.Device.BlueNrg2
{
    public class BlueNrg2 : INativeDevice
    {
        private readonly IHardwareInterface _hardwareInterface;
#if DEBUG
        private readonly ILogger _logger;
#endif
        private readonly bool _shouldDispose;

        internal readonly Gap Gap;
        internal readonly Gatt Gatt;
        internal readonly Hal Hal;
        internal readonly Hci Hci;
        internal readonly L2Cap L2Cap;
        internal Events Events;

        private bool _isRunning;
        private bool _running;
        private bool _isDiscoverable;
        private bool _isConnectable;
        private string _deviceName;
        private Service[] _gattServices;
        private ServiceContext[] _services;

        /// <summary>
        /// Creates Instance of BlueNrg2 class
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
            Events = new Events();
            var transportLayer = new TransportLayer(Events, _hardwareInterface);
            Hci = new Hci(transportLayer);
            Gap = new Gap(transportLayer);
            Gatt = new Gatt(transportLayer);
            Hal = new Hal(transportLayer);
            L2Cap = new L2Cap(transportLayer);

#if DEBUG
            _logger.LogInformation("BlueNRG-2 Application");
#endif

            Hci.Reset();

            Thread.Sleep(2000);

            StartBluetoothThread();

#if DEBUG
            byte hardwareVersion = 0;
            ushort firmWareVersion = 0;
            GetBlueNrgVersion(ref hardwareVersion, ref firmWareVersion);
            _logger.LogInformation($"HardwareVersion: {hardwareVersion}");
            _logger.LogInformation($"FirmwareVersion: {firmWareVersion}");
#endif

            byte bluetoothDeviceAddressDataLength = 0;
            var bluetoothDeviceAddress = new byte[6];
            var ret = Hal.ReadConfigData(Offset.StaticRandomAddress, ref bluetoothDeviceAddressDataLength, ref bluetoothDeviceAddress);

#if DEBUG
            if (ret != BleStatus.Success)
                _logger.LogError("Read static random address failed.");
#endif

            if ((bluetoothDeviceAddress[5] & 0xC0) != 0xC0)
            {
#if DEBUG
                _logger.LogError("Static random address is not well formed.");
#endif
                Thread.Sleep(Timeout.Infinite);
            }

            ret = Hal.WriteConfigData(Offset.BluetoothPublicAddress, bluetoothDeviceAddressDataLength, bluetoothDeviceAddress);
#if DEBUG
            if (ret != BleStatus.Success)
            {
                _logger.LogError($"Hal.WriteConfigData() failed: {ret}");
            }
            else
            {
                _logger.LogInformation("Hal.WriteConfigData --> SUCCESS");
            }
#endif

            Hal.SetTransmitterPowerLevel(true, 4);

#if DEBUG
            if (ret != BleStatus.Success)
            {
                _logger.LogError($"Hal.SetTransmitterPowerLevel() failed: {ret}");
            }
            else
            {
                _logger.LogInformation("Hal.SetTransmitterPowerLevel --> SUCCESS");
            }
#endif

            ret = Gatt.Init();
            if (ret != BleStatus.Success)
            {
#if DEBUG
                _logger.LogError($"Gatt.Init() failed: {ret}");
#endif
                Thread.Sleep(Timeout.Infinite);
            }
#if DEBUG
            else
            {
                _logger.LogInformation("Gatt.Init --> SUCCESS");
            }
#endif

            ushort serviceHandle = 0;
            ushort deviceNameCharacteristicHandle = 0;
            ushort appearanceCharacteristicHandle = 0;
            ret = Gap.Init(Role.Peripheral, false, 0x07, ref serviceHandle, ref deviceNameCharacteristicHandle, ref appearanceCharacteristicHandle);
            if (ret != BleStatus.Success)
            {
#if DEBUG
                _logger.LogError($"Gap.Init() failed: {ret}");
#endif
                Thread.Sleep(Timeout.Infinite);
            }
#if DEBUG
            else
            {
                _logger.LogInformation("Gap.Init --> SUCCESS");
            }
#endif

            var deviceName = "BlueNRG".ToCharArray();
            var deviceNameBytes = new byte[deviceName.Length];
            deviceName.CopyTo(deviceNameBytes, 0);
            ret = Gatt.UpdateCharacteristicValue(serviceHandle, deviceNameCharacteristicHandle, 0, (byte)deviceName.Length, deviceNameBytes);
            if (ret != BleStatus.Success)
            {
#if DEBUG
                _logger.LogError($"Gatt.UpdateCharacteristicValue() failed: {ret}");
#endif
                Thread.Sleep(Timeout.Infinite);
            }
#if DEBUG
            else
            {
                _logger.LogInformation("Gatt.UpdateCharacteristicValue --> SUCCESS");
            }
#endif

            ret = Gap.ClearSecurityDatabase();
            
#if DEBUG
            if (ret != BleStatus.Success)
            {
                _logger.LogError($"Gap.ClearSecurityDatabase failed: {ret}");
            }
            else
            {
                _logger.LogInformation("Gap.ClearSecurityDatabase --> SUCCESS");
            }
#endif

#if DEBUG
            _logger.LogInformation("BLE stack initialized with SUCCESS");
#endif
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _running = false;
            while (_isRunning)
            {
                Thread.Sleep(1);
            }

            if (_shouldDispose)
                _hardwareInterface.Dispose();
        }

        internal void StartBluetoothThread()
        {
            _running = true;
            new Thread(
                () =>
                {
                    _isRunning = true;
                    while (_running)
                    {
                        Hci.UserEventProcess();
                    }

                    _isRunning = false;
                }
            ).Start();
        }

        private void GetBlueNrgVersion(ref byte hardwareVersion, ref ushort firmwareVersion)
        {
            byte hciVersion = 0, lmpPalVersion = 0;
            ushort hciRevision = 0, manufacturerName = 0, lmpPalSubversion = 0;

            var status = Hci.ReadLocalVersionInformation(
                ref hciVersion,
                ref hciRevision,
                ref lmpPalVersion,
                ref manufacturerName,
                ref lmpPalSubversion
            );

            if (status != BleStatus.Success)
                return;
            hardwareVersion = (byte)(hciRevision >> 8);
            firmwareVersion = (ushort)((hciVersion & 0xFF) << 8);
            firmwareVersion |= (ushort)((lmpPalSubversion >> 4 & 0xF) << 4);
            firmwareVersion |= (ushort)(lmpPalSubversion & 0xF);
        }

        public void InitService()
        {
        }

        public bool StartAdvertising(bool isDiscoverable, bool isConnectable, byte[] deviceName, ArrayList services)
        {
            throw new NotImplementedException();
        }

        public void StopAdvertising()
        {
            throw new NotImplementedException();
        }

        public int NotifyClient(ushort connection, ushort characteristicId, byte[] notifyBuffer)
        {
            throw new NotImplementedException();
        }

        public void ReadRespondWithValue(ushort eventId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void ReadRespondWithProtocolError(ushort eventId, byte otherError)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteGetData(ushort eventId)
        {
            throw new NotImplementedException();
        }

        public void WriteRespond(ushort eventId)
        {
            throw new NotImplementedException();
        }

        public void WriteRespondWithProtocolError(ushort eventId, byte protocolError)
        {
            throw new NotImplementedException();
        }
    }
}
