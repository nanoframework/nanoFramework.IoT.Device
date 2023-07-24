// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the remaining PIN/PUK attempts.
    /// </summary>
    public class RemainingPinPukAttempts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemainingPinPukAttempts"/> class with the specified attempts.
        /// </summary>
        /// <param name="pin1">The remaining attempts for PIN1.</param>
        /// <param name="pin2">The remaining attempts for PIN2.</param>
        /// <param name="puk1">The remaining attempts for PUK1.</param>
        /// <param name="puk2">The remaining attempts for PUK2.</param>
        public RemainingPinPukAttempts(int pin1, int pin2, int puk1, int puk2)
        {
            Pin1 = pin1;
            Pin2 = pin2;
            Puk1 = puk1;
            Puk2 = puk2;
        }

        /// <summary>
        /// Gets the remaining attempts for PIN1.
        /// </summary>
        public int Pin1 { get; }

        /// <summary>
        /// Gets the remaining attempts for PIN2.
        /// </summary>
        public int Pin2 { get; }

        /// <summary>
        /// Gets the remaining attempts for PUK1.
        /// </summary>
        public int Puk1 { get; }

        /// <summary>
        /// Gets the remaining attempts for PUK2.
        /// </summary>
        public int Puk2 { get; }

        /// <summary>
        /// Returns a string representation of the remaining PIN/PUK attempts.
        /// </summary>
        /// <returns>A string representing the remaining attempts.</returns>
        public override string ToString()
        {
            return $"PIN1: {Pin1}, PIN2: {Pin2}, PUK1: {Puk1}, PUK2: {Puk2}";
        }
    }
}
