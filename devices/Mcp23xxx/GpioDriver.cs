// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;

namespace System.Device.Gpio
{
    /// <summary>
    /// Base class for Gpio Drivers.
    /// A Gpio driver provides methods to read from and write to digital I/O pins.
    /// </summary>
    public abstract class GpioDriver : IDisposable
    {
        /// <summary>
        /// The number of pins provided by the driver.
        /// </summary>
        protected abstract int PinCount { get; }

        /// <summary>
        /// Converts a board pin number to the driver's logical numbering scheme.
        /// </summary>
        /// <param name="pinNumber">The board pin number to convert.</param>
        /// <returns>The pin number in the driver's logical numbering scheme.</returns>
        protected abstract int ConvertPinNumberToLogicalNumberingScheme(int pinNumber);

        /// <summary>
        /// Opens a pin in order for it to be ready to use.
        /// The driver attempts to open the pin without changing its mode or value.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected abstract void OpenPin(int pinNumber);

        /// <summary>
        /// Closes an open pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        protected abstract void ClosePin(int pinNumber);

        /// <summary>
        /// Sets the mode to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to be set.</param>
        protected abstract void SetPinMode(int pinNumber, PinMode mode);

        /// <summary>
        /// Gets the mode of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The mode of the pin.</returns>
        protected abstract PinMode GetPinMode(int pinNumber);

        /// <summary>
        /// Checks if a pin supports a specific mode.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="mode">The mode to check.</param>
        /// <returns>The status if the pin supports the mode.</returns>
        protected abstract bool IsPinModeSupported(int pinNumber, PinMode mode);

        /// <summary>
        /// Reads the current value of a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <returns>The value of the pin.</returns>
        protected abstract PinValue Read(int pinNumber);

        /// <summary>
        /// Writes a value to a pin.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="value">The value to be written to the pin.</param>
        protected abstract void Write(int pinNumber, PinValue value);

        /// <summary>
        /// Adds a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="eventTypes">The event types to wait for.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected abstract void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback);

        /// <summary>
        /// Removes a handler for a pin value changed event.
        /// </summary>
        /// <param name="pinNumber">The pin number in the driver's logical numbering scheme.</param>
        /// <param name="callback">Delegate that defines the structure for callbacks when a pin value changed event occurs.</param>
        protected abstract void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback);

        /// <summary>
        /// Disposes this instance, closing all open pins
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <param name="disposing">True if explicitly disposing, false if in finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to do in base class.
        }
    }
}
