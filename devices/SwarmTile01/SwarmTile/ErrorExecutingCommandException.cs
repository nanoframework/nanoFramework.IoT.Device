﻿//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Exception occurred when executing a command.
    /// </summary>
    public class ErrorExecutingCommandException : Exception
    {
        /// <summary>
        /// Exception occurred when executing a command.
        /// </summary>
        public ErrorExecutingCommandException()
        {
        }

        /// <summary>
        /// Exception occurred when executing a command.
        /// </summary>
        /// <param name="message">Error message with details about the error.</param>
        public ErrorExecutingCommandException(string message) : base(message)
        {
        }
    }
}
