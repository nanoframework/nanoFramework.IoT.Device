// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Provides extension methods for <see cref="PhoneNumber"/> objects.
    /// </summary>
    public static class PhoneNumberExtensions
    {
        /// <summary>
        /// Gets the type of number for the phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>The <see cref="TypeOfNumber"/> for the phone number.</returns>
        public static TypeOfNumber GetTypeOfNumber(this PhoneNumber phoneNumber)
        {
            return phoneNumber.Number.StartsWith("+") ? TypeOfNumber.International : TypeOfNumber.National;
        }

        /// <summary>
        /// Gets the number plan identification for the phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>The <see cref="NumberPlanIdentification"/> for the phone number.</returns>
        public static NumberPlanIdentification GetNumberPlanIdentification(this PhoneNumber phoneNumber)
        {
            return phoneNumber.Number.StartsWith("+") ? NumberPlanIdentification.ISDN : NumberPlanIdentification.Unknown;
        }
    }
}
