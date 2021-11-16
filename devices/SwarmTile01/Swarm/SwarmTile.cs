//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Swarm Tile class.
    /// </summary>
    public class SwarmTile : IDisposable
    {
        // private static readonly SwarmTile _instance = new SwarmTile();

        private readonly object _lock = new object();
        private readonly object _commandLock = new object();

        private SerialPort _tileSerialPort;

        // event to signal that a new message has been received and placed on the queue
        private readonly AutoResetEvent _messageReceived = new AutoResetEvent(false);

        // event to signal that a new command has been added to the queue for processing
        private readonly AutoResetEvent _commandProcessed = new AutoResetEvent(false);

        // flag to signal that the very 1st message from the Tile was received
        private bool _isFirstMessage = true;

        // flag to signal that an error has occurred when processing the command
        private bool _errorOccurredWhenProcessingCommand = false;

        // variable holding the reply of the last command executed
        private object _commandProcessedReply;

        private bool _disposed;

        private Thread _processIncommingMessagesThread;


        private readonly Queue _incommingMessagesQueue = new();

        // backing fields 
        internal TileCommands.DateTimeStatus.Reply _dateTimeStatus;
        internal PowerState _powerState = PowerState.Unknown;

        /// <summary>
        /// Timeout for command exection (in miliseconds).
        /// </summary>
        public int TimeoutForCommandExecution { get; set; } = 5000;

        /// <summary>
        /// Returns the device firmware version.
        /// </summary>
        public string FirmwareVersion { get; private set; }

        /// <summary>
        /// Returns the time stamp of the device firmware.
        /// </summary>
        public string FirmwareTimeStamp { get; private set; }

        /// <summary>
        /// Device ID that identifies this device on the Swarm network.
        /// </summary>
        public string DeviceID { get; }

        /// <summary>
        /// Device type name.
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// Current DateTime value as received from the module.
        /// </summary>
        public DateTime CurrentDateTime => _dateTimeStatus.Value;

        /// <summary>
        /// Information on ether CurrentDateTime.
        /// </summary>
        public bool DateTimeIsValid => _dateTimeStatus.IsValid;

        /// <summary>
        /// Received background noise signal strength in dBm.
        /// </summary>
        /// <remarks>
        /// For reliable operation, this value should consistently be less than (more negative) than -93 dBm.
        /// Use OperationalQuality
        /// </remarks>
        public int BackgroundNoiseRssi { get; private set; } = 0;

        /// <summary>
        /// Quality of the operation based on the background noise signal strength.
        /// </summary>
        public OperationalQuality OperationalQuality => SwarmUtilities.GetOperationalQuality(BackgroundNoiseRssi);

        /// <summary>
        /// Received signal strength in dBm from satellite for last packet received.
        /// </summary>
        public int SatelliteRssi { get; private set; } = 0;

        /// <summary>
        /// Signal to noise ratio in dB for last packet received.
        /// </summary>
        public int SignalToNoiseRatio { get; private set; } = 0;

        /// <summary>
        /// Frequency deviation in Hz for packet.
        /// </summary>
        public int FrequencyDeviation { get; private set; } = 0;

        /// <summary>
        /// Timestamp of the last packet received.
        /// </summary>
        public DateTime LastPacketReceivedTimestamp { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// ID of the last satellite heard.
        /// </summary>
        public int SatelliteId { get; private set; } = 0;

        /// <summary>
        /// Power state of the Swarm Tile
        /// </summary>
        public PowerState PowerState
        {
            get { return _powerState; }
            internal set
            {
                if (_powerState != value)
                {
                    _powerState = value;

                    // raise event for power status changed on a thread
                    new Thread(() => { OnPowerStateChanged(_powerState); }).Start();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName"></param>
        public SwarmTile(string portName)
        {
            // config SerialPort and...
            _tileSerialPort = new SerialPort(portName, 115200);

            //... try opening it
            // failure to open the SerialPort will throw an exception
            _tileSerialPort.Open();

            // set new line char
            _tileSerialPort.NewLine = "\n";

            // set watch char: LF
            _tileSerialPort.WatchChar = '\n';

            // setup event handler for receive buffer
            _tileSerialPort.DataReceived += Tile_DataReceived;

            // incomming messages worker thread
            _processIncommingMessagesThread = new Thread(ProcessIncommingMessagesWorkerThread);
            _processIncommingMessagesThread.Start();
        }

        private void Tile_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // discard event if there is nothing to read or if this is not a WatchChar event
            // meaning that there isn't yet a full NMEA sentence to read in the buffer
            if (e.EventType != SerialData.WatchChar
                || _tileSerialPort.BytesToRead == 0)
            {
                return;
            }

#if DEBUG

            //Debug.WriteLine($"chars ava1>>{_tileSerialPort.BytesToRead}");

            var receivedMessage = _tileSerialPort.ReadLine();
            Debug.WriteLine($">>{receivedMessage}");

            //Debug.WriteLine($"chars ava2>>{_tileSerialPort.BytesToRead}");

#else

            // read line
            var receivedMessage = _tileSerialPort.ReadLine();

#endif
            // skip empty messages
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                _incommingMessagesQueue.Enqueue(receivedMessage);

                // signal event
                _messageReceived.Set();

                // check if this is the very 1st message receveide
                if (_isFirstMessage)
                {
                    // reset flag
                    _isFirstMessage = false;

                    PowerState = PowerState.On;

                    // fire thread to get general details
                    var getDetailsThread = new Thread(GetGeneralDetailsThread);
                    getDetailsThread.Start();
                }
            }
        }

        /// <summary>
        /// The thread used to process asynchronously incomming NMEA sentences
        /// </summary>
        private void ProcessIncommingMessagesWorkerThread()
        {
            while (true)
            {
                _messageReceived.WaitOne();

                while (_incommingMessagesQueue.Count > 0)
                {
                    lock (_lock)
                    {
                        var receivedMessage = (string)_incommingMessagesQueue.Dequeue();

                        var nmeaSentence = NmeaSentence.FromRawSentence(receivedMessage);

                        if (nmeaSentence != null)
                        {
                            ProcessIncommingMessage(nmeaSentence);
                        }
                        else
                        {
                            // invalid NMEA sentence
                            Debug.WriteLine($"Invalid NMEA sentence: {receivedMessage}");
                        }
                    }
                }
            }
        }

        private void ProcessIncommingMessage(NmeaSentence nmeaSentence)
        {
            var prefix = nmeaSentence.Data.Substring(0, 2);

            switch (prefix)
            {
                case TileCommands.DateTimeStatus.Command:
                    var dtStatus = new TileCommands.DateTimeStatus.Reply(nmeaSentence);

                    if (dtStatus.Value >= DateTime.MinValue)
                    {
                        _dateTimeStatus = dtStatus;
                    }
                    break;

                case TileCommands.ReceiveTest.Command:
                    var receiveTest = new TileCommands.ReceiveTest.Reply(nmeaSentence);

                    if (receiveTest.BackgroundRssi > int.MinValue)
                    {
                        // this reply it's a RT satellite message
                        BackgroundNoiseRssi = receiveTest.BackgroundRssi;
                        //_dateTimeStatus = dtStatus;
                    }
                    else if (receiveTest.Rate > uint.MinValue)
                    {
                        // reply it's the RT rate, store
                        _commandProcessedReply = receiveTest;

                        // signal event 
                        _commandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Substring(2, 3) == " OK")
                    {
                        // flag any command waiting for processing
                        _commandProcessed.Set();
                    }

                    break;

                case TileCommands.RetreiveFirmwareVersion.Command:
                    var fwVersion = new TileCommands.RetreiveFirmwareVersion.Reply(nmeaSentence);

                    if (fwVersion.FirmwareVersion != null)
                    {
                        FirmwareVersion = fwVersion.FirmwareVersion;
                        FirmwareTimeStamp = fwVersion.FirmwareTimeStamp;
                    }
                    break;

                case TileCommands.TransmitData.Command:
                    var txData = new TileCommands.TransmitData.Reply(nmeaSentence);

                    if (txData.MessageId != null)
                    {
                        // store reply
                        _commandProcessedReply = txData;

                        // signal event 
                        _commandProcessed.Set();
                    }
                    else if (txData.ErrorMessage != null)
                    {
                        // store reply, which contains the error
                        _commandProcessedReply = txData;

                        // set error flag 
                        _errorOccurredWhenProcessingCommand = true;

                        // signal event 
                        _commandProcessed.Set();
                    }
                    else if(txData.Event == Swarm.MessageEvent.Received)
                    {
                        // $TD SENT RSSI=<rssi_sat>,SNR=<snr>,FDEV=<fdev>,<msg_id>*xx
                        //          |              |         |           |       |
                        //          7                     

                        int startIndex = 7;
                        ProcessMessageReceivedEvent(nmeaSentence.Data.Substring(startIndex));
                    }
                    else if(txData.Event == Swarm.MessageEvent.Expired)
                    {
                        // raise event for message expired on a thread
                        new Thread(() => { OnMessageEvent(Swarm.MessageEvent.Expired, txData.MessageId); }).Start();
                    }
                    break;

                default:
                    // start checking OK and ERROR
                    // check for OK...
                    if (nmeaSentence.Data.Substring(2, 3) == " OK")
                    {
                        // we're good
                        // flag any command waiting for processing
                        _commandProcessed.Set();
                    }
                    // ... ERROR messages
                    else if (nmeaSentence.Data.Substring(2, 4) == " ERR")
                    {
                        // error 
                        // set error flag 
                        _errorOccurredWhenProcessingCommand = true;

                        // flag any command waiting for processing
                        _commandProcessed.Set();
                    }
                    else if (ProcessKnownPrompts(nmeaSentence))
                    {

                    }
                    else
                    {
                        // unknow message
                        Debug.WriteLine($"Unknown message NOT processed: {nmeaSentence.Data}");
                    }
                    break;
            }
        }

        private void ProcessMessageReceivedEvent(string eventData)
        {
            // RSSI=<rssi_sat>,SNR=<snr>,FDEV=<fdev>,<msg_id>

            try
            {
                // split data and fill in properties
                var eventDetails = eventData.Split(',');

                // RSSI
                var rssi = eventDetails[0].Split('=');
                SatelliteRssi = int.Parse(rssi[1]);

                // SNR
                var snr = eventDetails[1].Split('=');
                SignalToNoiseRatio = int.Parse(snr[1]);

                // FDEV
                var fdev = eventDetails[2].Split('=');
                FrequencyDeviation = int.Parse(fdev[1]);

                // raise event for message received on a thread
                new Thread(() => { OnMessageEvent(Swarm.MessageEvent.Received, eventDetails[3]); }).Start();
            }
            catch
            {
                // ignore any exceptions that occurr during processing
            }
        }

        private bool ProcessKnownPrompts(NmeaSentence nmeaSentence)
        {
            if (nmeaSentence.Data.Contains("SL WAKE"))
            {
                // device woke up
                // update property
                PowerState = PowerState.On;

                return true;
            }
            else if (nmeaSentence.Data.StartsWith("TILE"))
            {
                Debug.WriteLine(nmeaSentence.Data);

                return true;
            }

            return false;
        }

        private void GetGeneralDetailsThread()
        {
            if (string.IsNullOrEmpty(FirmwareVersion) || string.IsNullOrEmpty(FirmwareTimeStamp))
            {
                _tileSerialPort.WriteLine(new TileCommands.RetreiveFirmwareVersion().ComposeToSend().ToString());
            }
        }

        #region Commands

        /// <summary>
        /// Put Tile in Power Off mode.
        /// </summary>
        /// <remarks>
        /// After issuing this command all Tile power supplies should be disconnect.
        /// If power is not disconnected, the Tile enters a low power mode until power is completely removed and restored.
        /// </remarks>
        public void PowerOff()
        {
            lock (_commandLock)
            {
                // reset error flag
                _errorOccurredWhenProcessingCommand = false;

                // reset event
                _commandProcessed.Reset();

                _tileSerialPort.WriteLine(new TileCommands.PowerOff().ComposeToSend().ToString());

                // wait from command to be processed
                if (_commandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (_errorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException();
                    }
                    else
                    {
                        // update property
                        PowerState = PowerState.Off;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Set the rate for unsolicited report messages for device power state.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message. Set to 0 to disable.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timout occurred when waiting for command execution.</exception>
        public void SetReceiveTestRate(uint rate)
        {
            lock (_commandLock)
            {
                // reset error flag
                _errorOccurredWhenProcessingCommand = false;

                // reset event
                _commandProcessed.Reset();

                _tileSerialPort.WriteLine(new TileCommands.ReceiveTest((int)rate).ComposeToSend().ToString());

                // wait from command to be processed
                if (_commandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (_errorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException();
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Get the rate for unsolicited report messages for device power state.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timout occurred when waiting for command execution.</exception>
        public uint GetReceiveTestRate()
        {
            lock (_commandLock)
            {
                // reset error flag
                _errorOccurredWhenProcessingCommand = false;

                // reset event
                _commandProcessed.Reset();

                // send the command with -1 to get the current setting
                _tileSerialPort.WriteLine(new TileCommands.ReceiveTest(-1).ComposeToSend().ToString());

                // wait from command to be processed
                if (_commandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (_errorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException();
                    }
                    else
                    {
                        return ((TileCommands.ReceiveTest.Reply)_commandProcessedReply).Rate;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Puts the device into a low-power sleep mode.
        /// </summary>
        /// <param name="value">Sleep for this many seconds.</param>
        /// <exception cref="ArgumentException">If rate is &lt; 5.</exception>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timout occurred when waiting for command execution.</exception>
        public void SendToSleep(uint value)
        {
            if (value < 1)
            {
                throw new ArgumentException();
            }

            lock (_commandLock)
            {
                // reset error flag
                _errorOccurredWhenProcessingCommand = false;

                // reset event
                _commandProcessed.Reset();

                _tileSerialPort.WriteLine(new TileCommands.SleepMode(value).ComposeToSend().ToString());

                // wait from command to be processed
                if (_commandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (_errorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException();
                    }
                    else
                    {
                        // device is now in sleep mode
                        // update property
                        PowerState = PowerState.Sleep;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Puts the device into a low-power sleep mode.
        /// </summary>
        /// <param name="wakeupTime">Sleep until date and time.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timout occurred when waiting for command execution.</exception>
        public void SendToSleep(DateTime wakeupTime)
        {
            lock (_commandLock)
            {
                // reset error flag
                _errorOccurredWhenProcessingCommand = false;

                // reset event
                _commandProcessed.Reset();

                _tileSerialPort.WriteLine(new TileCommands.SleepMode(wakeupTime).ComposeToSend().ToString());

                // wait from command to be processed
                if (_commandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (_errorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException();
                    }
                    else
                    {
                        // device is now in sleep mode
                        // update property
                        PowerState = PowerState.Sleep;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Transmits data to the Swarm network.
        /// </summary>
        /// <param name="message">The message with the data to be transmited.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timout occurred when waiting for command execution.</exception>
        public string TransmitData(Message message)
        {
            lock (_commandLock)
            {
                // reset error flag
                _errorOccurredWhenProcessingCommand = false;

                // reset event
                _commandProcessed.Reset();

                _tileSerialPort.WriteLine(new TileCommands.TransmitData(message).ComposeToSend().ToString());

                // wait from command to be processed
                if (_commandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (_errorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException(((TileCommands.TransmitData.Reply)_commandProcessedReply).ErrorMessage);
                    }
                    else
                    {
                        return ((TileCommands.TransmitData.Reply)_commandProcessedReply).MessageId;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }

        #endregion

        #region Power State handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="PowerStateChanged"/> event.
        /// </summary>
        /// <param name="powerState"> new power status of the device</param>
        public delegate void PowerStateChangedHandler(PowerState powerState);

        /// <summary>
        /// Event raised when the power state of the device changes.
        /// </summary>
        public static event PowerStateChangedHandler PowerStateChanged;
        private PowerStateChangedHandler onPowerStateChanged;

        /// <summary>
        /// Raises the <see cref="PowerStateChanged"/> event.
        /// </summary>
        /// <param name="powerStatus"> new power status of the device</param>
        protected virtual void OnPowerStateChanged(PowerState powerStatus)
        {
            if (onPowerStateChanged == null) onPowerStateChanged = new PowerStateChangedHandler(PowerStateChanged);
            PowerStateChanged?.Invoke(powerStatus);
        }

        #endregion


        #region Message events

        /// <summary>
        /// Represents the delegate used for the <see cref="MessageEvent"/> event.
        /// </summary>
        /// <param name="messageEvent">Event occurred about a message</param>
        /// <param name="messageId">Id of message the event is related with</param>
        public delegate void MessageEventHandler(MessageEvent messageEvent, string messageId);

        /// <summary>
        /// Event raised related with a message.
        /// </summary>
        public static event MessageEventHandler MessageEvent;
        private MessageEventHandler onMessageEvent;

        /// <summary>
        /// Raises the <see cref="MessageEvent"/> event.
        /// </summary>
        /// <param name="messageEvent">Event occurred about a message</param>
        /// <param name="messageId">Id of message the event is related with</param>
        protected virtual void OnMessageEvent(MessageEvent messageEvent, string messageId)
        {
            if (onMessageEvent == null) onMessageEvent = new MessageEventHandler(MessageEvent);
            MessageEvent?.Invoke(messageEvent, messageId);
        }

        #endregion

        #region IDisposable implementation

        /// <inheritdoc/>
        ~SwarmTile()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Release managed resources
                try
                {
                    //_asyncTaskQueueThread.Abort();
                    //_asyncTaskQueueThread.Join();
                    //_serialDevice.Dispose();
                }
                finally
                {
                    //Instance.Release();
                }
            }

            _disposed = true;
        }

        #endregion
    }
}