// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using BlueNrg2;
using Microsoft.Extensions.Logging;

namespace Iot.Device.BlueNrg2
{
	internal class SpiInterface : IHardwareInterface
	{
#if DEBUG
		private readonly ILogger _logger;
#endif
		private readonly int _pinChipSelect;
		private readonly int _pinExternalInterrupt;
		private readonly int _pinReset;
		private readonly bool _shouldDispose;
		private GpioController _gpioController;
		private SpiDevice _spiDevice;

		internal SpiInterface(
			SpiDevice spiDevice,
			GpioController gpioController,
			int resetPin,
			int extIPin,
			int csPin,
#if DEBUG
			ILogger logger,
#endif
			bool shouldDispose = false
		)
		{
#if DEBUG
			_logger = logger;
			_shouldDispose = shouldDispose;
#endif

			_spiDevice = spiDevice;
			_gpioController = gpioController;
			_pinReset = resetPin;
			_pinExternalInterrupt = extIPin;
			_pinChipSelect = csPin;

			// Setup Reset
			if (_gpioController.IsPinOpen(_pinReset))
				_gpioController.SetPinMode(_pinReset, PinMode.Output);
			else
				_gpioController.OpenPin(_pinReset, PinMode.Output);
			_gpioController.Write(_pinReset, PinValue.Low);

			// Setup External Interrupt
			if (_gpioController.IsPinOpen(_pinExternalInterrupt))
				_gpioController.SetPinMode(_pinExternalInterrupt, PinMode.Input);
			else
				_gpioController.OpenPin(_pinExternalInterrupt, PinMode.Input);
			_gpioController.RegisterCallbackForPinValueChangedEvent(
				_pinExternalInterrupt,
				PinEventTypes.Rising,
				ExternalInterruptTriggered
			);

			// Setup ChipSelect
            if (_gpioController.IsPinOpen(_pinChipSelect))
                _gpioController.SetPinMode(_pinChipSelect, PinMode.Output);
            else
                _gpioController.OpenPin(_pinChipSelect, PinMode.Output);
			_gpioController.Write(_pinChipSelect, PinValue.Low);
        }

		public event NotifyAsync NotifyAsyncEvent;

		public void Dispose()
		{
			if (_shouldDispose)
			{
				_gpioController?.Dispose();
				_gpioController = null;
				_spiDevice.Dispose();
				_spiDevice = null;
			}
			else if (_gpioController is not null)
			{
				if (_pinReset >= 0)
				{
					_gpioController.ClosePin(_pinReset);
				}

				if (_pinChipSelect >= 0)
				{
					_gpioController.ClosePin(_pinChipSelect);
				}

				if (_pinChipSelect >= 0)
				{
					_gpioController.ClosePin(_pinChipSelect);
				}
			}
		}

		public int Reset()
		{
			_gpioController.Write(_pinChipSelect, PinValue.High);
			_gpioController.Write(_pinReset, PinValue.Low);
			Thread.Sleep(5);
			_gpioController.Write(_pinReset, PinValue.High);
			Thread.Sleep(5);
			return 0;
		}

		public int Receive(ref byte[] buffer, ushort size)
		{
			if (buffer is null)
				throw new ArgumentNullException(nameof(buffer));

			byte[] headerMaster =
			{
				0x0b,
				0x00,
				0x00,
				0x00,
				0x00
			};

			var headerSlave = new byte[5];
			_spiDevice.TransferFullDuplex(headerMaster, headerSlave);
			var len = 0;

			var byteCount = (ushort)(headerSlave[4] << 8 | headerSlave[3]);
			buffer = new byte[byteCount];
			if (byteCount > 0)
			{
				if (byteCount > size)
				{
					byteCount = size;
				}

				for (len = 0; len < byteCount; len++)
				{
					buffer[len] = _spiDevice.ReadByte();
				}
			}

			var tickStart = GetTick();
			while (GetTick() - tickStart < 1000)
			{
				if (_gpioController.Read(_pinExternalInterrupt) == PinValue.Low)
					break;
			}

			_gpioController.Write(_pinChipSelect, PinValue.High);
			return len;
		}

		public int Send(byte[] buffer, ushort size)
		{
			byte[] headerMaster =
			{
				0x0a,
				0x00,
				0x00,
				0x00,
				0x00
			};
			var headerSlave = new byte[5];

			var tickStart = GetTick();
			var readCharacteristicBuffer = new byte[255];
			var result = 0;

			do
			{
				var tickStartDataAvailable = GetTick();
				_gpioController.Write(_pinChipSelect, PinValue.Low);

				while (!IsDataAvailable())
				{
					if (DateTime.UtcNow.Ticks - tickStartDataAvailable <= 15)
						continue;
					result = -3;
					break;
				}

				if (result == -3)
				{
					_gpioController.Write(_pinChipSelect, PinValue.High);
					break;
				}

				_spiDevice.TransferFullDuplex(headerMaster, headerSlave);

				var receivedBytes = (ushort)(headerSlave[2] << 8 | headerSlave[1]);

				if (receivedBytes >= size)
				{
					_spiDevice.TransferFullDuplex(buffer, readCharacteristicBuffer);
				}
				else
				{
					result = -2;
				}

				_gpioController.Write(_pinChipSelect, PinValue.High);

				if (GetTick() - tickStart > 15)
				{
					result = -3;
					break;
				}
			} while (result < 0);

			tickStart = GetTick();
			while (GetTick() - tickStart < 1000)
			{
				if (_gpioController.Read(_pinExternalInterrupt) == PinValue.Low)
					break;
			}

			return result;
		}

		public long GetTick()
		{
			return DateTime.UtcNow.Ticks;
		}

		private void ExternalInterruptTriggered(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
		{
#if DEBUG
			_logger.LogInformation("The External Interrupt was triggered");
#endif

			while (IsDataAvailable())
				if (NotifyAsyncEvent is not null)
					if (NotifyAsyncEvent.Invoke())
						return;
		}

		private bool IsDataAvailable()
		{
			return _gpioController.Read(_pinExternalInterrupt) == PinValue.High;
		}
	}
}
