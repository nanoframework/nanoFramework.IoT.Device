// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer.Options
{
    /// <summary>
    /// A base class for implementing <see cref="IOption"/>.
    /// </summary>
    public abstract class Option : IOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Option"/> class with the specified <paramref name="code"/> and <paramref name="data"/>.
        /// </summary>
        /// <param name="code">The option code.</param>
        /// <param name="data">The option data.</param>
        protected Option(
            DhcpOptionCode code,
            byte[] data) : this(
                code,
                data,
                (byte)data.Length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Option"/> class with the specified <paramref name="code"/>, <paramref name="data"/>, and <paramref name="length"/>.
        /// </summary>
        /// <param name="code">The option code.</param>
        /// <param name="data">The option data.</param>
        /// <param name="length">The length of the option data.</param>
        protected Option(
            DhcpOptionCode code,
            byte[] data,
            byte length)
        {
            Code = code;
            Data = data;
            Length = length;
        }

        /// <inheritdoc />
        public DhcpOptionCode Code { get; }

        /// <inheritdoc />
        public byte[] Data { get; }

        /// <inheritdoc />
        public byte Length { get; }

        /// <inheritdoc />
        public ushort OptionLength => (ushort)(Length + 2);

        /// <inheritdoc />
        public byte[] GetBytes()
        {
            var data = new byte[OptionLength];
            data[0] = (byte)Code;
            data[1] = Length;

            Converter.CopyTo(Data, 0, data, 2, Length);

            return data;
        }

        /// <inheritdoc cref="object.ToString()" />
        public abstract override string ToString();

        /// <summary>
        /// Provides common formatting for <see cref="Option.ToString()"/>.
        /// </summary>
        /// <param name="value">The value to include in the string.</param>
        /// <returns>A <see langword="string"/> representation of the option.</returns>
        protected string ToString(object value) => $"{Code}: {value}";
    }
}
