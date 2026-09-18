// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.Aw8737
{
    /// <summary>
    /// Driver for the Awinic AW8737A mono Class-K audio power amplifier (analog input, speaker output).
    /// </summary>
    /// <remarks>
    /// The AW8737A is controlled through a single active-low <c>SHDN</c> pin. Driving <c>SHDN</c> high (a
    /// rising edge from the shutdown state) enables the amplifier in Mode 1 - the highest NCN speaker-guard
    /// power level (1.2 W into an 8 ohm load); driving it low shuts the amplifier down. The voltage gain is
    /// fixed by the external input resistor (for example 16.3 V/V with a 3 kohm resistor) and is not set by
    /// software. The audio signal is an analog input (for example a codec/DAC line-out such as an ES8311)
    /// and is not handled by this binding.
    /// <para>
    /// This binding uses a plain GPIO pin and only enables or disables the amplifier (Mode 1). The one-wire
    /// protocol can also select Mode 1 to Mode 4 using microsecond pulses; that needs a hardware timer such
    /// as the ESP32 RMT peripheral to generate the 0.75-10 us edges and is not provided by this binding.
    /// </para>
    /// </remarks>
    public class Aw8737 : IDisposable
    {
        private readonly bool _shouldDispose;
        private readonly int _controlPinNumber;
        private GpioController _gpioController;
        private GpioPin _controlPin;
        private bool _enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aw8737" /> class and leaves the amplifier shut down.
        /// </summary>
        /// <param name="controlPin">The GPIO pin number connected to the AW8737A <c>SHDN</c> control pin.</param>
        /// <param name="gpioController">The <see cref="GpioController" /> used to drive the control pin, or <see langword="null" /> to create a new one.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="gpioController"/> when this instance is disposed; otherwise, <see langword="false" />. Always <see langword="true" /> when the controller is created internally.</param>
        public Aw8737(int controlPin, GpioController gpioController = null, bool shouldDispose = true)
        {
            _shouldDispose = shouldDispose || gpioController is null;
            _gpioController = gpioController ?? new GpioController();
            _controlPinNumber = controlPin;
            _controlPin = _gpioController.OpenPin(controlPin, PinMode.Output);
            _controlPin.Write(PinValue.Low);
            _enabled = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the amplifier is enabled.
        /// </summary>
        /// <value><see langword="true" /> when <c>SHDN</c> is driven high (amplifier enabled in Mode 1); otherwise, <see langword="false" /> (shut down).</value>
        /// <remarks>
        /// The value is tracked internally rather than read back from the pin: the control pin is an output,
        /// and reading an ESP32 output pin returns its (disabled) input buffer, which always reads low.
        /// </remarks>
        public bool Enabled
        {
            get => _enabled;

            set
            {
                _controlPin.Write(value ? PinValue.High : PinValue.Low);
                _enabled = value;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_gpioController != null)
            {
                _gpioController.ClosePin(_controlPinNumber);

                if (_shouldDispose)
                {
                    _gpioController.Dispose();
                }

                _gpioController = null;
                _controlPin = null;
            }
        }
    }
}
