namespace Ld2410.Commands
{
    /// <summary>
    /// List of known command words.
    /// </summary>
    /// <remarks>
    /// The LD2410 operates in Little-Endian.
    /// </remarks>
    internal enum CommandWord : ushort
    {
        /// <summary>
        /// This command sets the LD2410 in configuration mode. 
        /// All commands must be issued after this one is executed, otherwise it will be invalid.
        /// </summary>
        EnableConfiguration = 0x00FF,

        /// <summary>
        /// This command ends the configuration command. The radar will resume in working mode after execution.
        /// If another command needs to be issues, <see cref="EnableConfiguration"/> must be sent first.
        /// </summary>
        EndConfiguration = 0x00FE,

        /// <summary>
        /// This command sets the maximum detection gate range (moving &amp; stationary), and the parameter of "No One" duration.
        /// The configuration value for this command is not lost after a power cycle.
        /// </summary>
        SetMaxDistanceGateAndNoOneDuration = 0x0060,

        /// <summary>
        /// This command reads the current configurations stored on the radar.
        /// </summary>
        ReadConfigurations = 0x0061,

        /// <summary>
        /// This command turns on the radar engineering mode. After the engineering mode is turned on, the energy value of each range gate
        /// will be added to the data reported by the radarr.
        /// Engineering mode does not survive a power cycle.
        /// </summary>
        EnableEngineeringMode = 0x0062,

        /// <summary>
        /// This command turns off the radar engineering mode.
        /// </summary>
        EndEngineeringMode = 0x0063,

        /// <summary>
        /// This command configures the sensitivity of the distance gates. It can configure a single gate or all gates simultaneously.
        /// The specified values will survive a power cycle.
        /// </summary>
        ConfigureGateSensitivity = 0x0064,

        /// <summary>
        /// This command reads the radar firmware version information.
        /// </summary>
        ReadFirmwareVersion = 0x00A0,

        /// <summary>
        /// This command sets the serial port baud rate on the radar. 
        /// The value will survive a power cycle, however, it will only be applied after restarting the module.
        /// </summary>
        SetBaudRate = 0x00A1,

        /// <summary>
        /// This command restores the radar module to factory settings.
        /// This takes effect after restarting the radar.
        /// </summary>
        Reset = 0x00A2,

        /// <summary>
        /// This command restarts the radar.
        /// The restart occurs after the acknowledgment respose is sent by the module.
        /// </summary>
        Restart = 0x00A3,

        /// <summary>
        /// This command is used turn on/off the bluetooth on the radar module.
        /// Takes effect after the module is restarted.
        /// </summary>
        /// <remarks>
        /// Bluetooth is enabled by default.
        /// </remarks>
        EnableDisableBluetooth = 0x00A4,

        /// <summary>
        /// This command is used to get the MAC Address of the bluetooth module on the radar.
        /// </summary>
        GetBluetoothMacAddress = 0x00A5,
    }
}
