// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Class with API to manage received messages in the device database.
    /// </summary>
    public class MessagesReceivedManagement
    {
        private readonly SwarmTile _device;

        internal MessagesReceivedManagement(SwarmTile device)
        {
            _device = device;
        }

        /// <summary>
        /// Count of received messages available in the device database.
        /// </summary>
        public int Count => GetMessageCount();

        /// <summary>
        /// Count of received messages unread available in the device database.
        /// </summary>
        public int UnreadCount => GetMessageCount(true);

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
        /// Deletes all the received messages from the device database.
        /// </summary>
        /// <returns><see langword="true"/> on successful deletion of the messages.</returns>
        public bool DeleteAllMessages()
        {
            return DeleteMessages(null, false);
        }

        /// <summary>
        /// Deletes all the received messages marked as read from the device database.
        /// </summary>
        /// <returns><see langword="true"/> on successful deletion of the messages.</returns>
        public bool DeleteAllReadMessages()
        {
            return DeleteMessages(null, true);
        }

        /// <summary>
        /// Marks the specified message as read.
        /// </summary>
        /// <param name="id">ID of message.</param>
        /// <returns><see langword="true"/> on successful marking of the message.</returns>
        public bool MarkMessageRead(string id)
        {
            return MarkMessagesRead(id);
        }

        /// <summary>
        /// Marks all the received messages as read.
        /// </summary>
        /// <returns><see langword="true"/> on successful marking of the messages.</returns>
        public bool MarkAllMessagesRead()
        {
            return MarkMessagesRead(null);
        }

        /// <summary>
        /// Reads the specified message from the device database.
        /// </summary>
        /// <param name="id">ID of message.</param>
        /// <returns>The <see cref="MessageReceived"/> read.</returns>
        /// <remarks>
        /// This is will return <see langword="null"/> if there message doesn't exist in the device database.
        /// </remarks>
        public MessageReceived ReadMessage(string id)
        {
            return ReadMessageHelper(id);
        }

        /// <summary>
        /// Reads the newest message from the device database.
        /// </summary>
        /// <returns>The <see cref="MessageReceived"/> message read.</returns>
        /// <remarks>
        /// This is will return <see langword="null"/> if there are no messages in the device database.
        /// </remarks>
        public MessageReceived ReadNewestMessage()
        {
            return ReadMessageHelper(null, true);
        }

        /// <summary>
        /// Reads the oldest message from the device database.
        /// </summary>
        /// <returns>The <see cref="MessageReceived"/> message read.</returns>
        /// <remarks>
        /// This is will return <see langword="null"/> if there are no messages in the device database.
        /// </remarks>
        public MessageReceived ReadOldestMessage()
        {
            return ReadMessageHelper(null, false);
        }

        internal void ProcessIncomingSentence(NmeaSentence nmeaSentence)
        {
            var msgManagement = new TileCommands.MessagesReceivedManagement.Reply(nmeaSentence);

            if (msgManagement.MessageCount >= 0
               || msgManagement.Message != null)
            {
                // got reply, store
                _device.CommandProcessedReply = msgManagement;

                // flag any command waiting for processing
                _device.CommandProcessed.Set();
            }
            else if (nmeaSentence.Data.Contains("MARKED,")
                    || nmeaSentence.Data.Contains("DELETED,")
                    || nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
            {
                // all good, flag any command waiting for processing
                _device.CommandProcessed.Set();
            }
            else
            {
                // must be an error

                // set error flag 
                _device.ErrorOccurredWhenProcessingCommand = true;

                // flag any command waiting for processing
                _device.CommandProcessed.Set();
            }
        }

        #region command helpers

        private int GetMessageCount(bool unreadOnly = false)
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesReceivedManagement.Command;

                _device.TileSerialPort.WriteLine(TileCommands.MessagesReceivedManagement.GetMessageCount(unreadOnly).ToString());

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
                        return ((TileCommands.MessagesReceivedManagement.Reply)_device.CommandProcessedReply).MessageCount;
                    }
                }
                else
                {
                    // clear command
                    _device.CommandInExecution = string.Empty;

                    throw new TimeoutException();
                }
            }
        }

        private bool DeleteMessages(string id, bool readOnly = false)
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesReceivedManagement.Command;

                if (id == null)
                {
                    _device.TileSerialPort.WriteLine(TileCommands.MessagesReceivedManagement.DeleteMessages(readOnly).ToString());
                }
                else
                {
                    _device.TileSerialPort.WriteLine(TileCommands.MessagesReceivedManagement.DeleteMessage(id).ToString());
                }

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
                    // clear command
                    _device.CommandInExecution = string.Empty;

                    throw new TimeoutException();
                }
            }
        }

        private bool MarkMessagesRead(string id)
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesReceivedManagement.Command;

                _device.TileSerialPort.WriteLine(TileCommands.MessagesReceivedManagement.MarkMessagesRead(id).ToString());

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
                    // clear command
                    _device.CommandInExecution = string.Empty;

                    throw new TimeoutException();
                }
            }
        }

        private MessageReceived ReadMessageHelper(string id, bool newest = false)
        {
            lock (_device.CommandLock)
            {
                // reset error flag
                _device.ErrorOccurredWhenProcessingCommand = false;

                // reset event
                _device.CommandProcessed.Reset();

                // store command
                _device.CommandInExecution = TileCommands.MessagesReceivedManagement.Command;

                if (id == null)
                {
                    _device.TileSerialPort.WriteLine(TileCommands.MessagesReceivedManagement.ReadMessage(newest).ToString());
                }
                else
                {
                    _device.TileSerialPort.WriteLine(TileCommands.MessagesReceivedManagement.ReadMessage(id).ToString());
                }

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
                        return ((TileCommands.MessagesReceivedManagement.Reply)_device.CommandProcessedReply).Message;
                    }
                }
                else
                {
                    // clear command
                    _device.CommandInExecution = string.Empty;

                    throw new TimeoutException();
                }
            }
        }

        #endregion
    }
}
