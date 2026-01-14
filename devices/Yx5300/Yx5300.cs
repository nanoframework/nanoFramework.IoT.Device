// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*
MD_YX5300 - Library for YX5300 Serial MP3 module
Converted from C++ to C# with original code: MD_YX5300 by MajicDesigns for Arduino. MajicDesigns allowed the conversion.
See example module specifications: https://wiki.keyestudio.com/KS0387_keyestudio_YX5200-24SS_MP3_Module
*/

using System;
using System.IO.Ports;
using System.Threading;

namespace Iot.Device.Yx5300
{
    /// <summary>
    /// Yx5300 - MP3 Player.
    /// </summary>
    public class Yx5300
    {
        // Serial port
        private const int SerialBaud = 9600;
        private const int TimeoutMiliseconds = 1000;

        // Protocol Message Characters
        private const byte PktSom = 0x7e;       // Start of message delimiter character
        private const byte PktVer = 0xff;       // Version information
        private const byte PktLen = 0x06;       // Data packet length in bytes (excluding SOM, EOM)
        private const byte PktCmdDummy = 0x00;    // Command placeholder
        private const byte PktFbOff = 0x00;    // Command feedback OFF
        private const byte PktFbOn = 0x01;     // Command feedback ON
        private const byte PktDataNull = 0x00;  // Packet data place marker 
        private const byte PKtEom = 0xef;       // End of message delimiter character

        // Command options
        private const byte CmdOptOn = 0x00;    // On indicator
        private const byte CmdOptOff = 0x01;   // Off indicator
        private const byte CmdOptDevUdisk = 0X01; // Device option UDisk (not used)
        private const byte CmdOptDevTf = 0X02;    // Device option TF
        private const byte CmdOptDevFlash = 0X04; // Device option Flash (not used)

        private readonly byte[] msg = new byte[] 
        {
                PktSom,      // 0: Start
                PktVer,      // 1: Version
                PktLen,      // 2: Length
                PktCmdDummy,         // 3: Command placeholder
                PktFbOn,    // 4: Feedback
                PktDataNull, // 5: Data Hi
                PktDataNull, // 6: Data Lo
                PktDataNull, // [7]: Checksum Hi (optional)
                PktDataNull, // [8]: Checksum Lo (optional)
                PKtEom       // 7, [9]: End
        };

        /// <summary>
        /// Maximum Volume.
        /// </summary>
        public const int MaxVolume = 30;

        private enum CommandSet
        {
            CMD_NUL = 0x00,              // No command
            CMD_NEXT_SONG = 0x01,        // Play next song
            CMD_PREV_SONG = 0x02,        // Play previous song
            CMD_PLAY_WITH_INDEX = 0x03,  // Play song with index number
            CMD_VOLUME_UP = 0x04,        // Volume increase by one
            CMD_VOLUME_DOWN = 0x05,      // Volume decrease by one
            CMD_SET_VOLUME = 0x06,       // Set the volume to level specified
            CMD_SET_EQUALIZER = 0x07,    // Set the equalizer to specified level
            CMD_SNG_CYCL_PLAY = 0x08,    // Loop play (repeat) specified track
            CMD_SEL_DEV = 0x09,          // Select storage device to TF card
            CMD_SLEEP_MODE = 0x0a,       // Chip enters sleep mode
            CMD_WAKE_UP = 0x0b,          // Chip wakes up from sleep mode
            CMD_RESET = 0x0c,            // Chip reset
            CMD_PLAY = 0x0d,             // Playback restart
            CMD_PAUSE = 0x0e,            // Playback is paused
            CMD_PLAY_FOLDER_FILE = 0x0f, // Play the song with the specified folder and index number
            CMD_STOP_PLAY = 0x16,        // Playback is stopped
            CMD_FOLDER_CYCLE = 0x17,     // Loop playback from specified folder
            CMD_SHUFFLE_PLAY = 0x18,     // Playback shuffle mode
            CMD_SET_SNGL_CYCL = 0x19,    // Set loop play (repeat) on/off for current file
            CMD_SET_DAC = 0x1a,          // DAC on/off control
            CMD_PLAY_W_VOL = 0x22,       // Play track at the specified volume
            CMD_SHUFFLE_FOLDER = 0x28,   // Playback shuffle mode for folder specified
            CMD_QUERY_STATUS = 0x42,     // Query Device Status
            CMD_QUERY_VOLUME = 0x43,     // Query Volume level
            CMD_QUERY_EQUALIZER = 0x44,  // Query current equalizer (disabled in hardware)
            CMD_QUERY_TOT_FILES = 0x48,  // Query total files in all folders
            CMD_QUERY_PLAYING = 0x4c,    // Query which track playing
            CMD_QUERY_FLDR_FILES = 0x4e, // Query total files in folder
            CMD_QUERY_TOT_FLDR = 0x4f,   // Query number of folders
        }

        /// <summary>
        /// Status Code.
        /// </summary>
        public enum StatusCode
        {
            /// <summary>No error (library generated status).</summary>
            NoError = 0x00,

            /// <summary>Timeout on response message (library generated status).</summary>
            Timeout = 0x01,

            /// <summary>Wrong version number in return message (library generated status).</summary>
            Version = 0x02,

            /// <summary>Device checksum invalid (library generated status).</summary>
            Checksum = 0x03,

            /// <summary>TF Card was inserted (unsolicited).</summary>
            CardInserted = 0x3a,

            /// <summary>TF card was removed (unsolicited).</summary>
            CardRemoved = 0x3b,

            /// <summary>Track/file has ended (unsolicited).</summary>
            EndOfFile = 0x3d,

            /// <summary>Initialization complete (unsolicited).</summary>
            InitializationComplete = 0x3f,

            /// <summary>Error file not found.</summary>
            FileNotFound = 0x40,

            /// <summary>Message acknowledged ok.</summary>
            AcknoledgeOk = 0x41,

            /// <summary>Current status.</summary>
            Status = 0x42,

            /// <summary>Current volume level.</summary>
            Volume = 0x43,

            /// <summary>Equalizer status.</summary>
            Equalizer = 0x44,

            /// <summary>TF Total file count.</summary>
            TotalFileCount = 0x48,

            /// <summary>Current file playing.</summary>
            Playing = 0x4c,

            /// <summary>Total number of files in the folder.</summary>
            NumberOfFilesInFolder = 0x4e,

            /// <summary>Total number of folders.</summary>
            TotalNumberOfFiles = 0x4f
        }

        /// <summary>
        /// Class containing status data.
        /// </summary>
        public class Status
        {
            /// <summary>
            /// Gets or sets status Code.
            /// </summary>
            public StatusCode Code { get; set; }

            /// <summary>
            /// Gets or sets associated data.
            /// </summary>
            public ushort Data { get; set; }
        }

        private Status _status = new Status();
        private SerialPort _serialPort;

        private byte[] _bufRx = new byte[30]; // receive buffer for serial comms
        private byte _bufIdx;    // index for next char into _bufIdx
        private DateTime _timeSent; // time last serial message was sent
        private bool _waitResponse; // true when we are waiting response to a query
        private int _timeoutDurationInMs = TimeoutMiliseconds;

        /// <summary>
        /// Initializes a new instance of the<see cref="Yx5300" /> class.
        /// </summary>
        /// <param name="portName">The serial port name. eg COM2.</param>
        public Yx5300(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = SerialBaud;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.DataBits = 8;
            _serialPort.WriteTimeout = TimeoutMiliseconds;
            _serialPort.ReadTimeout = TimeoutMiliseconds;
            _serialPort.Open();
            Begin();
        }

        private void Begin()
        {
            // set the TF card system.
            // The synchronous call will return when the command is accepted
            // then it will be followed by an initialization message saying TF card is inserted.
            // Doc says this should be 200ms, so we set a timeout for 1000ms.
            Device(CmdOptDevTf); // set the TF card file system
            _timeSent = DateTime.UtcNow;
            while (!Check())
            {
                if ((DateTime.UtcNow - _timeSent).TotalMilliseconds >= 1000)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true when received full message or timeout.
        /// </summary>
        /// <returns>True if success.</returns>
        private bool Check()
        {
            byte c = 0x00;

            // check for timeout if waiting response
            var currentWaitDuration = (DateTime.UtcNow - _timeSent).TotalMilliseconds;
            if (_waitResponse && (currentWaitDuration >= _timeoutDurationInMs))
            {
                ProcessResponse(true);
                return true;
            }

            // check if any characters available
            var bytesRead = _serialPort.BytesToRead;
            if (bytesRead == 0)
            {
                return false;
            }

            // process all the characters waiting
            do
            {
                c = (byte)_serialPort.ReadByte();
                if (c == PktSom)
                {
                    _bufIdx = 0;      // start of message - reset the index
                }

                _bufRx[_bufIdx++] = c;

                // keep index within array memory bounds
                if (_bufIdx >= _bufRx.Length)  
                {
                    _bufIdx = (byte)(_bufRx.Length - 1);
                }

                bytesRead--;
            } 
            while (bytesRead > 0 && c != PKtEom);

            // check if we have a whole message to 
            // process and do something with it here!
            if (c == PKtEom)
            {
                ProcessResponse();
            }

            return c == PKtEom;   // we have just processed a response
        }

        /// <summary>
        /// Sets the timeout in seconds.
        /// </summary>
        /// <param name="timeoutInSec">Timeout in seconds.</param>
        public void SetTimeout(int timeoutInSec)
        {
            _timeoutDurationInMs = timeoutInSec * 1000;
        }

        /// <summary>
        /// Gets the latest status.
        /// </summary>
        /// <returns>The status.</returns>
        public Status GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// Gets the latest status code.
        /// </summary>
        /// <returns>The status code.</returns>
        public StatusCode GetStatusCode()
        {
            return _status.Code;
        }

        /// <summary>
        /// Gets the latest data status.
        /// </summary>
        /// <returns>The latest data status.</returns>
        public ushort GetStatusData()
        {
            return _status.Data;
        }

        private bool Device(byte devId)
        {
            return SendRequest(CommandSet.CMD_SEL_DEV, PktDataNull, devId);
        }

        /// <summary>
        /// Set the equalizer.
        /// </summary>
        /// <param name="eqId">Id of the equalizer.</param>
        /// <returns>True if success.</returns>
        public bool Equalizer(int eqId)
        {
            return SendRequest(CommandSet.CMD_SET_EQUALIZER, PktDataNull, (byte)(eqId > 5 ? 0 : eqId));
        }

        internal bool Sleep(int eqId)
        {
            return SendRequest(CommandSet.CMD_SLEEP_MODE, PktDataNull, PktDataNull);
        }

        internal bool WakeUp(int eqId)
        {
            return SendRequest(CommandSet.CMD_WAKE_UP, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Shuffles the play.
        /// </summary>
        /// <param name="isShuffled">True to shuffle.</param>
        /// <returns>True if success.</returns>
        public bool Shuffle(bool isShuffled)
        {
            return SendRequest(CommandSet.CMD_SHUFFLE_PLAY, PktDataNull, isShuffled ? CmdOptOn : CmdOptOff);
        }

        /// <summary>
        /// Reset player settings.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool Reset()
        {
            int cachedTimeout = _timeoutDurationInMs;
            _timeoutDurationInMs = 2000;  // initialization timeout needs to be a long one

            var response = SendRequest(CommandSet.CMD_RESET, PktDataNull, PktDataNull);  // long timeout on this message

            _timeoutDurationInMs = cachedTimeout;  // put back saved value

            return response;
        }

        /// <summary>
        /// Plays the next file.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool PlayNext()
        {
            return SendRequest(CommandSet.CMD_NEXT_SONG, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Plays the previous file.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool PlayPrev()
        {
            return SendRequest(CommandSet.CMD_PREV_SONG, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Stops playing.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool Stop()
        {
            return SendRequest(CommandSet.CMD_STOP_PLAY, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Pauses playing.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool Pause()
        {
            return SendRequest(CommandSet.CMD_PAUSE, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Plays playing.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool Play()
        {
            return SendRequest(CommandSet.CMD_PLAY, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Plays a track.
        /// </summary>
        /// <param name="trackNum">The track number to play.</param>
        /// <returns>True if success.</returns>
        public bool PlayTrack(int trackNum)
        {
            return SendRequest(CommandSet.CMD_PLAY_WITH_INDEX, PktDataNull, (byte)trackNum);
        }

        /// <summary>
        /// Plays a file.
        /// </summary>
        /// <param name="fileNum">The file number to play.</param>
        /// <returns>True if success.</returns>
        public bool PlayTrackRepeat(int fileNum)
        {
            return SendRequest(CommandSet.CMD_SNG_CYCL_PLAY, PktDataNull, (byte)fileNum);
        }

        /// <summary>
        /// Plays a specific file in a specific folder.
        /// </summary>
        /// <param name="folderNum">The folder number.</param>
        /// <param name="fileNum">The file number.</param>
        /// <returns>True if success.</returns>
        public bool PlaySpecific(int folderNum, int fileNum)
        {
            return SendRequest(CommandSet.CMD_PLAY_FOLDER_FILE, (byte)folderNum, (byte)fileNum);
        }

        /// <summary>
        /// Plays and repeats the play in a specific folder.
        /// </summary>
        /// <param name="folderNum">The folder number.</param>
        /// <returns>True if success.</returns>
        public bool PlayFolderRepeat(int folderNum)
        {
            return SendRequest(CommandSet.CMD_FOLDER_CYCLE, PktDataNull, (byte)folderNum);
        }

        /// <summary>
        /// Plays and shuffles the play in a specific folder.
        /// </summary>
        /// <param name="folderNum">Numer of folder to shuffle.</param>
        /// <returns>True if success.</returns>
        public bool PlayFolderShuffle(int folderNum)
        {
            return SendRequest(CommandSet.CMD_SHUFFLE_FOLDER, PktDataNull, (byte)folderNum);
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume">The wanted volume from 0 to 30.</param>
        /// <returns>True if success.</returns>
        public bool Volume(int volume)
        {
            return SendRequest(CommandSet.CMD_SET_VOLUME, PktDataNull, (byte)(volume > MaxVolume ? MaxVolume : volume));
        }

        /// <summary>
        /// Gets the maximum Volume.
        /// </summary>
        /// <returns>The maximum volume.</returns>
        public int GetMaxVolume() 
        { 
            return MaxVolume; 
        }

        /// <summary>Increases the volume.
        /// Increases the volume.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool VolumeInc()
        {
            return SendRequest(CommandSet.CMD_VOLUME_UP, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Decreases the volume.
        /// </summary>
        /// <returns>True if success.</returns>
        public bool VolumeDec()
        {
            return SendRequest(CommandSet.CMD_VOLUME_DOWN, PktDataNull, PktDataNull);
        }

        /// <summary>
        /// Mutes or unmutes the volume.
        /// </summary>
        /// <param name="isMute">True to mute, false to unmute.</param>
        /// <returns>True if success.</returns>
        public bool VolumeMute(bool isMute)
        {
            return SendRequest(CommandSet.CMD_SET_DAC, PktDataNull, isMute ? CmdOptOff : CmdOptOn);
        }

        // Low level code
        private ushort CalcCheckSum(Span<byte> data, int len)
        {
            ushort sum = 0;

            for (int i = 0; i < len; i++)
            {
                sum += data[i];
            }

            return (ushort)(-sum);
        }

        private bool SendRequest(CommandSet cmd, byte dataHi, byte dataLo)
        {
            msg[3] = (byte)cmd;
            msg[5] = dataHi;
            msg[6] = dataLo;

            var data = new Span<byte>(msg, 1, msg.Length - 1);
            ushort chk = CalcCheckSum(data, msg[2]);

            msg[7] = (byte)(chk >> 8);
            msg[8] = (byte)(chk & 0x00ff);

            _serialPort.Write(msg, 0, msg.Length);
            _bufIdx = 0;

            Thread.Sleep(20);

            _timeSent = DateTime.UtcNow;
            _status.Code = StatusCode.NoError;
            _waitResponse = true;

            do
            {
                Thread.Sleep(10);
            } 
            while (!Check());

            return true;
        }

        private void ProcessResponse(bool isTimeout = false)
        {
            _waitResponse = false;    // definitely no longer waiting

            Span<byte> data = new Span<byte>(_bufRx, 1, _bufRx.Length - 1);
            ushort chk = CalcCheckSum(data, _bufRx[2]);
            ushort chkRcv = (ushort)((_bufRx[7] << 8) + _bufRx[8]);

            // initialize to most probable message outcome
            _status.Code = (StatusCode)_bufRx[3];
            _status.Data = (ushort)((_bufRx[5] << 8) | _bufRx[6]);

            // now override with message packet errors, if any
            if (isTimeout)
            {
                _status.Code = StatusCode.Timeout;
            }
            else if (_bufRx[1] != PktVer)
            {
                _status.Code = StatusCode.Version;
            }
            else if (chk != chkRcv)
            {
                _status.Code = StatusCode.Checksum;
            }
        }
    }
}
