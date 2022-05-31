using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using BlueNrg2.Aci;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;

namespace BlueNrg2
{
    public class BlueNrg2 : IDisposable
    {
       private readonly IHardwareInterface _hardwareInterface;
#if DEBUG
		private readonly ILogger _logger;
#endif
		private readonly bool _shouldDispose;
		private readonly TransportLayer _transportLayer;
		public readonly Gap Gap;
		public readonly Gatt Gatt;
		public readonly Hal Hal;
		public readonly Hci Hci;
		public readonly L2Cap L2Cap;
		private bool _isRunning;
		private bool _running;

		public BlueNrg2(
			SpiConnectionSettings spiConnectionSettings,
			int extIPin,
			int resetPin,
			UserEventCallback callback,
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
			_transportLayer = new TransportLayer(callback, _hardwareInterface);
			Hci = new Hci(_transportLayer);
			Gap = new Gap(_transportLayer);
			Gatt = new Gatt(_transportLayer);
			Hal = new Hal(_transportLayer);
			L2Cap = new L2Cap(_transportLayer);

#if DEBUG
			_logger.LogInformation("BlueNRG-2 Application");
#endif

			Hci.Reset();

			Thread.Sleep(2000);

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

		public void StartBluetoothThread()
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
    }
}
