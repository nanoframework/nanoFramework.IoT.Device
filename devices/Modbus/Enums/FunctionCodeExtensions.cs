// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus
{
    /// <summary>
    /// Extensions for <see cref="FunctionCode"/>.
    /// </summary>
    internal static class FunctionCodeExtensions
    {
        /// <summary>
        /// Returns the enum member name for a <see cref="FunctionCode"/> value.
        /// </summary>
        /// <param name="value">The <see cref="FunctionCode"/> value.</param>
        /// <returns>The name of the enum member if defined; otherwise, the numeric value as a string.</returns>
        public static string ToNameString(this FunctionCode value)
        {
            switch (value)
            {
                case FunctionCode.ReadCoils: return nameof(FunctionCode.ReadCoils);
                case FunctionCode.ReadDiscreteInputs: return nameof(FunctionCode.ReadDiscreteInputs);
                case FunctionCode.ReadHoldingRegisters: return nameof(FunctionCode.ReadHoldingRegisters);
                case FunctionCode.ReadInputRegisters: return nameof(FunctionCode.ReadInputRegisters);
                case FunctionCode.WriteSingleCoil: return nameof(FunctionCode.WriteSingleCoil);
                case FunctionCode.WriteSingleRegister: return nameof(FunctionCode.WriteSingleRegister);
                case FunctionCode.Diagnostics: return nameof(FunctionCode.Diagnostics);
                case FunctionCode.WriteMultipleCoils: return nameof(FunctionCode.WriteMultipleCoils);
                case FunctionCode.WriteMultipleRegisters: return nameof(FunctionCode.WriteMultipleRegisters);
                case FunctionCode.EncapsulatedInterface: return nameof(FunctionCode.EncapsulatedInterface);
                default:
                    // fallback to numeric value
                    return ((byte)value).ToString();
            }
        }
    }
}
