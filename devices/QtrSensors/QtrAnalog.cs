// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Adc;
using System.Device.Gpio;

namespace Iot.Device.QtrSensors
{
    /// <summary>
    /// QTR Pololu Reflectance Analog Sensor.
    /// </summary>
    public class QtrAnalog : QtrBase, IDisposable
    {
        /// <summary>
        /// The default noise threshold is set to 5%.
        /// </summary>
        public const double DefaultNoiseThreshold = 0.05;

        private readonly AdcChannel[] _qtrSensors;
        private readonly GpioPin[] _emiters;
        private GpioController _gpio;
        private AdcController _adc;
        private bool _shouldDisposeGpio;
        private EmitterSelection _emitterSelection;
        private PinValue _emitterValue;
        private CalibrationData[] _calibOn;
        private CalibrationData[] _calibOff;
        private bool _isDisposed = false;
        private double _lastPosition;

        /// <summary>
        /// Gets or sets the noise threshold.
        /// </summary>
        public double NoiseThreshold { get; set; } = DefaultNoiseThreshold;

        /// <summary>
        /// Gets or sets the Emitter selection.
        /// </summary>
        public override EmitterSelection EmitterSelection
        {
            get => _emitterSelection;
            set
            {
                _emitterSelection = value;
                if (_emiters.Length == 0)
                {
                    // No pin, we don't have anything to do here
                    return;
                }
                else if (_emiters.Length == 1)
                {
                    switch (_emitterSelection)
                    {
                        case EmitterSelection.None:
                            // If none, then switch of regardless of the pin values
                            _emiters[0].Write(PinValue.Low);
                            break;
                        case EmitterSelection.Even:
                        case EmitterSelection.Odd:
                        case EmitterSelection.All:
                            // Sets to the pin value
                            _emiters[0].Write(_emitterValue);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (_emitterSelection)
                    {
                        case EmitterSelection.None:
                            // If none, then switch of regardless of the pin values
                            _emiters[0].Write(PinValue.Low);
                            _emiters[1].Write(PinValue.Low);
                            break;
                        case EmitterSelection.Even:
                            // If none, then switch of regardless of the pin values
                            _emiters[0].Write(_emitterValue);
                            break;
                        case EmitterSelection.Odd:
                            _emiters[1].Write(_emitterValue);
                            break;
                        case EmitterSelection.All:
                            _emiters[0].Write(_emitterValue);
                            _emiters[1].Write(_emitterValue);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the EmiterValue.
        /// </summary>
        public override PinValue EmitterValue
        {
            get => _emitterValue;
            set
            {
                _emitterValue = value;

                // Force the refresh of the emmiter pins
                EmitterSelection = _emitterSelection;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QtrAnalog"/> class.
        /// </summary>
        /// <param name="sensors">Array of Adc channels.</param>
        /// <param name="emmiters">Gpio emmiter pins. Array of 0 = no emmiter managed here, 1 = common emmiter, 2 = one for even one for odds.</param>
        /// <param name="gpioController">A GPIO controller.</param>
        /// <param name="adcController">An ADC controller.</param>
        /// <param name="shouldDisposeo">Tru to dispose GPIO controller.</param>
        public QtrAnalog(int[] sensors, int[] emmiters, GpioController gpioController = null, AdcController adcController = null, bool shouldDisposeo = false)
        {
            if (emmiters.Length > 2)
            {
                throw new ArgumentException(nameof(emmiters));
            }

            _shouldDisposeGpio = gpioController == null || shouldDisposeo;
            _gpio = gpioController ?? new ();
            _adc = adcController ?? new ();
            _qtrSensors = new AdcChannel[sensors.Length];
            for (int i = 0; i < _qtrSensors.Length; i++)
            {
                _qtrSensors[i] = _adc.OpenChannel(sensors[i]);
            }

            _emiters = new GpioPin[emmiters.Length];
            for (int i = 0; i < _emiters.Length; i++)
            {
                _emiters[i] = _gpio.OpenPin(emmiters[i], PinMode.Output);
                _emiters[i].Write(PinValue.Low);
            }

            _emitterValue = PinValue.Low;
        }

        /// <summary>
        /// Calibrates the sensors.
        /// </summary>
        /// <param name="iteration">Number of iterations.</param>
        /// <param name="emitterOn">True to set the emitters on.</param>
        /// <returns>Calibration data.</returns>
        public override CalibrationData[] Calibrate(int iteration, bool emitterOn)
        {
            PinValue oldEmit = _emitterValue;
            EmitterSelection oldSection = EmitterSelection;

            if (emitterOn)
            {
                _calibOn = _calibOn ?? new CalibrationData[_qtrSensors.Length];
                _emitterValue = PinValue.High;
                EmitterSelection = EmitterSelection.All;
                for (int i = 0; i < _calibOn.Length; i++)
                {
                    _calibOn[i] = new CalibrationData()
                    {
                        MaximumValue = _adc.MinValue,
                        MinimumValue = _adc.MaxValue
                    };
                }
            }
            else
            {
                _calibOff = _calibOff ?? new CalibrationData[_qtrSensors.Length];
                _emitterValue = PinValue.Low;
                EmitterSelection = EmitterSelection.All;
                for (int i = 0; i < _calibOff.Length; i++)
                {
                    _calibOff[i] = new CalibrationData()
                    {
                        MaximumValue = _adc.MinValue,
                        MinimumValue = _adc.MaxValue
                    };
                }
            }

            int val;
            for (int i = 0; i < iteration; i++)
            {
                for (int qtr = 0; qtr < _qtrSensors.Length; qtr++)
                {
                    val = _qtrSensors[qtr].ReadValue();
                    if (emitterOn)
                    {
                        _calibOn[qtr].MaximumValue = _calibOn[qtr].MaximumValue < val ? val : _calibOn[qtr].MaximumValue;
                        _calibOn[qtr].MinimumValue = _calibOn[qtr].MinimumValue > val ? val : _calibOn[qtr].MinimumValue;
                    }
                    else
                    {
                        _calibOff[qtr].MaximumValue = _calibOff[qtr].MaximumValue < val ? val : _calibOff[qtr].MaximumValue;
                        _calibOff[qtr].MinimumValue = _calibOff[qtr].MinimumValue > val ? val : _calibOff[qtr].MinimumValue;
                    }
                }
            }

            _emitterValue = oldEmit;
            EmitterSelection = oldSection;
            return emitterOn ? _calibOn : _calibOff;
        }

        /// <summary>
        /// Resets the calibration.
        /// </summary>
        /// <param name="emitterOn">True to reset the emitters on.</param>
        public override void ResetCalibration(bool emitterOn)
        {
            if (emitterOn)
            {
                _calibOn = null;
            }
            else
            {
                _calibOff = null;
            }
        }

        /// <summary>
        /// Returns the values in a ration from 0.0 to 1.0.
        /// </summary>
        /// <returns>An array of raw values.</returns>
        public override double[] ReadRatio()
        {
            var raws = ReadRaw();
            double[] ratios = new double[raws.Length];
            int min;
            int max;
            int deno;
            for (int i = 0; i < ratios.Length; i++)
            {
                if (((_emitterSelection == EmitterSelection.All) && (_emitterValue == PinValue.High)) ||
                   ((_emitterSelection == EmitterSelection.Odd) && (_emitterValue == PinValue.High) && (i % 2 == 1)) ||
                  ((_emitterSelection == EmitterSelection.Even) && (_emitterValue == PinValue.High) && (i % 2 == 0)))
                {
                    if (_calibOn != null)
                    {
                        min = _calibOn[i].MinimumValue;
                        max = _calibOn[i].MaximumValue;
                        deno = max - min;
                        if (deno != 0)
                        {
                            ratios[i] = (raws[i] - min) / deno;
                            ratios[i] = ratios[i] < 0 ? 0 : ratios[i];
                            ratios[i] = ratios[i] > 1 ? 1 : ratios[i];
                        }
                    }
                    else
                    {
                        ratios[i] = raws[i] / _adc.MaxValue;
                    }
                }
                else
                {
                    if (_calibOff != null)
                    {
                        min = _calibOff[i].MinimumValue;
                        max = _calibOff[i].MaximumValue;
                        deno = max - min;
                        if (deno != 0)
                        {
                            ratios[i] = (raws[i] - min) / deno;
                            ratios[i] = ratios[i] < 0 ? 0 : ratios[i];
                            ratios[i] = ratios[i] > 1 ? 1 : ratios[i];
                        }
                    }
                    else
                    {
                        ratios[i] = raws[i] / _adc.MaxValue;
                    }
                }
            }

            return ratios;
        }

        /// <summary>
        /// Reads the values as Raw values.
        /// </summary>
        /// <returns>An array of raw values.</returns>
        public override double[] ReadRaw()
        {
            double[] vals = new double[_qtrSensors.Length];
            for (int i = 1; i <= SamplesPerSensor; i++)
            {
                for (int qtr = 0; qtr < _qtrSensors.Length; qtr++)
                {
                    vals[qtr] = ((vals[qtr] * (i - 1)) + _qtrSensors[qtr].ReadValue()) / i;
                }
            }

            return vals;
        }

        /// <summary>
        /// Reads the position of the line from -1.0 to 1.0, 0 is centered. -1 means full left, +1 means full right, 0 means centered.
        /// By convention, full left is the sensor 0 and full right is the last sensor.
        /// If no position is found, the last position will be returned.
        /// </summary>
        /// <param name="blackLine">True for a black line, false for a white line.</param>
        /// <returns>The position between -1.0 and 1.0.</returns>
        public override double ReadPosition(bool blackLine = true)
        {
            var ratios = ReadRatio();
            double pos = 0;
            double ratio;
            for (int i = 0; i < ratios.Length; i++)
            {
                // Only take what is not noisy
                if (ratios[i] > NoiseThreshold)
                {
                    ratio = blackLine ? ratios[i] : 1 - ratios[i];
                    pos += (i + 1) * ratio;
                }
            }

            // Normalize it
            pos = (pos / ratios.Length) - 1;

            // Check if the last read is still valid as -1 will just give full left and is the default value if no line is detected
            if (pos == -1)
            {
                if (_lastPosition < (1 / (ratios.Length - 1) - 1))
                {
                    _lastPosition = -1;
                }
                else if (_lastPosition > 1 - (1 / (ratios.Length - 1)))
                {
                    _lastPosition = 1;
                }

                return _lastPosition;
            }

            _lastPosition = pos;
            return pos;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if ((_emiters != null) && !_isDisposed)
            {
                for (int i = 0; i < _emiters.Length; i++)
                {
                    _gpio.ClosePin(_emiters[i].PinNumber);
                    _emiters[i] = null;
                }
            }

            if (_shouldDisposeGpio)
            {
                _gpio?.Dispose();
                _gpio = null;
            }

            if ((_qtrSensors != null) && !_isDisposed)
            {
                // No dispose but setting it to null
                for (int i = 0; i < _qtrSensors.Length; i++)
                {
                    _qtrSensors[i] = null;
                }

                _adc = null;
            }

            _isDisposed = true;
        }
    }
}
