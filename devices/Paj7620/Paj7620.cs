// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Threading;

namespace Iot.Device.Paj7620
{
    /// <summary>
    /// PAJ7620/PAJ7620U2 gesture sensor binding.
    /// </summary>
    [Interface("PAJ7620U2 gesture sensor")]
    public sealed class Paj7620 : IDisposable
    {
        /// <summary>
        /// Default I2C address.
        /// </summary>
        public const int DefaultI2CAddress = 0x73;

        /// <summary>
        /// Default debounce interval, in milliseconds, used to filter out rapid consecutive gesture inputs.
        /// </summary>
        public const int DefaultGestureDebounceMilliseconds = 500;

        private const int IoRetries = 3;
        private const int RetryDelayMilliseconds = 2;

        private const byte RegisterBankSelect = 0xEF;
        private const byte RegisterPartIdLow = 0x00;
        private const byte RegisterPartIdHigh = 0x01;
        private const byte RegisterGestureResult0 = 0x43;
        private const byte RegisterGestureResult1 = 0x44;

        private const byte ExpectedPartIdLow = 0x20;
        private const byte ExpectedPartIdHigh = 0x76;

        // Init sequence inspired by Seeed reference: https://github.com/Seeed-Studio/Grove_Gesture/tree/master
        // Datasheet: https://files.seeedstudio.com/wiki/Grove_Gesture_V_1.0/res/PAJ7620U2_DS_v1.5_05012022_Confidential.pdf
        // flat [register, value] pairs for memory efficiency on MCUs.
        private static readonly byte[] InitRegisterPairs = new byte[]
        {
            // Bank 0
            0xEF, 0x00,
            0x41, 0x00, // disable interrupts for first 8 gestures during setup
            0x42, 0x00, // disable wave interrupt during setup
            0x37, 0x07,
            0x38, 0x17,
            0x39, 0x06,
            0x42, 0x01,
            0x46, 0x2D,
            0x47, 0x0F,
            0x48, 0x3C,
            0x49, 0x00,
            0x4A, 0x1E,
            0x4C, 0x22,
            0x51, 0x10,
            0x5E, 0x10,
            0x60, 0x27,
            0x80, 0x42,
            0x81, 0x44,
            0x82, 0x04,
            0x8B, 0x01,
            0x90, 0x06,
            0x95, 0x0A,
            0x96, 0x0C,
            0x97, 0x05,
            0x9A, 0x14,
            0x9C, 0x3F,
            0xA5, 0x19,
            0xCC, 0x19,
            0xCD, 0x0B,
            0xCE, 0x13,
            0xCF, 0x64,
            0xD0, 0x21,

            // Bank 1
            0xEF, 0x01,
            0x02, 0x0F,
            0x03, 0x10,
            0x04, 0x02,
            0x25, 0x01,
            0x27, 0x39,
            0x28, 0x7F,
            0x29, 0x08,
            0x3E, 0xFF,
            0x5E, 0x3D,
            0x65, 0x96, // idle timing, commonly used for normal mode
            0x67, 0x97,
            0x69, 0xCD,
            0x6A, 0x01,
            0x6D, 0x2C,
            0x6E, 0x01,
            0x72, 0x01, // operation enable
            0x73, 0x35,
            0x74, 0x00, // gesture mode
            0x77, 0x01,

            // Return to Bank 0 and re-enable gesture interrupts
            0xEF, 0x00,
            0x41, 0xFF,
            0x42, 0x01,
        };

        private readonly I2cDevice _i2C;
        private readonly byte[] _writePairBuffer = new byte[2];
        private readonly byte[] _readRegisterBuffer = new byte[1];
        private readonly byte[] _readValueBuffer = new byte[1];
        private int _gestureDebounceMilliseconds = DefaultGestureDebounceMilliseconds;
        private bool _initialized;

        /// <summary>
        /// Gets or sets the debounce interval, in milliseconds, applied after a detected gesture.
        /// </summary>
        /// <remarks>Set to 0 to disable debounce.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Gesture debounce must be greater than or equal to 0.
        /// </exception>
        public int GestureDebounceMilliseconds
        {
            get => _gestureDebounceMilliseconds;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _gestureDebounceMilliseconds = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paj7620"/> class.
        /// </summary>
        /// <param name="i2CDevice">I2C device.</param>
        /// <exception cref="ArgumentNullException">
        /// PAJ7620 requires a non-null I2C device instance.
        /// </exception>
        public Paj7620(I2cDevice i2CDevice)
        {
            _i2C = i2CDevice ?? throw new ArgumentNullException(nameof(i2CDevice));
        }

        /// <summary>
        /// Initializes the sensor and validates the expected device ID.
        /// </summary>
        public void Initialize()
        {
            Thread.Sleep(500);

            // Wake up call
            try
            {
                _i2C.WriteByte(0x00);
            }
            catch
            {
                // Ignore. Errors will be caught during ID read. 
            }

            Thread.Sleep(10);

            // Park on Bank 0 before reading part ID.
            WriteByteWithRetry(RegisterBankSelect, 0x00);

            byte idHigh;
            byte idLow;
            try
            {
                idHigh = ReadByteWithRetry(RegisterPartIdHigh);
                idLow = ReadByteWithRetry(RegisterPartIdLow);
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }

            if (idHigh != ExpectedPartIdHigh || idLow != ExpectedPartIdLow)
            {
                throw new IOException($"PAJ7620 error. PartID: 0x{idHigh:X2}{idLow:X2} (Expected: 0x7620)");
            }

            WriteRegisterPairs(InitRegisterPairs);

            // Ensure reads happen from Bank 0.
            WriteByteWithRetry(RegisterBankSelect, 0x00);

            _initialized = true;
        }

        /// <summary>
        /// Tries to read the current gesture.
        /// </summary>
        /// <param name="gesture">Detected gesture or <see cref="Gesture.None"/>.</param>
        /// <returns>True if a valid gesture is available, otherwise false.</returns>
        public bool TryReadGesture(out Gesture gesture)
        {
            EnsureInitialized();

            byte result0 = ReadByteWithRetry(RegisterGestureResult0);
            byte result1 = ReadByteWithRetry(RegisterGestureResult1);

            // Clear latched flags by reading again.
            _ = ReadByteWithRetry(RegisterGestureResult0);
            _ = ReadByteWithRetry(RegisterGestureResult1);

            gesture = DecodeGesture(result0, result1);

            if (gesture != Gesture.None)
            {
                if (GestureDebounceMilliseconds > 0)
                {
                    Thread.Sleep(GestureDebounceMilliseconds);
                }

                // Clear any follow-up movement flags after debounce window.
                _ = ReadByteWithRetry(RegisterGestureResult0);
                _ = ReadByteWithRetry(RegisterGestureResult1);
            }

            return gesture != Gesture.None;
        }

        private static Gesture DecodeGesture(byte result0, byte result1)
        {
            Gesture gesture = (Gesture)result0;

            // Wave flag is in result register 1.
            if ((result1 & 0x01) != 0)
            {
                gesture |= Gesture.Wave;
            }

            return gesture;
        }

        /// <summary>
        /// Validates that the sensor has been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// PAJ7620 is not initialized. Call Initialize() before reading gestures.
        /// </exception>
        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Writes register/value pairs to the sensor.
        /// </summary>
        /// <param name="registerValuePairs">Flat register/value byte pairs to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Initialization data must contain register/value pairs.
        /// </exception>
        private void WriteRegisterPairs(byte[] registerValuePairs)
        {
            if (registerValuePairs == null || registerValuePairs.Length == 0)
            {
                return;
            }

            if ((registerValuePairs.Length & 0x01) != 0)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < registerValuePairs.Length; i += 2)
            {
                WriteByteWithRetry(registerValuePairs[i], registerValuePairs[i + 1]);
            }
        }

        /// <summary>
        /// Writes a byte value to the specified register with retry logic.
        /// </summary>
        /// <param name="register">Register address to write.</param>
        /// <param name="value">Byte value to write.</param>
        /// <exception cref="IOException">
        /// PAJ7620 I2C write failed after retries.
        /// </exception>
        private void WriteByteWithRetry(byte register, byte value)
        {
            _writePairBuffer[0] = register;
            _writePairBuffer[1] = value;

            for (int attempt = 0; attempt < IoRetries; attempt++)
            {
                try
                {
                    _i2C.Write(_writePairBuffer);
                    return;
                }
                catch
                {
                    if (attempt == IoRetries - 1)
                    {
                        throw new IOException();
                    }

                    Thread.Sleep(RetryDelayMilliseconds);
                }
            }
        }

        /// <summary>
        /// Reads a byte value from the specified register with retry logic.
        /// </summary>
        /// <param name="register">Register address to read.</param>
        /// <returns>The byte value read from the register.</returns>
        /// <exception cref="IOException">
        /// PAJ7620 I2C read failed after retries.
        /// </exception>
        private byte ReadByteWithRetry(byte register)
        {
            _readRegisterBuffer[0] = register;

            for (int attempt = 0; attempt < IoRetries; attempt++)
            {
                try
                {
                    _i2C.WriteRead(_readRegisterBuffer, _readValueBuffer);
                    return _readValueBuffer[0];
                }
                catch
                {
                    if (attempt == IoRetries - 1)
                    {
                        throw new IOException();
                    }

                    Thread.Sleep(RetryDelayMilliseconds);
                }
            }

            throw new IOException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2C?.Dispose();
        }
    }
}
