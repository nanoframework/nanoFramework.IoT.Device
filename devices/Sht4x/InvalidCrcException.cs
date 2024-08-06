// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Exception that is thrown when invalid CRC is encountered.
    /// </summary>
    public sealed class InvalidCrcException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCrcException"/> class.
        /// </summary>
        public InvalidCrcException() : base("Invalid CRC")
        { 
        }
    }
}