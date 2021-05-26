[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://github.com/nanoframework/Home/blob/main/resources/logo/nanoFramework-repo-logo.png)

-----

# Welcome to the **nanoFramework** IoT.Device Library repository!

This repository contains bindings which can be sensors, small screen and anything else that you can connect to your nanoFramework chip!

Most of the bindings have been migrated from [.NET IoT repository](https://github.com/dotnet/iot/tree/main/src/devices). Not all the bindings make sense to migrate to .NET nanoFramework, so the effort of migration has been placed into devices that can work with .NET nanoFramework. Please note as well that some devices have been migrated without been tested, so they main contain problems.

## List of devices

* [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](Uln2003/README.md)
* [AD5328 - Digital to Analog Convertor](AD5328/README.md)
* [ADS1115 - Analog to Digital Converter](Ads1115/README.md)
* [ADXL345 - Accelerometer](Adxl345/README.md)
* [ADXL357 - Accelerometer](Adxl357/README.md)
* [AGS01DB - MEMS VOC Gas Sensor](Ags01db/README.md)
* [AHT10/15/20 - Temperature and humidity sensor modules](Ahtxx/README.md)
* [AK8963 - Magnetometer](Ak8963/README.md)
* [AMG88xx Infrared Array Sensor Family](Amg88xx/README.md)
* [APA102 - Double line transmission integrated control LED](Apa102/README.md)
* [Bh1745 - RGB Sensor](Bh1745/README.md)
* [BH1750FVI - Ambient Light Sensor](Bh1750fvi/README.md)
* [BMP180 - barometer, altitude and temperature sensor](Bmp180/README.md)
* [BMxx80 Device Family](Bmxx80/README.md)
* [BNO055 - inertial measurement unit](Bno055/README.md)
* [CCS811 Gas sensor](Ccs811/README.md)
* [Charlieplex Segment binding](Charlieplex/README.md)
* [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](Dhtxx/README.md)
* [Digital liquid level switch](LiquidLevel/README.md)
* [Generic shift register](ShiftRegister/README.md)
* [HC-SR04 - Ultrasonic Ranging Module](Hcsr04/README.md)
* [HC-SR501 - PIR Motion Sensor](Hcsr501/README.md)
* [HMC5883L - 3 Axis Digital Compass](Hmc5883l/README.md)
* [HTS221 - Capacitive digital sensor for relative humidity and temperature](Hts221/README.md)
* [Key Matrix](KeyMatrix/README.md)
* [LidarLiteV3 - LIDAR Time of Flight Sensor](LidarLiteV3/README.md)
* [LM75 - Digital Temperature Sensor](Lm75/README.md)
* [LPS25H - Piezoresistive pressure and thermometer sensor](Lps25h/README.md)
* [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](Lsm9Ds1/README.md)
* [Max31856 - cold-junction compensated thermocouple to digital converter](Max31856/README.md)
* [MAX31865 - Resistance Temperature Detector Amplifier](Max31865/README.md)
* [MAX44009 - Ambient Light Sensor](Max44009/README.md)
* [Max7219 (LED Matrix driver)](Max7219/README.md)
* [MBI5027 -- 16-bit shift register with error detection](Mbi5027/README.md)
* [Mcp25xxx device family - CAN bus](Mcp25xxx/README.md)
* [Mcp3428 - Analog to Digital Converter (I2C)](Mcp3428/README.md)
* [MCP3xxx family of Analog to Digital Converters](Mcp3xxx/README.md)
* [MCP9808 - Digital Temperature Sensor](Mcp9808/README.md)
* [MLX90614 - Infra Red Thermometer](Mlx90614/README.md)
* [MPR121 - Proximity Capacitive Touch Sensor Controller](Mpr121/README.md)
* [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](Mpu9250/README.md)
* [nRF24L01 - Single Chip 2.4 GHz Transceiver](Nrf24l01/README.md)
* [Pca95x4 - I2C GPIO Expander](Pca95x4/README.md)
* [Quadrature Rotary Encoder](RotaryEncoder/README.md)
* [Radio Receiver](RadioReceiver/README.md)
* [Radio Transmitter](RadioTransmitter/README.md)
* [Si7021 - Temperature & Humidity Sensor](Si7021/README.md)
* [SHT3x - Temperature & Humidity Sensor](Shtc3/README.md)
* [SHTC3 - Temperature & Humidity Sensor](Sht3x/README.md)
* [SN74HC595 -- 8-bit shift register](Sn74hc595/README.md)
* [Solomon Systech Ssd1306 OLED display](Ssd13xx/README.md)
* [VL53L0X - distance sensor](Vl53L0X/README.md)

## Folder Structure

[/src/devices/](/src/devices/) contains devices that were cleaned up and should be working out of the box.

[/src/devices_generated/](/src/devices_generated/) contains devices that were automatically ported from [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices). They might not work or compile at this point, but are a good starting point if you need support for one of the devices contained here but missing from the [/src/devices/](/src/devices/) folder.

[/src/nanoFramework.IoT.Device.CodeConverter](/src/nanoFramework.IoT.Device.CodeConverter) contains the tool used to generate the devices from [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices).

Other folders in [/src](/src) contain nanoFramework projects that you can reference when creating/updating devices with provide functionality such as a StopWatach, a DelayHelper, BinaryPrimitives or various System.Device.Model Attributes.

## Contributing

**Important:** If you plan to clean up the code in [/src/devices_generated/](/src/devices_generated/), please copy your work to the [/src/devices/](/src/devices/) folder as the content of [/src/devices_generated/](/src/devices_generated/) will be overwritten by the generator tool.

Please check the [detail list of tips and tricks](./tips-tricks.md) to facilitate the migration. The generator takes care of some heavy lifting but there is always some manual adjustments needed.

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
