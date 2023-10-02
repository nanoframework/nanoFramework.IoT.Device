// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the battery status.
    /// </summary>
    public class BatteryStatus
    {
        /// <summary>
        /// Gets the battery charge status.
        /// </summary>
        public BatteryChargeStatus Status { get; }

        /// <summary>
        /// Gets the charge level of the battery.
        /// </summary>
        public Ratio ChargeLevel { get; }

        /// <summary>
        /// Gets the voltage of the battery.
        /// </summary>
        public ElectricPotential Voltage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatteryStatus"/> class with the specified parameters.
        /// </summary>
        /// <param name="status">The battery charge status.</param>
        /// <param name="chargeLevel">The charge level of the battery.</param>
        /// <param name="voltage">The voltage of the battery. (Optional).</param>
        public BatteryStatus(BatteryChargeStatus status, Ratio chargeLevel, ElectricPotential voltage = default)
        {
            Status = status;
            ChargeLevel = chargeLevel;
            Voltage = voltage;
        }

        /// <summary>
        /// Returns a string representation of the battery status.
        /// </summary>
        /// <returns>A string representation of the battery status.</returns>
        public override string ToString()
        {
            string statusStr;

            switch (Status)
            {
                case BatteryChargeStatus.PoweredByBattery:
                    statusStr = "Powered by battery";
                    break;
                case BatteryChargeStatus.Charging:
                    statusStr = "Charging";
                    break;
                case BatteryChargeStatus.ChargingFinished:
                    statusStr = "Charging finished";
                    break;
                case BatteryChargeStatus.PowerFault:
                    statusStr = "Power fault";
                    break;
                default:
                    statusStr = "Unknown";
                    break;
            }

            return $"Charge Status: {statusStr}, Charge Level: {ChargeLevel}, Voltage: {Voltage}";
        }
    }
}
