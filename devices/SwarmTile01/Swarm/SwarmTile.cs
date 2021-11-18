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
        private readonly object _lock = new object();
        internal readonly object CommandLock = new object();

        internal SerialPort TileSerialPort;

        // event to signal that a new message has been received and placed on the queue
        private readonly AutoResetEvent _messageReceived = new AutoResetEvent(false);

        // event to signal that a new command has been added to the queue for processing
        internal readonly AutoResetEvent CommandProcessed = new AutoResetEvent(false);

        // flag to signal that the very 1st message from the Tile was received
        private bool _isFirstMessage = true;

        // flag to signal that an error has occurred when processing the command
        internal bool ErrorOccurredWhenProcessingCommand = false;

        // variable holding the reply of the last command executed
        internal object CommandProcessedReply;

        private bool _disposed;

        private Thread _processIncommingSentencesThread;


        private readonly Queue _incommingMessagesQueue = new();

        // backing fields 
        internal PowerState _powerState = PowerState.Unknown;

        /// <summary>
        /// Timeout for command execution (in milliseconds).
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
        public string DeviceID { get; private set; }

        /// <summary>
        /// Device type name.
        /// </summary>
        public string DeviceName { get; private set; }

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
        /// Time-stamp of the last packet received.
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
        /// Messages received database.
        /// </summary>
        public MessagesReceivedManagement MessagesReceived { get; }

        /// <summary>
        /// Messages to transmit database.
        /// </summary>
        public MessagesToTransmitManagement MessagesToTransmit { get; }

        /// <summary>
        /// Event signaling that the Tile is ready for operation.
        /// </summary>
        /// <remarks>
        /// This event indicates that there is communication to/from the Tile.
        /// In case the Tile is powered off or in sleep mode it won't be able to respond to commands, therefore this event won't be immediately signaled.
        /// Despite this, there is nothing preventing the application from using the library. As soon as the Tile is responsive, this event will be signaled and the <see cref="PowerStateChanged"/> will be raised.
        /// </remarks>
        public AutoResetEvent DeviceReady = new AutoResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName"></param>
        public SwarmTile(string portName)
        {
            // configure SerialPort and...
            TileSerialPort = new SerialPort(portName, 115200);

            //... try opening it
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

            //Debug.WriteLine($"chars ava1>>{_tileSerialPort.BytesToRead}");

            var receivedMessage = TileSerialPort.ReadLine();
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

                // check if this is the very 1st message received
                if (_isFirstMessage)
                {
                    // reset flag
                    _isFirstMessage = false;

                    // signal event
                    DeviceReady.Set();

                    PowerState = PowerState.On;
                }
            }
        }

        /// <summary>
        /// The thread used to process asynchronously incoming NMEA sentences
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
                        var receivedMessage = (string)_incommingMessagesQueue.Dequeue();

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

            switch (prefix)
            {
                case TileCommands.DateTimeStatus.Command:
                    var dtStatus = new TileCommands.DateTimeStatus.Reply(nmeaSentence);

                    if (dtStatus.DateTimeInfo != null)
                    {
                        // reply it's the RT rate, store
                        CommandProcessedReply = dtStatus;

                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }
                    else if (dtStatus.Rate > uint.MinValue)
                    {
                        // reply it's the DT rate, store
                        CommandProcessedReply = dtStatus;

                        // raise event for DateTimeInfo available on a thread
                        new Thread(() => { OnDateTimeStatusAvailable(dtStatus.DateTimeInfo); }).Start();

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }

                    break;

                case TileCommands.GpsJammingSpoofing.Command:
                    var jsIndication = new TileCommands.GpsJammingSpoofing.Reply(nmeaSentence);

                    if (jsIndication.Indication != null)
                    {
                        // this reply it's a GJ indication, store
                        CommandProcessedReply = jsIndication;

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }

                    break;

                case TileCommands.GeospatialInfo.Command:
                    var geoInfo = new TileCommands.GeospatialInfo.Reply(nmeaSentence);

                    if (geoInfo.Information != null)
                    {
                        // this reply it's a GN information, store
                        CommandProcessedReply = geoInfo;

                        // raise event for GeospatialInfo available on a thread
                        new Thread(() => { OnGeospatialInfoAvailable(geoInfo.Information); }).Start();

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }

                    break;

                case TileCommands.GpsFixQualityCmd.Command:
                    var gpsFixInfo = new TileCommands.GpsFixQualityCmd.Reply(nmeaSentence);

                    if (gpsFixInfo.Information != null)
                    {
                        // this reply it's a GS, store
                        CommandProcessedReply = gpsFixInfo;

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }

                    break;


                case TileCommands.Gpio1Control.Command:
                    var gpioMode = new TileCommands.Gpio1Control.Reply(nmeaSentence);

                    if (gpioMode.Mode != GpioMode.Unknwon)
                    {
                        // this reply it's a GP, store
                        CommandProcessedReply = gpioMode;

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // flag any command waiting for processing
                        CommandProcessed.Set();
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

                        //_dateTimeStatus = dtStatus;
                    }
                    else if (receiveTest.Rate > uint.MinValue)
                    {
                        // reply it's the RT rate, store
                        CommandProcessedReply = receiveTest;

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptOkReply))
                    {
                        // flag any command waiting for processing
                        CommandProcessed.Set();
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
                    break;

                case TileCommands.TransmitData.Command:
                    var txData = new TileCommands.TransmitData.Reply(nmeaSentence);

                    if (txData.MessageId != null)
                    {
                        // store reply
                        CommandProcessedReply = txData;

                        // signal event 
                        CommandProcessed.Set();
                    }
                    else if (txData.ErrorMessage != null)
                    {
                        // store reply, which contains the error
                        CommandProcessedReply = txData;

                        // set error flag 
                        ErrorOccurredWhenProcessingCommand = true;

                        // signal event 
                        CommandProcessed.Set();
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
                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }
                    // ... ERROR messages
                    else if (nmeaSentence.Data.Contains(CommandBase.PromptErrorReply))
                    {
                        // error 
                        // set error flag 
                        ErrorOccurredWhenProcessingCommand = true;

                        // flag any command waiting for processing
                        CommandProcessed.Set();
                    }
                    else if (ProcessKnownPrompts(nmeaSentence))
                    {

                    }
                    else
                    {
                        // unknown message
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
                }
                if (nmeaSentence.Data.Contains(",RUNNING"))
                {

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

                TileSerialPort.WriteLine(new TileCommands.PowerOff().ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

                TileSerialPort.WriteLine(new TileCommands.RestartDevice().ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Set the rate for unsolicited report messages for device power state.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message. Set to 0 to disable.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SetReceiveTestRate(uint rate)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.ReceiveTest((int)rate).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Get the rate for unsolicited report messages for device power state.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GetReceiveTestRate()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(new TileCommands.ReceiveTest(-1).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

                TileSerialPort.WriteLine(new TileCommands.SleepMode(value).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

                TileSerialPort.WriteLine(new TileCommands.SleepMode(wakeupTime).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Transmits data to the Swarm network.
        /// </summary>
        /// <param name="message">The message with the data to be transmitted.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public string TransmitData(MessageToTransmit message)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.TransmitData(message).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
                {
                    // check for error
                    if (ErrorOccurredWhenProcessingCommand)
                    {
                        throw new ErrorExecutingCommandException(((TileCommands.TransmitData.Reply)CommandProcessedReply).ErrorMessage);
                    }
                    else
                    {
                        return ((TileCommands.TransmitData.Reply)CommandProcessedReply).MessageId;
                    }
                }
                else
                {
                    throw new TimeoutException();
                }
            }
        }


        /// <summary>
        /// Set the rate for unsolicited report messages for date and time.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message. Set to 0 to disable.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SetDateTimeStatusRate(uint rate)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.DateTimeStatus((int)rate).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Get the rate for unsolicited report messages for date and time.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GetDateTimeStatusRate()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(new TileCommands.DateTimeStatus(-1).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Get the current <see cref="DateTimeInfo"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public DateTimeInfo GetDateTimeStatus()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(TileCommands.DateTimeStatus.GetLast().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Set the rate for unsolicited report messages for jamming and spoofing indicators.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message. Set to 0 to disable.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SetJammingSpoofingIndicationRate(uint rate)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.GpsJammingSpoofing((int)rate).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Get the rate for unsolicited report messages for jamming and spoofing indicators.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GetJammingSpoofingIndicationRate()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(new TileCommands.GpsJammingSpoofing(-1).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Get the current <see cref="JammingSpoofingIndication"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public JammingSpoofingIndication GetJammingSpoofingIndication()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(TileCommands.GpsJammingSpoofing.GetLast().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Set the rate for unsolicited report messages for geospatial information.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message. Set to 0 to disable.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SetGeospatialInformationRate(uint rate)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.GeospatialInfo((int)rate).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Get the rate for unsolicited report messages for geospatial information.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GetGeospatialInformationRate()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(new TileCommands.GeospatialInfo(-1).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Get the current <see cref="GeospatialInformation"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public GeospatialInformation GetGeospatialInformation()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(TileCommands.GeospatialInfo.GetLast().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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


        /// <summary>
        /// Set the rate for unsolicited report messages for GPS fix quality.
        /// </summary>
        /// <param name="rate">Number of seconds in between each message. Set to 0 to disable.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SetGpsFixQualityRate(uint rate)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.GpsFixQualityCmd((int)rate).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Get the rate for unsolicited report messages for GPS fix quality.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public uint GetGpsFixQualityRate()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(new TileCommands.GpsFixQualityCmd(-1).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Get the current <see cref="GpsFixQuality"/> from the Tile.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public GpsFixQuality GetGpsFixQuality()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(TileCommands.GpsFixQualityCmd.GetLast().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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

        /// <summary>
        /// Set mode for GPIO1 pin.
        /// </summary>
        /// <param name="mode">Mode for GPIO1 pin.</param>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public void SetGpio1Mode(GpioMode mode)
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                TileSerialPort.WriteLine(new TileCommands.Gpio1Control(mode).ComposeToSend().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        /// Set mode for GPIO1 pin.
        /// </summary>
        /// <exception cref="ErrorExecutingCommandException">Tile returned error when executing the command.</exception>
        /// <exception cref="TimeoutException">Timeout occurred when waiting for command execution.</exception>
        public GpioMode GetGpio1Mode()
        {
            lock (CommandLock)
            {
                // reset error flag
                ErrorOccurredWhenProcessingCommand = false;

                // reset event
                CommandProcessed.Reset();

                // send the command with -1 to get the current setting
                TileSerialPort.WriteLine(TileCommands.Gpio1Control.GetMode().ToString());

                // wait from command to be processed
                if (CommandProcessed.WaitOne(TimeoutForCommandExecution, false))
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
        public event PowerStateChangedHandler PowerStateChanged;
        private PowerStateChangedHandler onPowerStateChanged;

        /// <summary>
        /// Raises the <see cref="PowerStateChanged"/> event.
        /// </summary>
        /// <param name="powerStatus"> new power status of the device</param>
        protected void OnPowerStateChanged(PowerState powerStatus)
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
        public event MessageEventHandler MessageEvent;
        private MessageEventHandler onMessageEvent;

        /// <summary>
        /// Raises the <see cref="MessageEvent"/> event.
        /// </summary>
        /// <param name="messageEvent">Event occurred about a message</param>
        /// <param name="messageId">Id of message the event is related with</param>
        protected void OnMessageEvent(MessageEvent messageEvent, string messageId)
        {
            if (onMessageEvent == null) onMessageEvent = new MessageEventHandler(MessageEvent);
            MessageEvent?.Invoke(messageEvent, messageId);
        }

        #endregion

        #region Date time handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="DateTimeStatusAvailable"/> event.
        /// </summary>
        /// <param name="dateTimeInfo"> data available</param>
        public delegate void DateTimeStatusHandler(DateTimeInfo dateTimeInfo);

        /// <summary>
        /// Event raised when there is an updated <see cref="DateTimeInfo"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetDateTimeStatusRate"/> to configure the rate that this event is generated.
        /// </remarks>
        public event DateTimeStatusHandler DateTimeStatusAvailable;
        private DateTimeStatusHandler onDateTimeStatusAvailable;

        /// <summary>
        /// Raises the <see cref="DateTimeStatusAvailable"/> event.
        /// </summary>
        /// Updated <param name="dateTimeInfo"> data.</param>
        protected void OnDateTimeStatusAvailable(DateTimeInfo dateTimeInfo)
        {
            if (onDateTimeStatusAvailable == null)
            {
                onDateTimeStatusAvailable = new DateTimeStatusHandler(DateTimeStatusAvailable);
            }

            DateTimeStatusAvailable?.Invoke(dateTimeInfo);
        }

        #endregion

        #region Background noise info handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="BackgroundNoiseInfoAvailable"/> event.
        /// </summary>
        /// <param name="rssi">Value for background noise RSSI</param>
        public delegate void BackgroundNoiseInfoHandler(int rssi);

        /// <summary>
        /// Event raised when there is a new reading of background noise RSSI.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetReceiveTestRate"/> to configure the rate that this event is generated.
        /// </remarks>
        public event BackgroundNoiseInfoHandler BackgroundNoiseInfoAvailable;
        private BackgroundNoiseInfoHandler onBackgroundNoiseInfoAvailable;

        /// <summary>
        /// Raises the <see cref="DateTimeStatusAvailable"/> event.
        /// </summary>
        /// <param name="rssi">Value for background noise RSSI</param>
        protected void OnBackgroundNoiseInfoAvailable(int rssi)
        {
            if (onBackgroundNoiseInfoAvailable == null)
            {
                onBackgroundNoiseInfoAvailable = new BackgroundNoiseInfoHandler(BackgroundNoiseInfoAvailable);
            }

            BackgroundNoiseInfoAvailable?.Invoke(rssi);
        }

        #endregion

        #region GeospatialInfo handlers and events

        /// <summary>
        /// Represents the delegate used for the <see cref="GeospatialInfoAvailable"/> event.
        /// </summary>
        /// <param name="geoSpatialInfo"> data available</param>
        public delegate void GeospatialInfoHandler(GeospatialInformation geoSpatialInfo);

        /// <summary>
        /// Event raised when there is an updated <see cref="GeospatialInformation"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetGeospatialInformationRate"/> to configure the rate that this event is generated.
        /// </remarks>
        public event GeospatialInfoHandler GeospatialInfoAvailable;
        private GeospatialInfoHandler onGeospatialInfoAvailable;

        /// <summary>
        /// Raises the <see cref="GeospatialInfoAvailable"/> event.
        /// </summary>
        /// Updated <param name="geoSpatialInfo"> data.</param>
        protected void OnGeospatialInfoAvailable(GeospatialInformation geoSpatialInfo)
        {
            if (onGeospatialInfoAvailable == null)
            {
                onGeospatialInfoAvailable = new GeospatialInfoHandler(GeospatialInfoAvailable);
            }

            GeospatialInfoAvailable?.Invoke(geoSpatialInfo);
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