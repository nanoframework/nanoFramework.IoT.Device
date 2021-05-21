using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.PiJuiceDevice;
using Iot.Device.PiJuiceDevice.Models;
using UnitsNet;
using UnitsNet.Units;

namespace PiJuiceDevice.Sample
{
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        public static void Main(string[] args)
        {
            Debug.WriteLine("Hello PiJuice!");
            I2cConnectionSettings i2CConnectionSettings = new(1, PiJuice.DefaultI2cAddress);
            PiJuice piJuice = new(I2cDevice.Create(i2CConnectionSettings));
            Debug.WriteLine($"Manufacturer :{piJuice.PiJuiceInfo.Manufacturer}");
            Debug.WriteLine($"Board: {piJuice.PiJuiceInfo.Board}");
            Debug.WriteLine($"Firmware version: {piJuice.PiJuiceInfo.FirmwareVersion}");
            PiJuiceStatus piJuiceStatus = new(piJuice);
            PiJuicePower piJuicePower = new(piJuice);
            PiJuiceConfig piJuiceConfig = new(piJuice);
            while (!Console.KeyAvailable)
            {
                Console.Clear();
                Status status = piJuiceStatus.GetStatus();
                Debug.WriteLine($"Battery state: {status.Battery}");
                Debug.WriteLine($"Battery charge level: {piJuiceStatus.GetChargeLevel()}%");
                Debug.WriteLine($"Battery temperature: {piJuiceStatus.GetBatteryTemperature()}");
                ChargingConfig chargeConfig = piJuiceConfig.GetChargingConfig();
                Debug.WriteLine($"Battery charging enabled: {chargeConfig.Enabled}");

                // Wake up on charge functionality
                WakeUpOnCharge wakeUp = piJuicePower.WakeUpOnCharge;
                Debug.WriteLine($"Is wake up on charge disabled: {wakeUp.Disabled}, Wake up at charging percentage: {wakeUp.WakeUpPercentage}%");
                Debug.WriteLine("Set wake up on charge percentage to 60%");
                piJuicePower.WakeUpOnCharge = new(false, new Ratio(60, RatioUnit.Percent));
                Thread.Sleep(5);
                wakeUp = piJuicePower.WakeUpOnCharge;
                Debug.WriteLine($"Is wake up on charge disabled: {wakeUp.Disabled}, Wake up at charging percentage: {wakeUp.WakeUpPercentage.Value}%");
                piJuicePower.WakeUpOnCharge = new(true, new Ratio(0, RatioUnit.Percent));

                Thread.Sleep(2000);
            }
        }
    }
}
