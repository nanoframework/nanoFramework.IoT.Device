// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents the Explorer HAT DCMotors collection
    /// </summary>
    public class Motors : IDisposable
    {
        private const int MOTOR1_SPDPIN = 19;
        private const int MOTOR1_DIRPIN = 20;
        private const int MOTOR2_SPDPIN = 21;
        private const int MOTOR2_DIRPIN = 26;

        private GpioController _controller;

        private List<DCMotor.DCMotor> _motorArray;

        /// <summary>
        /// Motor #1
        /// </summary>
        /// <value></value>
        public DCMotor.DCMotor One { get => _motorArray[0]; }

        /// <summary>
        /// Motor #2
        /// </summary>
        /// <value></value>
        public DCMotor.DCMotor Two { get => _motorArray[1]; }

        /// <summary>
        /// Both motors turns forwards at indicated speed
        /// </summary>
        /// <param name="speed">Indicated speed</param>
        public void Forwards(double speed = 1)
        {
            _motorArray[0].Forwards(speed);
            _motorArray[1].Forwards(speed);
        }

        /// <summary>
        /// Both motors turns backwards at indicated speed
        /// </summary>
        /// <param name="speed">Indicated speed</param>
        public void Backwards(double speed = 1)
        {
            _motorArray[0].Backwards(speed);
            _motorArray[1].Backwards(speed);
        }

        /// <summary>
        /// Both motors stops
        /// </summary>
        public void Stop()
        {
            _motorArray[0].Stop();
            _motorArray[1].Stop();
        }

        /// <summary>
        /// Initializes a <see cref="Motors"/> instance
        /// </summary>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Motors"/> to manage GPIO resources</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        internal Motors(GpioController? controller = null, bool shouldDispose = true)
        {
            _controller = controller ?? new();
            _shouldDispose = shouldDispose || controller is null;

            _motorArray = new List<DCMotor.DCMotor>()
            {
                DCMotor.DCMotor.Create(MOTOR1_SPDPIN, MOTOR1_DIRPIN, _controller),
                DCMotor.DCMotor.Create(MOTOR2_SPDPIN, MOTOR2_DIRPIN, _controller)
            };
        }

        private bool _shouldDispose;

        /// <summary>
        /// Disposes the <see cref="Motors"/> instance
        /// </summary>
        public void Dispose()
        {
            _motorArray[0]?.Dispose();
            _motorArray[1]?.Dispose();
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
        }
    }
}
