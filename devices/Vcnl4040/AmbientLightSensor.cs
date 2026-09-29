// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;
using UnitsNet;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Represents the ambient light sensor component of the VCNL4040 device.
    /// </summary>
    public class AmbientLightSensor
    {
        private readonly AlsConfRegister _alsConfRegister;
        private readonly AlsHighInterruptThresholdRegister _alsHighInterruptThresholdRegister;
        private readonly AlsLowInterruptThresholdRegister _alsLowInterruptThresholdRegister;
        private readonly AlsDataRegister _alsDataRegister;
        private readonly PsMsRegister _psMsRegister;
        private AlsIntegrationTime _localIntegrationTime = AlsIntegrationTime.Time80ms;
        private bool _loadReductionModeEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientLightSensor"/> class.
        /// </summary>
        /// <param name="device">The device value.</param>
        internal AmbientLightSensor(I2cDevice device)
        {
            _alsConfRegister = new AlsConfRegister(device);
            _alsHighInterruptThresholdRegister = new AlsHighInterruptThresholdRegister(device);
            _alsLowInterruptThresholdRegister = new AlsLowInterruptThresholdRegister(device);
            _alsDataRegister = new AlsDataRegister(device);
            _psMsRegister = new PsMsRegister(device);
        }

        #region General

        /// <summary>
        /// Gets or sets a value indicating whether the ambient light sensor is powered on.
        /// The default setting is that the sensor is turned off.
        /// The sensor can be configured while in the off state.
        /// To query measurements, the sensor must be activated.
        /// The time until the first value is available depends on the set integration time.
        /// In the off state, the measurement value is not updated.
        /// The sensor can be turned on and off at any time.
        /// The configuration remains unaffected by this.
        /// </summary>
        public bool PowerOn
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsSd == PowerState.PowerOn;
            }

            set
            {
                _alsConfRegister.AlsSd = value ? PowerState.PowerOn : PowerState.PowerOff;
                _alsConfRegister.Write();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether load reduction mode is enabled.
        /// If the Load Reduction Mode is enabled, the binding uses a local copy of the last set
        /// value for the integration time to calculate the measurement value. Otherwise, the
        /// current integration time is read from the device each time a measurement is queried.
        /// This causes additional load on the I2C bus, which may be relevant at a high query frequency.
        /// In Load Reduction Mode, this query is eliminated. However, if the integration time setting
        /// is changed without using the corresponding property of the binding, it may lead to inconsistency
        /// resulting in incorrect measurement value calculation.
        /// Therefore, it is crucial that any changes are made exclusively through the binding.
        /// The Load Reduction Mode should only be used when the bus load is relevant.
        /// The default state is that it is turned off.
        /// </summary>
        public bool LoadReductionModeEnabled
        {
            get => _loadReductionModeEnabled;

            set
            {
                // retrieve current integration time from the device for a last time
                if (value)
                {
                    _alsConfRegister.Read();
                    _localIntegrationTime = _alsConfRegister.AlsIt;
                }

                _loadReductionModeEnabled = value;
            }
        }

        #endregion

        #region Measurement

        /// <summary>
        /// Gets the current ambient light sensor reading.
        /// The device internal count is converted into a measurement value in Lux
        /// using the configured resolution.
        /// Note: the resolution is an indirect parameter derived from the integration time, as described in the datasheet.
        /// Note: the documentation for the Load Reduction Mode should be considered.
        /// </summary>
        public Illuminance Illuminance
        {
            get
            {
                _alsDataRegister.Read();
                Illuminance resolution;
                if (_loadReductionModeEnabled)
                {
                    GetDetectionRangeAndResolution(_localIntegrationTime, out _, out resolution);
                }
                else
                {
                    GetDetectionRangeAndResolution(IntegrationTime, out _, out resolution);
                }

                return Illuminance.FromLux(_alsDataRegister.Data * resolution.Lux);
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Gets or sets the ambient light sensor integration time.
        /// Note: Changing the integration time implicitly results in an adjustment of the resolution and range.
        ///       Range and resolution are parameters that indirectly depend on the integration time.
        ///       The specific relationship is specified in the datasheet.
        /// Important: when setting the integration time, possibly configured interrupts are implicitly deactivated.
        ///            This is done to prevent accidentally using invalid threshold levels.
        ///            Interrupts must be explicitly configured and re-enabled.
        /// </summary>
        public AlsIntegrationTime IntegrationTime
        {
            get
            {
                _alsConfRegister.Read();

                // Since we are already reading the current integration time from the chip,
                // we can also update the internally stored value. While not strictly necessary,
                // this could potentially resolve any existing inconsistency, ideally.
                _localIntegrationTime = _alsConfRegister.AlsIt;

                return _alsConfRegister.AlsIt;
            }

            set
            {
                DisableInterrupts();

                _alsConfRegister.AlsIt = value;
                _alsConfRegister.Write();
                _localIntegrationTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the detection range.
        /// Note: The range is a parameter that depends on the integration time.
        ///       Changing the range, therefore, implicitly adjusts the integration time and
        ///       consequently deactivates any set interrupts.
        ///       Refer to the description of the <see cref="IntegrationTime"/> property for more information.
        /// </summary>
        public AlsRange Range
        {
            get
            {
                // get range derived from corresponding integration time
                switch (IntegrationTime)
                {
                    case AlsIntegrationTime.Time80ms:
                        return AlsRange.Range6553;
                    case AlsIntegrationTime.Time160ms:
                        return AlsRange.Range3276;
                    case AlsIntegrationTime.Time320ms:
                        return AlsRange.Range1638;
                    case AlsIntegrationTime.Time640ms:
                        return AlsRange.Range819;
                    default:
                        throw new NotImplementedException();
                }
            }

            set
            {
                // set the range by setting the corresponding integration time
                switch (value)
                {
                    case AlsRange.Range6553:
                        IntegrationTime = AlsIntegrationTime.Time80ms;
                        break;
                    case AlsRange.Range3276:
                        IntegrationTime = AlsIntegrationTime.Time160ms;
                        break;
                    case AlsRange.Range1638:
                        IntegrationTime = AlsIntegrationTime.Time320ms;
                        break;
                    case AlsRange.Range819:
                        IntegrationTime = AlsIntegrationTime.Time640ms;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Gets the range as illuminance value.
        /// </summary>
        public Illuminance RangeAsIlluminance
        {
            get
            {
                GetDetectionRangeAndResolution(IntegrationTime, out Illuminance range, out _);
                return range;
            }
        }

        /// <summary>
        /// Gets or sets the resolution.
        /// Note: The resolution is a parameter that depends on the integration time.
        ///       Changing the resolution, therefore, implicitly adjusts the integration time and
        ///       consequently deactivates any set interrupts.
        ///       Refer to the description of the <see cref="IntegrationTime"/> property for more information.
        /// </summary>
        public AlsResolution Resolution
        {
            get
            {
                // get resolution derived from corresponding integration time
                switch (IntegrationTime)
                {
                    case AlsIntegrationTime.Time80ms:
                        return AlsResolution.Resolution_0_1;
                    case AlsIntegrationTime.Time160ms:
                        return AlsResolution.Resolution_0_05;
                    case AlsIntegrationTime.Time320ms:
                        return AlsResolution.Resolution_0_025;
                    case AlsIntegrationTime.Time640ms:
                        return AlsResolution.Resolution_0_0125;
                    default:
                        throw new NotImplementedException();
                }
            }

            set
            {
                // set the range by setting the corresponding integration time
                switch (value)
                {
                    case AlsResolution.Resolution_0_1:
                        IntegrationTime = AlsIntegrationTime.Time80ms;
                        break;
                    case AlsResolution.Resolution_0_05:
                        IntegrationTime = AlsIntegrationTime.Time160ms;
                        break;
                    case AlsResolution.Resolution_0_025:
                        IntegrationTime = AlsIntegrationTime.Time320ms;
                        break;
                    case AlsResolution.Resolution_0_0125:
                        IntegrationTime = AlsIntegrationTime.Time640ms;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Gets the resolution as illuminance value.
        /// </summary>
        public Illuminance ResolutionAsIlluminance
        {
            get
            {
                GetDetectionRangeAndResolution(IntegrationTime, out _, out Illuminance resolution);
                return resolution;
            }
        }
        #endregion

        #region Interrupt

        /// <summary>
        /// Gets a value indicating whether interrupt function (INT-pin function) is enabled.
        /// </summary>
        public bool IsInterruptEnabled
        {
            get
            {
                _alsConfRegister.Read();
                return _alsConfRegister.AlsIntEn == AlsInterrupt.Enabled;
            }
        }

        /// <summary>
        /// Disables the interrupts (INT-pin function).
        /// </summary>
        public void DisableInterrupts()
        {
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Configures the interrupt parameters and enables the interrupt (INT-pin function).
        /// Refer to <see cref="AmbientLightInterruptConfiguration"/> for more information on the parameters.
        /// Note: even in Load Reduction Mode the actual integration time setting from the device is
        ///       used. This would implicitly update the local copy if an inconsistency should prevail.
        ///       Refer to <see cref="LoadReductionModeEnabled"/> for further information.
        /// </summary>
        /// <param name="configuration">The configuration value.</param>
        /// <exception cref="ArgumentException">Thrown if any threshold exceeds the limit defined by the range (integration time),
        /// or lower threshold is higher than the upper one, or a threshold is negative.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the proximity logic output of the proximity sensor is enabled.
        /// Refer to <see cref="ProximityInterruptConfiguration"/> for more information.</exception>
        public void EnableInterrupts(AmbientLightInterruptConfiguration configuration)
        {
            // the maximum detection range and resolution depends on the integration time setting
            _alsConfRegister.Read();
            GetDetectionRangeAndResolution(_alsConfRegister.AlsIt, out Illuminance maxDetectionRange, out Illuminance resolution);

            if (configuration.LowerThreshold.Lux < 0 || configuration.UpperThreshold.Lux < 0)
            {
                throw new ArgumentException();
            }

            if (configuration.LowerThreshold.Lux > maxDetectionRange.Lux || configuration.UpperThreshold.Lux > maxDetectionRange.Lux)
            {
                throw new ArgumentException();
            }

            if (configuration.LowerThreshold.Lux > configuration.UpperThreshold.Lux)
            {
                throw new ArgumentException();
            }

            _psMsRegister.Read();
            if (_psMsRegister.PsMs == PsProximityDetectionOutput.LogicOutput)
            {
                throw new InvalidOperationException();
            }

            // disable interrupts before altering configuration to avoid transient side effects
            _alsConfRegister.AlsIntEn = AlsInterrupt.Disabled;
            _alsConfRegister.Write();

            // set threshold levels by calculating the register value in counts based on the current resolution.
            _alsLowInterruptThresholdRegister.Level = (ushort)(configuration.LowerThreshold.Lux / resolution.Lux);
            _alsHighInterruptThresholdRegister.Level = (ushort)(configuration.UpperThreshold.Lux / resolution.Lux);
            _alsLowInterruptThresholdRegister.Write();
            _alsHighInterruptThresholdRegister.Write();

            // set persistence and enable interrupts
            _alsConfRegister.AlsPers = configuration.Persistence;
            _alsConfRegister.AlsIntEn = AlsInterrupt.Enabled;
            _alsConfRegister.Write();
        }

        /// <summary>
        /// Gets the current interrupt configuration from the device.
        /// Note: even in Load Reduction Mode the actual integration time setting from the device is
        ///       used. This would implicitly update the local copy if an inconsistency should prevail.
        ///       Refer to <see cref="LoadReductionModeEnabled"/> for further information.
        /// </summary>
        /// <returns>The ambient light interrupt configuration.</returns>
        public AmbientLightInterruptConfiguration GetInterruptConfiguration()
        {
            _alsLowInterruptThresholdRegister.Read();
            _alsHighInterruptThresholdRegister.Read();
            _alsConfRegister.Read();

            GetDetectionRangeAndResolution(_alsConfRegister.AlsIt, out _, out Illuminance resolution);

            return new AmbientLightInterruptConfiguration(
                Illuminance.FromLux(_alsLowInterruptThresholdRegister.Level * resolution.Lux),
                Illuminance.FromLux(_alsHighInterruptThresholdRegister.Level * resolution.Lux),
                _alsConfRegister.AlsPers);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Helper method to get detection range and resolution for the given integration time.
        /// </summary>
        /// <param name="integrationTime">The integration time.</param>
        /// <param name="range">The resulting detection range.</param>
        /// <param name="resolution">The resulting resolution.</param>
        private static void GetDetectionRangeAndResolution(
            AlsIntegrationTime integrationTime,
            out Illuminance range,
            out Illuminance resolution)
        {
            switch (integrationTime)
            {
                case AlsIntegrationTime.Time80ms:
                    range = Illuminance.FromLux(6553.5);
                    resolution = Illuminance.FromLux(0.1);
                    break;
                case AlsIntegrationTime.Time160ms:
                    range = Illuminance.FromLux(3276.8);
                    resolution = Illuminance.FromLux(0.05);
                    break;
                case AlsIntegrationTime.Time320ms:
                    range = Illuminance.FromLux(1638.4);
                    resolution = Illuminance.FromLux(0.025);
                    break;
                case AlsIntegrationTime.Time640ms:
                    range = Illuminance.FromLux(819.2);
                    resolution = Illuminance.FromLux(0.0125);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion
    }
}
