// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using Iot.Device.BlueNrg2.Aci;
using Iot.Device.BlueNrg2.Aci.Events;
using Microsoft.Extensions.Logging;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using nanoFramework.Device.Bluetooth.NativeDevice;
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
        private ushort[][] _characteristicHandles;
        private byte[] _deviceName;
        private ArrayList _eventIds;
        private GattLocalService[] _gattServices;
        private bool _isConnectable;
        private bool _isDiscoverable;

        private bool _isRunning;
        private ushort _nextEventId;
        private bool _running;
        private ushort[] _serviceHandles;
        private ServiceContext[] _services;
        internal EventProcessor _eventProcessor;

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
            _eventProcessor = new EventProcessor();
            var transportLayer = new TransportLayer(_eventProcessor, _hardwareInterface);
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

        /// <inheritdoc />
        public event EventHandler<NativeReadRequestedEventArgs> OnReadRequested;

        /// <inheritdoc />
        public event EventHandler<NativeWriteRequestedEventArgs> OnWriteRequested;

        /// <inheritdoc />
        public event EventHandler<NativeSubscribedClientsChangedEventArgs> OnClientSubscribed;

        /// <inheritdoc />
        public event EventHandler<NativeSubscribedClientsChangedEventArgs> OnClientUnsubscribed;

        /// <inheritdoc />
        public void InitService()
        {
            Gatt.Init();

            _eventIds = new ArrayList();

            _eventProcessor.GattReadPermitRequestEvent += GattReadPermitRequestEvent;
        }

        private void GattReadPermitRequestEvent(object sender, GattReadPermitRequestEventArgs e)
        {
            var eventId = new EventId
            {
                Id = _nextEventId++,
                Args = e,
            };
        }

        /// <inheritdoc />
        public void AddCharacteristic(GattLocalCharacteristic characteristic)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemoveCharacteristic(GattLocalCharacteristic characteristic)
        {
        }

        /// <inheritdoc />
        public bool StartAdvertising(bool isConnectable, bool isDiscoverable, Buffer serviceData, byte[] deviceName, ArrayList services)
        {
            if (deviceName is null)
                throw new ArgumentNullException(nameof(deviceName));

            _isDiscoverable = isDiscoverable;
            _isConnectable = isConnectable;
            _deviceName = deviceName;
            ushort serviceHandle = 0, deviceNameCharacteristicHandle = 0, appearanceCharacteristicHandle = 0;

            Gap.Init(
                Role.Peripheral,
                false,
                0x07,
                ref serviceHandle,
                ref deviceNameCharacteristicHandle,
                ref appearanceCharacteristicHandle
            );

            Gatt.UpdateCharacteristicValue(
                serviceHandle,
                deviceNameCharacteristicHandle,
                0,
                BitConverter.GetBytes(_deviceName.Length)[3],
                _deviceName
            );

            Gap.SetIoCapability(IoCapability.DisplayOnly);

            Gap.SetAuthenticationRequirement(
                true,
                true,
                SecureConnectionSupport.Supported,
                false,
                7,
                16,
                false,
                123456,
                AddressType.PublicDeviceAddress
            );
            _gattServices = new GattLocalService[services.Count];

            for (var i = 0; i < services.Count; i++)
                _gattServices[i] = (GattLocalService)services[i];

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

                Gatt.AddService(
                    UuidType.Uuid128,
                    uuid,
                    ServiceType.Primary,
                    maxAttributeRecords,
                    ref _serviceHandles[i]
                );

                _characteristicHandles[i] = new ushort[service.Characteristics.Length];

                for (var j = 0; j < service.Characteristics.Length; j++)
                {
                    var characteristic = service.Characteristics[j];
                    uuid = characteristic.Uuid.ToByteArray();
                    var characteristicValueLength = (ushort)characteristic.StaticValue.Length;
                    var characteristicProperties =
                        (CharacteristicProperties)((uint)characteristic.CharacteristicProperties & 0x11111111);

                    Gatt.AddCharacteristic(
                        _serviceHandles[i],
                        UuidType.Uuid128,
                        uuid,
                        characteristicValueLength,
                        characteristicProperties,
                        SecurityPermissions.None,
                        Gatt.EventMask.NotifyReadRequestAndWaitForApprovalResponse |
                        Gatt.EventMask.NotifyWriteRequestAndWaitForApprovalResponse,
                        16,
                        false,
                        ref _characteristicHandles[i][j]
                    );
                }
            }

            Hci.LeSetScanResponseData(0, null);

            Gap.SetDiscoverable(
                AdvertisingType.ConnectableUndirected,
                100,
                100,
                AddressType.PublicDeviceAddress,
                FilterPolicy.ScanAnyRequestAny,
                (byte)_deviceName.Length,
                _deviceName,
                0,
                null,
                0,
                0
            );

            return true;
        }

        /// <inheritdoc />
        public void StopAdvertising()
        {
            Reset();
        }

        /// <inheritdoc />
        public int NotifyClient(ushort connection, ushort characteristicId, byte[] notifyBuffer)
        {
            if (notifyBuffer is null)
                throw new ArgumentNullException(nameof(notifyBuffer));

            ushort value = 0;

            for (var i = 0; i < _serviceHandles.Length; i++)
            {
                var serviceHandle = _serviceHandles[i];
                var characteristicHandles = _characteristicHandles[i];

                for (var j = 0; j < characteristicHandles.Length; j++)
                {
                    var characteristicHandle = characteristicHandles[j];

                    //FIXME: Change this to be the actual characteristicId check
                    if (characteristicId == value)
                        return (int)Gatt.UpdateCharacteristicValue(
                            serviceHandle,
                            characteristicHandle,
                            0,
                            (byte)notifyBuffer.Length,
                            notifyBuffer
                        );

                    value++;
                }
            }

            return 1;
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

        /// <inheritdoc />
        public void AddService(Guid serviceUuid)
        {
            ushort serviceHandle = 0;
            Gatt.AddService(UuidType.Uuid128, serviceUuid.ToByteArray(), ServiceType.Primary, byte.MaxValue, ref serviceHandle);
        }

        /// <inheritdoc />
        public void RemoveService(Guid serviceUuid)
        {
            throw new NotImplementedException();
        }

        private ushort GenerateEventId()
        {
            var id = _nextEventId++;
            return id;
        }

        /// <summary>
        ///     Resets BlueNrg2
        /// </summary>
        public void Reset()
        {
            Hci.Reset();
        }

        internal void StartBluetoothThread()
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

        private struct ServiceId
        {
            public Guid ServiceUuid;
            public ushort Handle;
        }

        private struct EventId
        {
            public ushort Id;
            public EventArgs Args;
        }

        private struct ReadRequestArgs
        {
            public ushort Handle;
            public ushort AttributeHandle;
            public ushort Offset;
        }

        private struct WriteRequestArgs
        {
            public ushort Handle;
            public ushort AttributeHandle;
            public byte Lenght;
            public byte[] Data;
        }
    }
}
