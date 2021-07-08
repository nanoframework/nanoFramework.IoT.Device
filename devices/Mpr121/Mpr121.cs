// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Mpr121
{
    /// <summary>
    /// Supports MPR121 Proximity Capacitive Touch Sensor Controller.
    /// </summary>
    public class Mpr121 : IDisposable
    {
        /// <summary>
        /// MPR121 Default I2C Address.
        /// </summary>
        public static readonly byte DefaultI2cAddress = 0x5A;

        private static readonly int CHANNELS_NUMBER = 12;// Enum.GetValues(typeof(Channels)).Length;

        private I2cDevice _i2cDevice;
        private Timer _timer;

        private bool[] _statuses;

        private int _periodRefresh;

        /// <summary>
        /// Notifies about a the channel statuses have been changed.
        /// Refresh period can be changed by setting PeriodRefresh property.
        /// </summary>
        public event ChannelStatusesChanged ChannelStatusesChanged;

        /// <summary>
        /// Gets or sets the period in milliseconds to refresh the channels statuses.
        /// </summary>
        /// <remark>
        /// Set value 0 to stop the automatically refreshing. Setting the value greater than 0 will start/update auto-refresh.
        /// </remark>
        public int PeriodRefresh
        {
            get => _periodRefresh;
            set
            {
                _periodRefresh = value;

                if (_periodRefresh > 0)
                {
                    _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
                }
                else
                {
                    // Disable the auto-refresh.
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Initialize a MPR121 controller.
        /// </summary>
        /// <param name="i2cDevice">The i2c device.</param>
        /// <param name="periodRefresh">The period in milliseconds of refresing the channel statuses.</param>
        /// <param name="configuration">The controller configuration.</param>
        public Mpr121(I2cDevice i2cDevice, int periodRefresh = -1, Mpr121Configuration? configuration = null)
        {
            configuration = configuration ?? GetDefaultConfiguration();

            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _timer = new Timer(RefreshChannelStatuses, this, Timeout.Infinite, Timeout.Infinite);

            _statuses = new bool[CHANNELS_NUMBER]; // new Dictionary<Channels, bool>();
            for (int i = 0; i < CHANNELS_NUMBER; i++)
            {
                _statuses[i] = false;
            }

            InitializeController(configuration);

            PeriodRefresh = periodRefresh;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
            _timer?.Dispose();
            _timer = null!;
        }

        /// <summary>
        /// Reads the channel statuses of MPR121 controller.
        /// </summary>
        public bool[] ReadChannelStatuses()
        {
            RefreshChannelStatuses();
            bool[] statuses = new bool[CHANNELS_NUMBER];
            Array.Copy(_statuses, statuses, CHANNELS_NUMBER);
            return statuses;
        }

        /// <summary>
        /// Reads the channel status of MPR121 controller.
        /// </summary>
        /// <param name="channel">The channel to read status.</param>
        /// <remark>
        /// Please use ReadChannelStatuses() if you need to read statuses of multiple channels.
        /// Using this method several times to read status for several channels can affect the performance.
        /// </remark>
        public bool ReadChannelStatus(Channels channel)
        {
            RefreshChannelStatuses();

            return _statuses[(int)channel];
        }

        private static Mpr121Configuration GetDefaultConfiguration()
        {
            return new Mpr121Configuration()
            {
                MaxHalfDeltaRising = 0x01,
                NoiseHalfDeltaRising = 0x01,
                NoiseCountLimitRising = 0x00,
                FilterDelayCountLimitRising = 0x00,
                MaxHalfDeltaFalling = 0x01,
                NoiseHalfDeltaFalling = 0x01,
                NoiseCountLimitFalling = 0xFF,
                FilterDelayCountLimitFalling = 0x01,
                ElectrodeTouchThreshold = 0x0F,
                ElectrodeReleaseThreshold = 0x0A,
                ChargeDischargeTimeConfiguration = 0x04,
                ElectrodeConfiguration = 0x0C
            };
        }

        private void InitializeController(Mpr121Configuration configuration)
        {
            SetRegister(Registers.MHDR, configuration.MaxHalfDeltaRising);
            SetRegister(Registers.NHDR, configuration.NoiseHalfDeltaRising);
            SetRegister(Registers.NCLR, configuration.NoiseCountLimitRising);
            SetRegister(Registers.FDLR, configuration.FilterDelayCountLimitRising);
            SetRegister(Registers.MHDF, configuration.MaxHalfDeltaFalling);
            SetRegister(Registers.NHDF, configuration.NoiseHalfDeltaFalling);
            SetRegister(Registers.NCLF, configuration.NoiseCountLimitFalling);
            SetRegister(Registers.FDLF, configuration.FilterDelayCountLimitFalling);
            SetRegister(Registers.E0TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E0RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E1TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E1RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E2TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E2RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E3TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E3RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E4TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E4RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E5TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E5RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E6TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E6RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E7TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E7RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E8TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E8RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E9TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E9RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E10TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E10RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E11TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E11RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.CDTC, configuration.ChargeDischargeTimeConfiguration);
            SetRegister(Registers.ELECONF, configuration.ElectrodeConfiguration);
        }

        /// <summary>
        /// The callback function for timer to refresh channels statuses.
        /// </summary>
        private void RefreshChannelStatuses(object? state) => RefreshChannelStatuses();

        /// <summary>
        /// Refresh the channel statuses.
        /// </summary>
        private void RefreshChannelStatuses()
        {
            // Pause the auto-refresh to prevent possible collisions.
            var periodRefresh = PeriodRefresh;
            PeriodRefresh = 0;

            SpanByte buffer = new byte[2];
            _i2cDevice.Read(buffer);

            short rawStatus = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            bool isStatusChanged = false;
            for (var i = 0; i < CHANNELS_NUMBER; i++)
            {
                bool status = ((1 << i) & rawStatus) > 0;
                if (_statuses[i] != status)
                {
                    _statuses[i] = status;
                    isStatusChanged = true;
                }
            }

            if (isStatusChanged)
            {
                OnChannelStatusesChanged();
            }

            // Resume the auto-refresh.
            PeriodRefresh = periodRefresh;
        }

        private void SetRegister(Registers register, byte value)
        {
            SpanByte data = new byte[]
            {
                (byte)register, value
            };
            _i2cDevice.Write(data);
        }

        private void OnChannelStatusesChanged()
        {
            bool[] statuses = new bool[CHANNELS_NUMBER];
            Array.Copy(_statuses, statuses, CHANNELS_NUMBER);
            ChannelStatusesChanged?.Invoke(this, new ChannelStatusesChangedEventArgs(statuses));
        }
    }
}
