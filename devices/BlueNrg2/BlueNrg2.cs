// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.BlueNrg2.Aci;
using Microsoft.Extensions.Logging;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using nanoFramework.Logging;

namespace Iot.Device.BlueNrg2
{
    /// <summary>
    /// The BlueNrg2 device.
    /// </summary>
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
        private byte[] _deviceName;
        private GattLocalService[] _gattServices;
        private ServiceContext[] _services;
        private ushort[] _serviceHandles;
        private ushort[][] _characteristicHandles;

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

            Hci.Init();

            Thread.Sleep(2000);

            StartBluetoothThread();

            byte bluetoothDeviceAddressDataLength = 6;
            var bluetoothDeviceAddress = new byte[] { 0xc0, 0x00, 0x00, 0x00, 0x00, 0x01 };
            Hal.WriteConfigData(Offset.BluetoothPublicAddress, bluetoothDeviceAddressDataLength, bluetoothDeviceAddress);
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

        /// <inheritdoc />
        public void InitService()
        {
            Gatt.Init();
        }

        /// <inheritdoc />
        public bool StartAdvertising(bool isDiscoverable, bool isConnectable, byte[] deviceName, ArrayList services)
        {
            if (deviceName is null)
                throw new ArgumentNullException(nameof(deviceName));

            _isDiscoverable = isDiscoverable;
            _isConnectable = isConnectable;
            _deviceName = deviceName;

            _gattServices = new GattLocalService[services.Count];

            for (int i = 0; i < services.Count; i++)
            {
                _gattServices[i] = (GattLocalService)services[i];
            }

            ushort serviceHandle = 0, deviceNameCharacteristicHandle = 0, appearanceCharacteristicHandle = 0;

            Gap.Init(Role.Peripheral, false, 0x07, ref serviceHandle, ref deviceNameCharacteristicHandle,
                ref appearanceCharacteristicHandle);

            Gatt.UpdateCharacteristicValue(serviceHandle, deviceNameCharacteristicHandle, 0, BitConverter.GetBytes(_deviceName.Length)[3],
                _deviceName);

            Gap.SetIoCapability(IoCapability.DisplayOnly);

            Gap.SetAuthenticationRequirement(true, true, SecureConnectionSupport.Supported, false, 7, 16, false, 123456,
                AddressType.PublicIdentity);

            _serviceHandles = new ushort[services.Count];
            _characteristicHandles = new ushort[services.Count][];

            for (var i = 0; i < services.Count; i++)
            {
                var service = (GattLocalService)services[i];
                var uuid = service.Uuid.ToByteArray();

                // num of characteristics of the service
                var charCount = (byte)service.Characteristics.Length;
                // number of attribute records that can be added to the service
                var maxAttributeRecords = (byte)(1 + 3 * charCount);

                Gatt.AddService(UuidType.Uuid128, uuid, ServiceType.Primary, maxAttributeRecords, ref _serviceHandles[i]);

                _characteristicHandles[i] = new ushort[service.Characteristics.Length];

                for (var j = 0; j < service.Characteristics.Length; j++)
                {
                    var characteristic = service.Characteristics[j];
                    uuid = characteristic.Uuid.ToByteArray();
                    var characteristicValueLength = (ushort)characteristic.StaticValue.Length;
                    CharacteristicProperties characteristicProperties =
                        (CharacteristicProperties)((uint)characteristic.CharacteristicProperties & 0x11111111);

                    Gatt.AddCharacteristic(_serviceHandles[i], UuidType.Uuid128, uuid, characteristicValueLength,
                        characteristicProperties, SecurityPermissions.None,
                        Gatt.EventMask.NotifyReadRequestAndWaitForApprovalResponse, 16, false,
                        ref _characteristicHandles[i][j]);
                }
            }

            return true;
        }

        /// <inheritdoc />
        public void StopAdvertising()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int NotifyClient(ushort connection, ushort characteristicId, byte[] notifyBuffer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ReadRespondWithValue(ushort eventId, byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ReadRespondWithProtocolError(ushort eventId, byte otherError)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public byte[] WriteGetData(ushort eventId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void WriteRespond(ushort eventId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void WriteRespondWithProtocolError(ushort eventId, byte protocolError)
        {
            throw new NotImplementedException();
        }
    }
}
