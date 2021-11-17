//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Base class for the received and transmitted messages.
    /// </summary>
    public class MessageBase
    {

        /// <summary>
        /// Application ID tag for message.
        /// </summary>
        /// <remarks>
        /// This is optional. If not specified the message will be sent with ID 0.
        /// Swarm reserves the use of application IDs 65000 to 65535.
        /// </remarks>
        public uint ApplicationID { get; set; }

        /// <summary>
        /// Payload of data.
        /// </summary>
        /// <remarks>
        /// The maximum message size is 192 bytes.
        /// </remarks>
        public string Data { get; set; }

        /// <summary>
        /// Create a Message to be sent to the Swarm network. 
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        /// <param name="applicationId">The application ID tag for the message.</param>
        public MessageBase(string data, uint applicationId = 0)
        {
            if (data.Length > 192)
            {
                throw new ArgumentOutOfRangeException();
            }

            Data = data;
            ApplicationID = applicationId;
        }

        /// <summary>
        /// Create a Message to be sent to the Swarm network. 
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        /// <param name="applicationId">The application ID tag for the message.</param>
        public MessageBase(byte[] data, uint applicationId = 0)
        {
            if (data.Length > 192)
            {
                throw new ArgumentOutOfRangeException();
            }

            Data = BitConverter.ToString(data);
            ApplicationID = applicationId;
        }
    }
}
