// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Message to be transmitted to the Swarm network.
    /// </summary>
    public class MessageToTransmit : MessageBase
    {
        private const long MinExpiryDateTime = 637134336000000000;
        private const long MaxExpiryDateTime = 642830804470000000;

        private uint _holdDuration = 0;
        private DateTime _expireTime = DateTime.MinValue;

        /// <summary>
        /// Gets or sets the number of seconds to hold the message if it has not been sent before expiring it from the database.
        /// </summary>
        /// <remarks>
        /// This is optional. The default is 172800 seconds, minimum is 60 seconds.
        /// </remarks>
        public uint HoldDuration
        {
            get
            {
                return _holdDuration;
            }

            set
            {
                if (value < 60 || value > 31536000)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _holdDuration = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DateTime"/> value after which the message will be expired if it has not been sent.
        /// </summary>
        /// <remarks>
        /// The message will be considered expired if not sent before the specified time.
        /// </remarks>
        public DateTime ExpireTime
        {
            get
            {
                return _expireTime;
            }

            set
            {
                if (value.Ticks < MinExpiryDateTime || value.Ticks > MaxExpiryDateTime)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _expireTime = value;
            }
        }

        /// <summary>
        /// Gets message identifier assigned by the device.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public string ID { get; internal set; }

        /// <summary>
        /// Gets time-stamp of the moment the message was received by the Tile.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public DateTime TimeStamp { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageToTransmit"/> class.
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        /// <param name="applicationId">The application ID tag for the message.</param>
        /// <remarks>Creates a Message to be sent to the Swarm network.</remarks>
        public MessageToTransmit(string data, uint applicationId = 0) : base(data, applicationId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageToTransmit"/> class.
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        /// <param name="applicationId">The application ID tag for the message.</param>
        /// <remarks>Creates a Message to be sent to the Swarm network.</remarks>
        public MessageToTransmit(byte[] data, uint applicationId = 0) : base(data, applicationId)
        {
        }
    }
}
