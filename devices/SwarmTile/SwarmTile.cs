// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.//

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
        internal readonly object CommandLock = new object();

        // event to signal that a new command has been added to the queue for processing
        internal readonly AutoResetEvent CommandProcessed = new AutoResetEvent(false);

        // variable holding the command that's being executed, if any
        internal string CommandInExecution { get; set; }

        internal SerialPort TileSerialPort { get; set; }

        private readonly object _lock = new object();
 
        // flag to signal that an error has occurred when processing the command
        internal bool ErrorOccurredWhenProcessingCommand { get; set; } = false;

        // variable holding the reply of the last command executed
        internal object CommandProcessedReply { get; set; }

        // backing fields 
        internal PowerState InternalPowerState { get; set; } = Swarm.PowerState.Unknown;

        // event to signal that a new message has been received and placed on the queue
        private readonly AutoResetEvent _messageReceived = new AutoResetEvent(false);

        private readonly Queue _incommingMessagesQueue = new Queue();

        // flag to signal that the very 1st message from the Tile was received
        private bool _isFirstMessage = true;

        private bool _disposed;

        private Thread _processIncommingSentencesThread;

        /// <summary>
        /// Gets or sets the timeout for command execution (in milliseconds).
        /// </summary>
        public int TimeoutForCommandExecution { get; set; } = 5000;

        /// <summary>
        /// Gets the device firmware version.
        /// </summary>
        public string FirmwareVersion { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the time stamp of the device firmware.
        /// </summary>
        public string FirmwareTimeStamp { get; private set; } = string.Empty;

        /// <summary>
        /// Gets device ID that identifies this device on the Swarm network.
        /// </summary>
        public string DeviceID { get; private set; } = string.Empty;

        /// <summary>
        /// Gets device type name.
        /// </summary>
        public string DeviceName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets received background noise signal strength in dBm.
        /// </summary>
        /// <remarks>
        /// For reliable operation, this value should consistently be less than (more negative) than -93 dBm.
        /// Use OperationalQuality.
        /// </remarks>
        public int BackgroundNoiseRssi { get; private set; } = 0;

        /// <summary>
        /// Quality of the operation based on the background noise signal strength.
        /// </summary>
        public OperationalQuality OperationalQuality => SwarmUtilities.GetOperationalQuality(BackgroundNoiseRssi);

        /// <summary>
        /// Gets received signal strength in dBm from satellite for last packet received.
        /// </summary>
        public int SatelliteRssi { get; private set; } = 0;

        /// <summary>
        /// Gets signal to noise ratio in dB for last packet received.
        /// </summary>
        public int SignalToNoiseRatio { get; private set; } = 0;

        /// <summary>
        /// Gets frequency deviation in Hz for packet.
        /// </summary>
        public int FrequencyDeviation { get; private set; } = 0;

        /// <summary>
        /// Gets time-stamp of the last packet received.
        /// </summary>
        public DateTime LastPacketReceivedTimestamp { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Gets ID of the last satellite heard.
        /// </summary>
        public int SatelliteId { get; private set; } = 0;

        /// <summary>
        /// Gets power state of the Swarm Tile.
        /// </summary>
        public PowerState PowerState
        {
            get => PowerState;
            internal set
            {
                if (PowerState != value)
                {
                    PowerState = value;

                    // raise event for power status changed on a thread
                    new Thread(() => { OnPowerStateChanged(PowerState); }).Start();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether GPS has acquired a valid date/time reference.
        /// </summary>
        public bool DateTimeIsValid { get; private set; }

        /// <summary>
        /// Gets a value indicating whether GPS has acquired a valid position 3D fix.
        /// </summary>
        public bool PostionIsValid { get; private set; }

        /// <summary>
        /// Gets messages received database.
        /// </summary>
        public MessagesReceivedManagement MessagesReceived { get; }

        /// <summary>
        /// Gets messages to transmit database.
        /// </summary>
        public MessagesToTransmitManagement MessagesToTransmit { get; }

        /// <summary>
        /// Gets last error message from the Tile.
        /// </summary>
        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets event signaling that the Tile is ready for operation.
        /// </summary>
        /// <remarks>
        /// This event indicates that there is communication to/from the Tile.
        /// In case the Tile is powered off or in sleep mode it won't be able to respond to commands, therefore this event won't be immediately signaled.
        /// Despite this, there is nothing preventing the application from using the library. As soon as the Tile is responsive, this event will be signaled and the <see cref="PowerStateChanged"/> will be raised.
        /// </remarks>
        public AutoResetEvent DeviceReady { get; set; } = new AutoResetEvent(false);

        /// <summary>
        /// Gets or sets the rate of unsolicited report messages for date and time.
        /// </summary>
        /// <value>Number of seconds in between each message. Set to 0 to disable.</value>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint DateTimeStatusRate
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.DateTimeStatus.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(new TileCommands.DateTimeStatus(-1).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.DateTimeStatus.Reply)CommandProcessedReply).Rate;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }

            set
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.DateTimeStatus.Command;

                    TileSerialPort.WriteLine(new TileCommands.DateTimeStatus((int)value).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
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
        }

        /// <summary>
        /// Gets or sets the rate of unsolicited report messages for device power state.
        /// </summary>
        /// <value>Number of seconds in between each message. Set to 0 to disable.</value>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint ReceiveTestRate
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.ReceiveTest.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(new TileCommands.ReceiveTest(-1).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.ReceiveTest.Reply)CommandProcessedReply).Rate;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }

            set
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.ReceiveTest.Command;

                    TileSerialPort.WriteLine(new TileCommands.ReceiveTest((int)value).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
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
        }

        /// <summary>
        /// Gets or sets the rate of unsolicited report messages for jamming and spoofing indicators.
        /// </summary>
        /// <value>Number of seconds in between each message. Set to 0 to disable.</value>>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint JammingSpoofingIndicationRate
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GpsJammingSpoofing.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(new TileCommands.GpsJammingSpoofing(-1).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.GpsJammingSpoofing.Reply)CommandProcessedReply).Rate;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }

            set
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GpsJammingSpoofing.Command;

                    TileSerialPort.WriteLine(new TileCommands.GpsJammingSpoofing((int)value).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
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
        }

        /// <summary>
        /// Gets or sets the rate of unsolicited report messages for geospatial information.
        /// </summary>
        /// <value>Number of seconds in between each message. Set to 0 to disable.</value>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GeospatialInformationRate
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GeospatialInfo.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(new TileCommands.GeospatialInfo(-1).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.GeospatialInfo.Reply)CommandProcessedReply).Rate;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }

            set
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GeospatialInfo.Command;

                    TileSerialPort.WriteLine(new TileCommands.GeospatialInfo((int)value).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
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
        }

        /// <summary>
        /// Gets or sets the rate of unsolicited report messages for GPS fix quality.
        /// </summary>
        /// <value>Number of seconds in between each message. Set to 0 to disable.</value>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GpsFixQualityRate
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GpsFixQualityCmd.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(new TileCommands.GpsFixQualityCmd(-1).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.GpsFixQualityCmd.Reply)CommandProcessedReply).Rate;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }

            set
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GpsFixQualityCmd.Command;

                    TileSerialPort.WriteLine(new TileCommands.GpsFixQualityCmd((int)value).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
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
        }

        /// <summary>
        /// Gets or sets the mode for GPIO1 pin.
        /// </summary>
        /// <value>Mode for GPIO1 pin.</value>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public GpioMode SetGpio1Mode
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.Gpio1Control.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(TileCommands.Gpio1Control.GetMode().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.Gpio1Control.Reply)CommandProcessedReply).Mode;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }

            set
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.Gpio1Control.Command;

                    TileSerialPort.WriteLine(new TileCommands.Gpio1Control(value).ComposeToSend().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
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
        }

        /// <summary>
        /// Gets the current <see cref="DateTimeInfo"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public DateTimeInfo DateTimeStatus
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.DateTimeStatus.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(TileCommands.DateTimeStatus.GetLast().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.DateTimeStatus.Reply)CommandProcessedReply).DateTimeInfo;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current <see cref="JammingSpoofingIndication"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public JammingSpoofingIndication JammingSpoofingIndication
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GpsJammingSpoofing.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(TileCommands.GpsJammingSpoofing.GetLast().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.GpsJammingSpoofing.Reply)CommandProcessedReply).Indication;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current <see cref="GeospatialInformation"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public GeospatialInformation GeospatialInformation
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GeospatialInfo.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(TileCommands.GeospatialInfo.GetLast().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.GeospatialInfo.Reply)CommandProcessedReply).Information;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current <see cref="GpsFixQuality"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public GpsFixQuality GpsFixQuality
        {
            get
            {
                lock (CommandLock)
                {
                    // reset error flag
                    ErrorOccurredWhenProcessingCommand = false;

                    // reset event
                    CommandProcessed.Reset();

                    // store command
                    CommandInExecution = TileCommands.GpsFixQualityCmd.Command;

                    // send the command with -1 to get the current setting
                    TileSerialPort.WriteLine(TileCommands.GpsFixQualityCmd.GetLast().ToString());

                    // wait from command to be processed
                    var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                    // clear command
                    CommandInExecution = string.Empty;

                    if (eventSignaled)
                    {
                        // check for error
                        if (ErrorOccurredWhenProcessingCommand)
                        {
                            throw new ErrorExecutingCommandException();
                        }
                        else
                        {
                            return ((TileCommands.GpsFixQualityCmd.Reply)CommandProcessedReply).Information;
                        }
                    }
                    else
                    {
                        throw new TimeoutException();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwarmTile"/> class.
        /// </summary>
        /// <param name="portName">The port where the Swarm Tile is connected to (for example, COM1).</param>
        public SwarmTile(string portName)
        {
            // configure SerialPort and...
            TileSerialPort = new SerialPort(portName, 115200);

            ////... try opening it
            // failure to open the SerialPort will throw an exception
            TileSerialPort.Open();

            // set new line char
            TileSerialPort.NewLine = "\n";

            // set watch char: LF
            TileSerialPort.WatchChar = '\n';

            // setup event handler for receive buffer
            TileSerialPort.DataReceived += Tile_DataReceived;

            // incoming NMEA sentences worker thread
            _processIncommingSentencesThread = new Thread(ProcessIncommingSentencesWorkerThread);
            _processIncommingSentencesThread.Start();

            // start thread to get general details
            var getDetailsThread = new Thread(GetGeneralDetailsThread);
            getDetailsThread.Start();

            // create accessors to messages received and to transmit databases
            MessagesReceived = new MessagesReceivedManagement(this);
            MessagesToTransmit = new MessagesToTransmitManagement(this);
        }

        /// <summary>
        /// Close connection to Swarm Tile.
        /// </summary>
        public void Close()
        {
            TileSerialPort?.Close();
        }

        private void Tile_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // discard event if there is nothing to read or if this is not a WatchChar event
            // meaning that there isn't yet a full NMEA sentence to read in the buffer
            if (e.EventType != SerialData.WatchChar
                || TileSerialPort.BytesToRead == 0)
            {
                return;
            }

#if DEBUG

            // Debug.WriteLine($"chars ava1>>{TileSerialPort.BytesToRead}");
            var receivedMessage = TileSerialPort.ReadLine();
            Debug.WriteLine($">>{receivedMessage}");

            //// TODO: is this dead code to be removed?
            //// Debug.WriteLine($"chars ava2>>{TileSerialPort.BytesToRead}");
#else

            // read line
            var receivedMessage = TileSerialPort.ReadLine();

#endif
            // skip empty messages
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                _incommingMessagesQueue.Enqueue(receivedMessage);

                // signal event
                _messageReceived.Set();

                // check if this is the very 1st message received
                if (_isFirstMessage)
                {
                    // reset flag
                    _isFirstMessage = false;

                    // seems that the Tile is ON
                    PowerState = PowerState.On;
                }
            }
        }

        /// <summary>
        /// The thread used to process asynchronously incoming NMEA sentences.
        /// </summary>
        private void ProcessIncommingSentencesWorkerThread()
        {
            while (true)
            {
                _messageReceived.WaitOne();

                while (_incommingMessagesQueue.Count > 0)
                {
                    lock (_lock)
                    {
                        var receivedMessage = _incommingMessagesQueue.Dequeue() as string;

                        var nmeaSentence = NmeaSentence.FromRawSentence(receivedMessage);

                        if (nmeaSentence != null)
                        {
                            ProcessIncommingSentence(nmeaSentence);
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

        private void ProcessIncommingSentence(NmeaSentence nmeaSentence)
        {
            var prefix = nmeaSentence.Data.Substring(0, 2);

            bool signalCommandEvent = false;

            switch (prefix)
            {
                case TileCommands.DateTimeStatus.Command:
                    var dtStatus = new TileCommands.DateTimeStatus.Reply(nmeaSentence);

                    if (dtStatus.DateTimeInfo != null)
                    {
                        // update property, just in case
                        DateTimeIsValid = true;

                        // reply it's the RT rate, store
                        CommandProcessedReply = dtStatus;

                        signalCommandEvent = true;
                    }
                    else if (dtStatus.Rate > uint.MinValue)
                    {
                        // reply it's the DT rate, store
                        CommandProcessedReply = dtStatus;

                        // raise event for DateTimeInfo available on a thread
                        new Thread(() => { OnDateTimeStatusAvailable(dtStatus.DateTimeInfo); }).Start();

                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        signalCommandEvent = true;
                    }

                    break;

                case TileCommands.GpsJammingSpoofing.Command:
                    var jsIndication = new TileCommands.GpsJammingSpoofing.Reply(nmeaSentence);

                    if (jsIndication.Indication != null)
                    {
                        // this reply it's a GJ indication, store
                        CommandProcessedReply = jsIndication;

                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        signalCommandEvent = true;
                    }

                    break;

                case TileCommands.GeospatialInfo.Command:
                    var geoInfo = new TileCommands.GeospatialInfo.Reply(nmeaSentence);

                    if (geoInfo.Information != null)
                    {
                        // update property, just in case
                        PostionIsValid = true;

                        // this reply it's a GN information, store
                        CommandProcessedReply = geoInfo;

                        // raise event for GeospatialInfo available on a thread
                        new Thread(() => { OnGeospatialInfoAvailable(geoInfo.Information); }).Start();

                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        signalCommandEvent = true;
                    }

                    break;

                case TileCommands.GpsFixQualityCmd.Command:
                    var gpsFixInfo = new TileCommands.GpsFixQualityCmd.Reply(nmeaSentence);

                    if (gpsFixInfo.Information != null)
                    {
                        // this reply it's a GS, store
                        CommandProcessedReply = gpsFixInfo;

                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        signalCommandEvent = true;
                    }

                    break;

                case TileCommands.Gpio1Control.Command:
                    var gpioMode = new TileCommands.Gpio1Control.Reply(nmeaSentence);

                    if (gpioMode.Mode != GpioMode.Unknwon)
                    {
                        // this reply it's a GP, store
                        CommandProcessedReply = gpioMode;

                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        signalCommandEvent = true;
                    }

                    break;

                case TileCommands.ReceiveTest.Command:
                    var receiveTest = new TileCommands.ReceiveTest.Reply(nmeaSentence);

                    if (receiveTest.BackgroundRssi > int.MinValue)
                    {
                        // this reply it's a RT with background noise info
                        BackgroundNoiseRssi = receiveTest.BackgroundRssi;

                        // raise event for background noise info available
                        new Thread(() => { OnBackgroundNoiseInfoAvailable(receiveTest.BackgroundRssi); }).Start();
                    }
                    else if (receiveTest.Rate > uint.MinValue)
                    {
                        // reply it's the RT rate, store
                        CommandProcessedReply = receiveTest;

                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        signalCommandEvent = true;
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

                case TileCommands.ConfigurationSettings.Command:
                    var configDetails = new TileCommands.ConfigurationSettings.Reply(nmeaSentence);

                    if (!string.IsNullOrEmpty(configDetails.DeviceId))
                    {
                        DeviceID = configDetails.DeviceId;
                    }

                    if (!string.IsNullOrEmpty(configDetails.DeviceName))
                    {
                        DeviceName = configDetails.DeviceName;
                    }

                    // if all goes as expected, by now we should have fw details and device IDs
                    if (!(string.IsNullOrEmpty(FirmwareVersion)
                        || string.IsNullOrEmpty(FirmwareTimeStamp)
                        || string.IsNullOrEmpty(DeviceID)
                        || string.IsNullOrEmpty(DeviceName)))
                    {
                        // signal event that device is operational
                        DeviceReady.Set();
                    }

                    break;

                case TileCommands.TransmitData.Command:
                    var txData = new TileCommands.TransmitData.Reply(nmeaSentence);

                    if (txData.MessageId != null)
                    {
                        // store reply
                        CommandProcessedReply = txData;

                        signalCommandEvent = true;
                    }
                    else if (txData.ErrorMessage != null)
                    {
                        // store reply, which contains the error
                        CommandProcessedReply = txData;

                        // set error flag 
                        ErrorOccurredWhenProcessingCommand = true;

                        signalCommandEvent = true;
                    }
                    else if (txData.Event == Swarm.MessageEvent.Received)
                    {
                        // $TD SENT RSSI=<rssi_sat>,SNR=<snr>,FDEV=<fdev>,<msg_id>*xx
                        //          |              |         |           |       |
                        //          7                     
                        int startIndex = 7;
                        ProcessMessageReceivedEvent(nmeaSentence.Data.Substring(startIndex));
                    }
                    else if (txData.Event == Swarm.MessageEvent.Expired)
                    {
                        // raise event for message expired on a thread
                        new Thread(() => { OnMessageEvent(Swarm.MessageEvent.Expired, txData.MessageId); }).Start();
                    }

                    break;

                case TileCommands.MessagesReceivedManagement.Command:
                    MessagesReceived.ProcessIncomingSentence(nmeaSentence);
                    break;

                case TileCommands.MessagesToTransmitManagement.Command:
                    MessagesToTransmit.ProcessIncomingSentence(nmeaSentence);
                    break;

                default:
                    // start checking OK and ERROR
                    // check for OK...
                    if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // we're good
                        signalCommandEvent = true;
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptErrorReply))
                    {
                        //// ... ERROR messages
                        // set error flag 
                        ErrorOccurredWhenProcessingCommand = true;

                        signalCommandEvent = true;
                    }
                    else if (ProcessKnownPrompts(nmeaSentence))
                    {
                        //// Nothing to process
                    }
                    else
                    {
                        // unknown message
                        Debug.WriteLine($"Unknown message NOT processed: {nmeaSentence.Data}");
                    }

                    break;
            }

            if (signalCommandEvent
                && prefix == CommandInExecution)
            {
                // flag any command waiting for processing
                CommandProcessed.Set();
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
                // ignore any exceptions that occur during processing
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
                if (nmeaSentence.Data.Contains(",VERSION"))
                {
                    // TILE BOOT,VERSION,2021-07-21-23:19:41,v1.1.0
                    //                   |
                    //                   18 
                    int startIndex = 18;

                    // get version now
                    var fwInfo = nmeaSentence.Data.Substring(startIndex).Split(',');

                    FirmwareTimeStamp = fwInfo[0];
                    FirmwareVersion = fwInfo[1];

                    // start thread to get general details
                    var getDetailsThread = new Thread(GetGeneralDetailsThread);
                    getDetailsThread.Start();
                }
                else if (nmeaSentence.Data.Contains(",RUNNING"))
                {
                    // update power state
                    PowerState = PowerState.On;
                }
                else if (nmeaSentence.Data.Contains("DATETIME"))
                {
                    // update property
                    DateTimeIsValid = true;

                    // raise event for new tile status on a thread
                    new Thread(() => { OnTileStatusEvent(TileStatus.DateTimeAvailable); }).Start();
                }
                else if (nmeaSentence.Data.Contains("POSITION"))
                {
                    // update property
                    PostionIsValid = true;

                    // raise event for new tile status on a thread
                    new Thread(() => { OnTileStatusEvent(TileStatus.PositionAvailable); }).Start();
                }
                else if (nmeaSentence.Data.Contains("ERROR"))
                {
                    // $TILE <msg>,[<data>]*xx
                    //       |             |
                    //       5

                    // extract error message
                    var errorMessage = nmeaSentence.Data.Substring(5).Split(',');

                    LastErrorMessage = errorMessage[1];

                    // raise event for new tile status on a thread
                    new Thread(() => { OnTileStatusEvent(TileStatus.Error); }).Start();
                }
                else
                {
                    Debug.WriteLine(nmeaSentence.Data);
                }

                return true;
            }

            return false;
        }

        private void GetGeneralDetailsThread()
        {
            if (string.IsNullOrEmpty(FirmwareVersion) || string.IsNullOrEmpty(FirmwareTimeStamp))
            {
                TileSerialPort.WriteLine(new TileCommands.RetreiveFirmwareVersion().ComposeToSend().ToString());
            }

            if (string.IsNullOrEmpty(DeviceID) || string.IsNullOrEmpty(DeviceName))
            {
                TileSerialPort.WriteLine(new TileCommands.ConfigurationSettings().ComposeToSend().ToString());
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
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // store command
                CommandInExecution = TileCommands.PowerOff.Command;

                TileSerialPort.WriteLine(new TileCommands.PowerOff().ComposeToSend().ToString());

                // wait from command to be processed
                var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                // clear command
                CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (ErrorOccurredWhenProcessingCommand)
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
        /// Perform a software cold restart of the device.
        /// </summary>
        public void RestartDevice()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // store command
                CommandInExecution = TileCommands.RestartDevice.Command;

                TileSerialPort.WriteLine(new TileCommands.RestartDevice().ComposeToSend().ToString());

                // wait from command to be processed
                var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                // clear command
                CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (ErrorOccurredWhenProcessingCommand)
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
        /// Puts the device into a low-power sleep mode.
        /// </summary>
        /// <param name="value">Sleep for this many seconds.</param>
        /// <exception cref="ArgumentException">If rate is &lt; 5.</exception>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SendToSleep(uint value)
        {
            if (value < 1)
            {
                throw new ArgumentException();
            }

            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // store command
                CommandInExecution = TileCommands.SleepMode.Command;

                TileSerialPort.WriteLine(new TileCommands.SleepMode(value).ComposeToSend().ToString());

                // wait from command to be processed
                var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                // clear command
                CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (ErrorOccurredWhenProcessingCommand)
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
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SendToSleep(DateTime wakeupTime)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // store command
                CommandInExecution = TileCommands.SleepMode.Command;

                TileSerialPort.WriteLine(new TileCommands.SleepMode(wakeupTime).ComposeToSend().ToString());

                // wait from command to be processed
                var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                // clear command
                CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (ErrorOccurredWhenProcessingCommand)
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
        /// Tries to send a message to the Swarm network.
        /// </summary>
        /// <param name="message">The message with the data to be transmitted.</param>
        /// <param name="messageId">The ID assigned to this message.</param>
        /// <returns><see langword="true"/> if the message has been successfully added to the queue for transmission, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        /// <remarks>
        /// Sending messages it's only possible if the Tile has a valid date and time information. That can be checked with <see cref="DateTimeIsValid"/>.
        /// In case of failure, the error is stored in the <see cref="LastErrorMessage"/> property.
        /// </remarks>
        public bool TryToSendMessage(MessageToTransmit message, out string messageId)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // store command
                CommandInExecution = TileCommands.TransmitData.Command;

                TileSerialPort.WriteLine(new TileCommands.TransmitData(message).ComposeToSend().ToString());

                // wait from command to be processed
                var eventSignaled = CommandProcessed.WaitOne(TimeoutForCommandExecution, false);

                // clear command
                CommandInExecution = string.Empty;

                if (eventSignaled)
                {
                    // check for error
                    if (ErrorOccurredWhenProcessingCommand)
                    {
                        // update the error message
                        LastErrorMessage = ((TileCommands.TransmitData.Reply)CommandProcessedReply).ErrorMessage;

                        messageId = string.Empty;

                        return false;
                    }
                    else
                    {
                        // update message ID
                        messageId = ((TileCommands.TransmitData.Reply)CommandProcessedReply).MessageId;

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

        #region Power State handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="PowerStateChanged"/> event.
        /// </summary>
        /// <param name="powerState">New power status of the device.</param>
        public delegate void PowerStateChangedHandler(PowerState powerState);

        /// <summary>
        /// Event raised when the power state of the device changes.
        /// </summary>
        public event PowerStateChangedHandler PowerStateChanged;

        /// <summary>
        /// Raises the <see cref="PowerStateChanged"/> event.
        /// </summary>
        /// <param name="powerStatus">New power status of the device.</param>
        protected void OnPowerStateChanged(PowerState powerStatus)
        {
            PowerStateChanged?.Invoke(powerStatus);
        }

        #endregion

        #region Message events

        /// <summary>
        /// Represents the delegate used for the <see cref="MessageEvent"/> event.
        /// </summary>
        /// <param name="messageEvent">Event occurred about a message.</param>
        /// <param name="messageId">Id of message the event is related with.</param>
        public delegate void MessageEventHandler(MessageEvent messageEvent, string messageId);

        /// <summary>
        /// Event raised related with a message.
        /// </summary>
        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// Raises the <see cref="MessageEvent"/> event.
        /// </summary>
        /// <param name="messageEvent">Event occurred about a message.</param>
        /// <param name="messageId">Id of message the event is related with.</param>
        protected void OnMessageEvent(MessageEvent messageEvent, string messageId)
        {
            MessageEvent?.Invoke(messageEvent, messageId);
        }

        #endregion

        #region Date time handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="DateTimeStatusAvailable"/> event.
        /// </summary>
        /// <param name="dateTimeInfo">Data available.</param>
        public delegate void DateTimeStatusHandler(DateTimeInfo dateTimeInfo);

        /// <summary>
        /// Event raised when there is an updated <see cref="DateTimeInfo"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="DateTimeStatusRate"/> property to configure the rate that this event is generated.
        /// </remarks>
        public event DateTimeStatusHandler DateTimeStatusAvailable;

        /// <summary>
        /// Raises the <see cref="DateTimeStatusAvailable"/> event.
        /// </summary>
        /// <param name="dateTimeInfo">Updated data.</param>
        protected void OnDateTimeStatusAvailable(DateTimeInfo dateTimeInfo)
        {
            DateTimeStatusAvailable?.Invoke(dateTimeInfo);
        }

        #endregion

        #region Background noise info handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="BackgroundNoiseInfoAvailable"/> event.
        /// </summary>
        /// <param name="rssi">Value for background noise RSSI.</param>
        public delegate void BackgroundNoiseInfoHandler(int rssi);

        /// <summary>
        /// Event raised when there is a new reading of background noise RSSI.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ReceiveTestRate"/> property to configure the rate that this event is generated.
        /// </remarks>
        public event BackgroundNoiseInfoHandler BackgroundNoiseInfoAvailable;

        /// <summary>
        /// Raises the <see cref="DateTimeStatusAvailable"/> event.
        /// </summary>
        /// <param name="rssi">Value for background noise RSSI.</param>
        protected void OnBackgroundNoiseInfoAvailable(int rssi)
        {
            BackgroundNoiseInfoAvailable?.Invoke(rssi);
        }

        #endregion

        #region GeospatialInfo handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="GeospatialInfoAvailable"/> event.
        /// </summary>
        /// <param name="geoSpatialInfo">Data available.</param>
        public delegate void GeospatialInfoHandler(GeospatialInformation geoSpatialInfo);

        /// <summary>
        /// Event raised when there is an updated <see cref="GeospatialInformation"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="GeospatialInformationRate"/> property to configure the rate that this event is generated.
        /// </remarks>
        public event GeospatialInfoHandler GeospatialInfoAvailable;

        /// <summary>
        /// Raises the <see cref="GeospatialInfoAvailable"/> event.
        /// </summary>
        /// <param name="geoSpatialInfo">Updated data.</param>.
        protected void OnGeospatialInfoAvailable(GeospatialInformation geoSpatialInfo)
        {
            GeospatialInfoAvailable?.Invoke(geoSpatialInfo);
        }

        #endregion

        #region Status message events

        /// <summary>
        /// Represents the delegate used for the <see cref="TileStatusEvent"/> event.
        /// </summary>
        /// <param name="status">Tile status.</param>
        public delegate void TileStatusEventHandler(TileStatus status);

        /// <summary>
        /// Event raised when there is a new Tile Status.
        /// </summary>
        public event TileStatusEventHandler TileStatusEvent;

        /// <summary>
        /// Raises the <see cref="TileStatusEvent"/> event.
        /// </summary>
        /// <param name="status">Event occurred about a message.</param>
        protected void OnTileStatusEvent(TileStatus status)
        {
            TileStatusEvent?.Invoke(status);
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Finalizes an instance of the <see cref="SwarmTile"/> class.
        /// </summary>
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
                    _processIncommingSentencesThread.Abort();
                    TileSerialPort.Dispose();
                }
                catch
                {
                    // don't care
                }
            }

            _disposed = true;
        }

        #endregion
    }
}