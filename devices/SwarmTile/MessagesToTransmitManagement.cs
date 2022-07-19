// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Diagnostics;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Class with API to manage the device database for the messages to be transmitted.
    /// </summary>
    public class MessagesToTransmitManagement
    {
        // queue to hold messages being listed
        private readonly Queue _msgCollection = new Queue();
        private readonly SwarmTile _device;

        // flag to signal that we're listing messages
        private bool _listingMessages = false;

        private int _expectedMessageCount;

        internal MessagesToTransmitManagement(SwarmTile device)
        {
            _device = device;
        }

        /// <summary>
        /// Count of unsent messages available in the device database.
        /// </summary>
        public int Count => GetMessageCount();

        /// <summary>
        /// Deletes the specified message from the device database.
        /// </summary>
        /// <param name="id">ID of message.</param>
        /// <returns><see langword="true"/> on successful deletion of the message.</returns>
        public bool DeleteMessage(string id)
        {
            return DeleteMessages(id);
        }

        /// <summary>
        /// Deletes all the unsent messages from the device database.
        /// </summary>
        /// <returns><see langword="true"/> on successful deletion of the messages.</returns>
        public bool DeleteAllMessages()
        {
            return DeleteMessages(null);
        }

        /// <summary>
        /// Gets the specified message from the device database.
        /// </summary>
        /// <param name="id">ID of message.</param>
        /// <returns>The <see cref="MessageToTransmit"/> read.</returns>
        /// <remarks>
        /// This is will return <see langword="null"/> if there message doesn't exist in the device database.
        /// </remarks>
        public MessageToTransmit GetMessage(string id)
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesToTransmitManagement.Command;

                _device.TileSerialPort.WriteLine(TileCommands.MessagesToTransmitManagement.ListMessage(id).ToString());

                // wait from command to be processed
                var eventSignaled = _device.CommandProcessed.WaitOne(_device.TimeoutForCommandExecution, false);

                // clear command
                _device.CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (_device.ErrorOccurredWhenProcessingCommand)
                    {
                        return null;
                    }
                    else
                    {
                        return ((TileCommands.MessagesToTransmitManagement.Reply)_device.CommandProcessedReply).Message;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Gets all the unsent messages from the device database.
        /// </summary>
        /// <returns>A array with <see cref="MessageReceived"/> messages read.</returns>
        /// <remarks>
        /// This is will return an empty array if there are no messages in the device database.
        /// </remarks>
        public MessageToTransmit[] GetAllMessages()
        {
            // clear queue, just in case
            _msgCollection.Clear();

            // get how many messages are available
            _expectedMessageCount = Count;

            if (_expectedMessageCount == 0)
            {
                return new MessageToTransmit[0];
            }

            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // set flag that we're listing messages
                _listingMessages = true;

                // store command
                _device.CommandInExecution = TileCommands.MessagesToTransmitManagement.Command;

                _device.TileSerialPort.WriteLine(TileCommands.MessagesToTransmitManagement.ListMessage(null).ToString());

                // wait from command to be processed
                var eventSignaled = _device.CommandProcessed.WaitOne(_device.TimeoutForCommandExecution, false);

                // clear command
                _device.CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // clear flag
                    _listingMessages = false;
                }
                else
                {
                    // clear flag
                    _listingMessages = false;

                    throw new TimeoutException();
                }
            }

            // copy to a new array to return that one
            MessageToTransmit[] tempArray = new MessageToTransmit[_msgCollection.Count];
            _msgCollection.CopyTo(tempArray, 0);

            // clear the internal field to save memory
            _msgCollection.Clear();

            return tempArray;
        }

        internal void ProcessIncomingSentence(NmeaSentence nmeaSentence)
        {
            var msgManagement = new TileCommands.MessagesToTransmitManagement.Reply(nmeaSentence);

            if (msgManagement.MessageCount >= 0
               || msgManagement.Message != null)
            {
                if (_listingMessages)
                {
                    // add to queue
                    _msgCollection.Enqueue(msgManagement.Message);

                    // decrease count
                    _expectedMessageCount--;

                    if (_expectedMessageCount > 0)
                    {
                        // done here
                        return;
                    }
                }
                else
                {
                    // got reply, store
                    _device.CommandProcessedReply = msgManagement;
                }

                // flag any command waiting for processing
                _device.CommandProcessed.Set();
            }
            else if (nmeaSentence.Data.Contains("DELETED,")
                     || nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
            {
                // all good, flag any command waiting for processing
                _device.CommandProcessed.Set();
            }
            else
            {
                // must be an error or ignoring a list messages reply

                // set error flag 
                _device.ErrorOccurredWhenProcessingCommand = true;

                // flag any command waiting for processing
                _device.CommandProcessed.Set();
            }
        }

        #region command helpers

        private int GetMessageCount()
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesToTransmitManagement.Command;

                _device.TileSerialPort.WriteLine(TileCommands.MessagesToTransmitManagement.GetMessageCount().ToString());

                // wait from command to be processed
                var eventSignaled = _device.CommandProcessed.WaitOne(_device.TimeoutForCommandExecution, false);

                // clear command
                _device.CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (_device.ErrorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException();
                    }
                    else
                    {
                        return ((TileCommands.MessagesToTransmitManagement.Reply)_device.CommandProcessedReply).MessageCount;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        private bool DeleteMessages(string id)
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesToTransmitManagement.Command;

                _device.TileSerialPort.WriteLine(TileCommands.MessagesToTransmitManagement.DeleteMessages(id).ToString());

                // wait from command to be processed
                var eventSignaled = _device.CommandProcessed.WaitOne(_device.TimeoutForCommandExecution, false);

                // clear command
                _device.CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (_device.ErrorOccurredWhenProcessingCommand)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        #endregion
    }
}
