[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://github.com/nanoframework/Home/blob/main/resources/logo/nanoFramework-repo-logo.png)

-----

# Welcome to the **nanoFramework** IoT.Device Library repository!

This repository contains bindings which can be sensors, small screen and anything else that you can connect to your nanoFramework chip!

Most of the bindings have been migrated from [.NET IoT repository](https://github.com/dotnet/iot/tree/main/src/devices). Not all the bindings make sense to migrate to .NET nanoFramework, so the effort of migration has been placed into devices that can work with .NET nanoFramework. Please note as well that some devices have been migrated without been tested, so they main contain problems.

## List of devices

<devices>

* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Uln2003.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Uln2003/) [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](devices/Uln2003)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.AD5328.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.AD5328/) [AD5328 - Digital to Analog Convertor](devices/AD5328)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ads1115.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ads1115/) [ADS1115 - Analog to Digital Converter](devices/Ads1115)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl345.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl345/) [ADXL345 - Accelerometer](devices/Adxl345)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl357.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl357/) [ADXL357 - Accelerometer](devices/Adxl357)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ags01db.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ags01db/) [AGS01DB - MEMS VOC Gas Sensor](devices/Ags01db)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ahtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ahtxx/) [AHT10/15/20 - Temperature and humidity sensor modules](devices/Ahtxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ak8963.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ak8963/) [AK8963 - Magnetometer](devices/Ak8963)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Amg88xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Amg88xx/) [AMG88xx Infrared Array Sensor Family](devices/Amg88xx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Apa102.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Apa102/) [APA102 - Double line transmission integrated control LED](devices/Apa102)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.At24C128C.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.At24C128C/) [AT24C128C - I2C EEPROM read/write](devices/At24C128C)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bh1745.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bh1745/) [Bh1745 - RGB Sensor](devices/Bh1745)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bh1750fvi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bh1750fvi/) [BH1750FVI - Ambient Light Sensor](devices/Bh1750fvi)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmp180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmp180/) [BMP180 - barometer, altitude and temperature sensor](devices/Bmp180)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmxx80.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmxx80/) [BMxx80 Device Family](devices/Bmxx80)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bno055.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bno055/) [BNO055 - inertial measurement unit](devices/Bno055)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ccs811.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ccs811/) [CCS811 Gas sensor](devices/Ccs811)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Charlieplex.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Charlieplex/) [Charlieplex Segment binding](devices/Charlieplex)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Dhtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Dhtxx/) [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](devices/Dhtxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.LiquidLevel.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.LiquidLevel/) [Digital liquid level switch](devices/LiquidLevel)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.ShiftRegister.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.ShiftRegister/) [Generic shift register](devices/ShiftRegister)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr04.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr04/) [HC-SR04 - Ultrasonic Ranging Module](devices/Hcsr04)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr501.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr501/) [HC-SR501 - PIR Motion Sensor](devices/Hcsr501)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hmc5883l.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hmc5883l/) [HMC5883L - 3 Axis Digital Compass](devices/Hmc5883l)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hts221.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hts221/) [HTS221 - Capacitive digital sensor for relative humidity and temperature](devices/Hts221)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Multiplexing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Multiplexing/) [Iot.Device.Multiplexing](devices/Multiplexing)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.NumberHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.NumberHelper/) [Iot.Device.NumberHelper](devices/NumberHelper)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.WeatherHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.WeatherHelper/) [Iot.Device.WeatherHelper](devices/WeatherHelper)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.KeyMatrix.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.KeyMatrix/) [Key Matrix](devices/KeyMatrix)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.LidarLiteV3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.LidarLiteV3/) [LidarLiteV3 - LIDAR Time of Flight Sensor](devices/LidarLiteV3)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lm75.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lm75/) [LM75 - Digital Temperature Sensor](devices/Lm75)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lps25h.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lps25h/) [LPS25H - Piezoresistive pressure and thermometer sensor](devices/Lps25h)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lsm9Ds1.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lsm9Ds1/) [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](devices/Lsm9Ds1)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max31856.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max31856/) [Max31856 - cold-junction compensated thermocouple to digital converter](devices/Max31856)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max31865.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max31865/) [MAX31865 - Resistance Temperature Detector Amplifier](devices/Max31865)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max44009.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max44009/) [MAX44009 - Ambient Light Sensor](devices/Max44009)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max7219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max7219/) [Max7219 (LED Matrix driver)](devices/Max7219)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mbi5027.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mbi5027/) [MBI5027 -- 16-bit shift register with error detection](devices/Mbi5027)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp25xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp25xxx/) [Mcp25xxx device family - CAN bus](devices/Mcp25xxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp3428.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp3428/) [Mcp3428 - Analog to Digital Converter (I2C)](devices/Mcp3428)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp3xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp3xxx/) [MCP3xxx family of Analog to Digital Converters](devices/Mcp3xxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp9808.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp9808/) [MCP9808 - Digital Temperature Sensor](devices/Mcp9808)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mfrc522.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mfrc522/) [MFRC522 - RFID reader](devices/Mfrc522)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mlx90614.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mlx90614/) [MLX90614 - Infra Red Thermometer](devices/Mlx90614)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpr121.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpr121/) [MPR121 - Proximity Capacitive Touch Sensor Controller](devices/Mpr121)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpu9250.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpu9250/) [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](devices/Mpu9250)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Nrf24l01.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Nrf24l01/) [nRF24L01 - Single Chip 2.4 GHz Transceiver](devices/Nrf24l01)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pca95x4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pca95x4/) [Pca95x4 - I2C GPIO Expander](devices/Pca95x4)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pn5180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pn5180/) [PN5180 - RFID and NFC reader](devices/Pn5180)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pn532.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pn532/) [PN532 - RFID and NFC reader](devices/Pn532)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RotaryEncoder.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RotaryEncoder/) [Quadrature Rotary Encoder](devices/RotaryEncoder)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RadioReceiver.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RadioReceiver/) [Radio Receiver](devices/RadioReceiver)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RadioTransmitter.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RadioTransmitter/) [Radio Transmitter](devices/RadioTransmitter)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Rtc.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Rtc/) [Realtime Clock](devices/Rtc)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.CardRfid.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.CardRfid/) [RFID shared elements](devices/Card)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sht3x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sht3x/) [SHT3x - Temperature & Humidity Sensor](devices/Sht3x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Shtc3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Shtc3/) [SHTC3 - Temperature & Humidity Sensor](devices/Shtc3)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Si7021.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Si7021/) [Si7021 - Temperature & Humidity Sensor](devices/Si7021)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sn74hc595.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sn74hc595/) [SN74HC595 -- 8-bit shift register](devices/Sn74hc595)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ssd13xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ssd13xx/) [Solomon Systech Ssd1306 OLED display](devices/Ssd13xx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Buffers.Binary.BinaryPrimitives.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Buffers.Binary.BinaryPrimitives/) [System.Buffers.Binary.BinaryPrimitives](devices/System.Buffers.Binary.BinaryPrimitives)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Device.Model.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.Model/) [System.Device.Model - attributes for device bindings](devices/System.Device.Model)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Diagnostics.Stopwatch.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Diagnostics.Stopwatch/) [System.Diagnostics.Stopwatch and DelayHelper](devices/System.Diagnostics.Stopwatch)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Drawing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Drawing/) [System.Drawing](devices/System.Drawing)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Numerics.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Numerics/) [System.Numerics](devices/System.Numerics)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Vl53L0X.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Vl53L0X/) [VL53L0X - distance sensor](devices/Vl53L0X)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Yx5300.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Yx5300/) [YX5200/YX5300 - MP3 Player](devices/Yx5300)
</devices>

## Folder Structure

[/src/devices/](/src/devices/) contains devices that were cleaned up and should be working out of the box.

[/src/devices_generated/](/src/devices_generated/) contains devices that were automatically ported from [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices). They might not work or compile at this point, but are a good starting point if you need support for one of the devices contained here but missing from the [/src/devices/](/src/devices/) folder.

[/src/nanoFramework.IoT.Device.CodeConverter](/src/nanoFramework.IoT.Device.CodeConverter) contains the tool used to generate the devices from [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices).

Other folders in [/src](/src) contain nanoFramework projects that you can reference when creating/updating devices with provide functionality such as a StopWatach, a DelayHelper, BinaryPrimitives or various System.Device.Model Attributes.

## Contributing

**Important:** If you plan to clean up the code in [/src/devices_generated/](/src/devices_generated/), please copy your work to the [/src/devices/](/src/devices/) folder as the content of [/src/devices_generated/](/src/devices_generated/) will be overwritten by the generator tool.

Please check the [detail list of tips and tricks](./tips-trick.md) to facilitate the migration. The generator takes care of some heavy lifting but there is always some manual adjustments needed.

We are using the following structure for the bindings:

```text
/devices
  /Binding1
    /samples
      Binding1.Samples.nfproj
      AssicateFile.cs
      Program.cs
    /test
      BindingA.Test.nfproj
      AssociatedTestFile.cs
    Binding1.nfproj
    Binding1.nuspec
    version.json
    OtherFiles.cs
    OtherFiles.anythingelse
    Readme.md
```

## Using the Code Converter

The Code Converter allows to facilitate migration of .NET Core/.NET 5.0 code into .NET nanoFramework. More information and how to [customize and run it here](./src/nanoFramework.IoT.Device.CodeConverter/README.md).

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
