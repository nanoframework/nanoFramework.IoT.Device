using System;
using System.IO.Ports;

namespace Ld2410
{
	public sealed class Ld2410 : IDisposable
	{
		private readonly SerialPort serialPort;

		public Configuration Configuration { get; }

		public bool ConfigurationModeEnabled { get; private set; }

		public bool EngineeringModeEnabled { get; private set; }

		public string FirmwareVersion { get; private set; }

		public Ld2410(string serialPortName, BaudRate baudRate = BaudRate.BaudRate256000)
		{
			this.Configuration = new()
			{
				BaudRate = baudRate
			};

			this.serialPort = new SerialPort(portName: serialPortName,
				baudRate: (int)baudRate,
				stopBits: StopBits.One,
				parity: Parity.None);
		}

		public static BaudRate FindBaudRate(string serialPortName)
		{
			return BaudRate.BaudRate256000;
		}

		public void EnterConfigurationMode()
		{
			this.ConfigurationModeEnabled = true;
		}

		public void ExitConfigurationMode()
		{
			this.ConfigurationModeEnabled = false;
		}

		public void ReadConfigurations()
		{
			this.ThrowIfNotInConfigurationMode();
		}

		public void CommitConfigurations()
		{
			this.ThrowIfNotInConfigurationMode();
		}

		public void SetEngineeringMode(bool enabled)
		{
			this.ThrowIfNotInConfigurationMode();

			this.EngineeringModeEnabled = enabled;
		}

		public void RestoreFactorySettings(bool restartOnCompletion)
		{
			this.ThrowIfNotInConfigurationMode();
		}

		public void Restart()
		{
			this.ThrowIfNotInConfigurationMode();
		}

		private void ReadFirmwareVersion()
		{
			this.ThrowIfNotInConfigurationMode();

			this.FirmwareVersion = "1.02.22062416";
		}

		private void ThrowIfNotInConfigurationMode()
		{
			if (!this.ConfigurationModeEnabled)
			{
				throw new InvalidOperationException();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			serialPort?.Dispose();
		}
	}
}
