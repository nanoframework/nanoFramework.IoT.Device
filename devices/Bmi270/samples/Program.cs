// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Axp2101;
using Iot.Device.Bmi270;
using Iot.Device.Magnetometer;
using System.Device.I2c;
using System.Diagnostics;
using nanoFramework.Hardware.Esp32;
using System.Threading;
using UnitsNet;

namespace Bmi270Sample
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("BMI270 sample adjusted for the M5Stack CoreS3.");

            // CoreS3 internal I2C bus: official M5Stack pin map uses G12 SDA / G11 SCL.
            Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

            EnableCoreS3InternalBusPower();
            EnableCoreS3SensorPower();

            I2cConnectionSettings settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);

            using (Bmi270AccelerometerGyroscope imu = new(I2cDevice.Create(settings)))
            {
                Debug.WriteLine("BMI270 initialized successfully!");

                // Enable auxiliary I2C for BMM150 magnetometer
                imu.EnableAuxiliaryI2c(Bmm150.SecondaryI2cAddress);
                Debug.WriteLine("Auxiliary I2C enabled for BMM150.");

                // Create BMM150 using the BMI270 auxiliary I2C bridge
                I2cConnectionSettings bmm150Settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);
                using (Bmm150 mag = new(I2cDevice.Create(bmm150Settings), new Bmm150I2cBmi270(Bmm150.SecondaryI2cAddress)))
                {
                    Debug.WriteLine("BMM150 initialized successfully via BMI270 aux I2C!");

                    Debug.WriteLine("Start calibration ...");
                    var offset = imu.Calibrate(1000);
                    Debug.WriteLine($"Calibration done, calculated offsets X:{offset.X} Y:{offset.Y} Z:{offset.Z}");

                    Debug.WriteLine($"Internal temperature: {imu.GetInternalTemperature().DegreesCelsius} C");

                    while (true)
                    {
                        var acc = imu.GetAccelerometer();
                        var gyr = imu.GetGyroscope();
                        var magData = mag.ReadMagnetometer();
                        Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
                        Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}");
                        Debug.WriteLine($"Magnetometer data x:{magData.X} y:{magData.Y} z:{magData.Z}\n");
                        Thread.Sleep(100);
                    }
                }
            }
        }

        private static void EnableCoreS3SensorPower()
        {
            using I2cDevice i2cAxp2101 = I2cDevice.Create(new I2cConnectionSettings(1, Axp2101.I2cDefaultAddress));
            using Axp2101 power = new(i2cAxp2101);

            byte chipId = power.GetChipId();
            Debug.WriteLine($"AXP2101 Chip ID: 0x{chipId:X2} (expected 0x{Axp2101.ChipId:X2})");
            if (chipId != Axp2101.ChipId)
            {
                throw new InvalidOperationException("AXP2101 chip ID mismatch. CoreS3 internal sensor rails were not configured.");
            }

            // Match M5Stack's CoreS3 power profile used by the official source.
            power.Aldo1Voltage = ElectricPotential.FromVolts(1.8);
            power.EnableAldo1();

            power.Aldo2Voltage = ElectricPotential.FromVolts(3.3);
            power.EnableAldo2();

            power.Aldo3Voltage = ElectricPotential.FromVolts(3.3);
            power.EnableAldo3();

            power.Aldo4Voltage = ElectricPotential.FromVolts(3.3);
            power.EnableAldo4();

            power.Bldo1Voltage = ElectricPotential.FromVolts(3.3);
            power.EnableBldo1();

            power.Bldo2Voltage = ElectricPotential.FromVolts(3.3);
            power.EnableBldo2();

            Debug.WriteLine("CoreS3 power rails enabled via AXP2101: ALDO1/2/3/4 + BLDO1/2.");
            Thread.Sleep(20);
        }

        private static void EnableCoreS3InternalBusPower()
        {
            const int aw9523Address = 0x58;
            const byte regPort0Output = 0x02;
            const byte regPort1Output = 0x03;
            const byte busEnableMask = 0x02;
            const byte boostEnableMask = 0x80;

            using I2cDevice aw9523 = I2cDevice.Create(new I2cConnectionSettings(1, aw9523Address));

            if (!TrySetBits(aw9523, regPort0Output, busEnableMask, out I2cTransferStatus port0Status))
            {
                throw new InvalidOperationException($"AW9523 BUS_EN update failed. Status: {port0Status}");
            }

            if (!TrySetBits(aw9523, regPort1Output, boostEnableMask, out I2cTransferStatus port1Status))
            {
                throw new InvalidOperationException($"AW9523 BOOST_EN update failed. Status: {port1Status}");
            }

            Debug.WriteLine("CoreS3 internal bus power enabled via AW9523: BUS_EN + BOOST_EN.");
            Thread.Sleep(10);
        }

        private static bool TrySetBits(I2cDevice device, byte register, byte bits, out I2cTransferStatus status)
        {
            byte[] readCmd = new byte[] { register };
            byte[] value = new byte[1];
            I2cTransferResult read = device.WriteRead(readCmd, value);
            if (read.Status != I2cTransferStatus.FullTransfer)
            {
                status = read.Status;
                return false;
            }

            byte[] write = new byte[] { register, (byte)(value[0] | bits) };
            I2cTransferResult writeResult = device.Write(write);
            status = writeResult.Status;
            return writeResult.Status == I2cTransferStatus.FullTransfer;
        }
    }
}
