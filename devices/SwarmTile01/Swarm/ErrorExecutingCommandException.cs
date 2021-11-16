//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Exception when executing a command.
    /// </summary>
    public class ErrorExecutingCommandException : Exception
    {
        public ErrorExecutingCommandException()
        {
        }

        public ErrorExecutingCommandException(string message) : base(message)
        {
        }
    }
}
