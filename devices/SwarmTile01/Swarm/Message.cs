//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Message to send to the Swarm network.
    /// </summary>
    public class Message
    {
        private const long _minExpiryDateTime = 637134336000000000;
        private const long _maxExpiryDateTime = 642830804470000000;

        private uint _holdDuration = 0;
        private DateTime _expireTime = DateTime.MinValue;

        /// <summary>
        /// Application ID tag for message.
        /// </summary>
        /// <remarks>
        /// This is optional. If not specified the message will be sent with ID 0.
        /// Swarm reserves the use of application IDs 65000 to 65535.
        /// </remarks>
        public uint ApplicationID { get; set; }

        /// <summary>
        /// Is the number of seconds to hold the message if it has not been sent before expiring it from the database.
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
                if(value < 60 || value > 31536000)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _holdDuration = value;
            }
        }

        /// <summary>
        /// is an epoch second date after which the message will be expired if it has not been sent.
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
                if(value.Ticks < _minExpiryDateTime || value.Ticks > _maxExpiryDateTime)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _expireTime = value;
            }
        }

        /// <summary>
        /// Payload of data.
        /// </summary>
        /// <remarks>
        /// The maximum message size is 192 bytes.
        /// </remarks>
        public string Data { get; set; }
        
        /// <summary>
        /// Message identifier assigned by the device.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public string ID { get; internal set; }

        /// <summary>
        /// Time-stamp of the moment the message was received by the Tile.
        /// </summary>
        /// <remarks>
        /// This property is empty for messages that have been created by code.
        /// </remarks>
        public DateTime TimeStamp { get; internal set; }

        /// <summary>
        /// Create a Message to be sent to the Swarm network. 
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        /// <param name="applicationId">The application ID tag for the message.</param>
        public Message(string data, uint applicationId = 0)
        {
            if(data.Length > 192)
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
        public Message(byte[] data, uint applicationId = 0)
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
