// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Yx5300
{
    /// <summary>
    /// Yx5300 MP3 player.
    /// </summary>
    public partial class Yx5300
    {
        private enum CommandSet
        {
            // No command
            CMD_NUL = 0x00,

            // Play next song
            CMD_NEXT_SONG = 0x01,

            // Play previous song
            CMD_PREV_SONG = 0x02,

            // Play song with index number
            CMD_PLAY_WITH_INDEX = 0x03,

            // Volume increase by one
            CMD_VOLUME_UP = 0x04,

            // Volume decrease by one
            CMD_VOLUME_DOWN = 0x05,

            // Set the volume to level specified
            CMD_SET_VOLUME = 0x06,

            // Set the equalizer to specified level
            CMD_SET_EQUALIZER = 0x07,

            // Loop play (repeat) specified track
            CMD_SNG_CYCL_PLAY = 0x08,

            // Select storage device to TF card
            CMD_SEL_DEV = 0x09,

            // Chip enters sleep mode
            CMD_SLEEP_MODE = 0x0a,

            // Chip wakes up from sleep mode
            CMD_WAKE_UP = 0x0b,

            // Chip reset
            CMD_RESET = 0x0c,

            // Playback restart
            CMD_PLAY = 0x0d,

            // Playback is paused
            CMD_PAUSE = 0x0e,

            // Play the song with the specified folder and index number
            CMD_PLAY_FOLDER_FILE = 0x0f,

            // Playback is stopped
            CMD_STOP_PLAY = 0x16,

            // Loop playback from specified folder
            CMD_FOLDER_CYCLE = 0x17,

            // Playback shuffle mode
            CMD_SHUFFLE_PLAY = 0x18,

            // Set loop play (repeat) on/off for current file
            CMD_SET_SNGL_CYCL = 0x19,

            // DAC on/off control
            CMD_SET_DAC = 0x1a,

            // Play track at the specified volume
            CMD_PLAY_W_VOL = 0x22,

            // Playback shuffle mode for folder specified
            CMD_SHUFFLE_FOLDER = 0x28,

            // Query device status
            CMD_QUERY_STATUS = 0x42,

            // Query volume level
            CMD_QUERY_VOLUME = 0x43,

            // Query current equalizer (disabled in hardware)
            CMD_QUERY_EQUALIZER = 0x44,

            // Query total files in all folders
            CMD_QUERY_TOT_FILES = 0x48,

            // Query which track playing
            CMD_QUERY_PLAYING = 0x4c,

            // Query total files in folder
            CMD_QUERY_FLDR_FILES = 0x4e,

            // Query number of folders
            CMD_QUERY_TOT_FLDR = 0x4f,
        }
    }
}