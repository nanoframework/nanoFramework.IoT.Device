// Copyright (c) 2024 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.EPaper.Utilities
{
    internal static class GpioPinExtensions
    {
        /// <summary>
        /// Blocks the current thread until the pin value equals the desired value.
        /// </summary>
        /// <param name="pin">The GPIO pin to wait on.</param>
        /// <param name="desiredValue">The desired value of the pin to continue thread execution.</param>
        /// <param name="cancellationToken">The <see cref="CancellationTokenSource"/> to use to cancel blocking the thread if needed.</param>
        /// <returns><see langword="true"/> if the waiting ended with the pin changing to the desired value. <see langword="false"/> if the wait ended due to timeout.</returns>
        /// <remarks>Use the <paramref name="cancellationToken"/> as a way to cancel the wait after some time if needed.</remarks>
        public static bool WaitUntilPinValueEquals(
            this GpioPin pin, 
            PinValue desiredValue,
            CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested && pin.Read() != desiredValue)
            {
                cancellationToken.WaitHandle.WaitOne(5, true);
            }

            return !cancellationToken.IsCancellationRequested;
        }
    }
}
