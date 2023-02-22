[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://github.com/nanoframework/Home/blob/main/resources/logo/nanoFramework-repo-logo.png)

-----
文档语言: [English](README.md) | [简体中文](README.zh-cn.md)

# 欢迎使用.net **nanoFramework** IoT.Device Library repository

该存储库包含绑定，可以是传感器，小屏幕和任何您可以连接到纳米框架芯片的东西!

大多数绑定都是从[.NET IoT repository](https://github.com/dotnet/iot/tree/main/src/devices)迁移过来的。并不是所有的绑定都对迁移到.net纳米框架有意义，所以迁移的工作已经放在了能够与.net nanoFramework 工作的设备上。也请注意，一些设备已经迁移，没有测试，所以它们主要包含问题。

## 设备列表

<devices>

* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Uln2003.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Uln2003/) [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](devices/Uln2003/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.A4988.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.A4988/) [4-Wire stepper motor & A4988 driver board](devices/A4988/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.AD5328.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.AD5328/) [AD5328 - Digital to Analog Convertor](devices/AD5328/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ads1115.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ads1115/) [ADS1115 - Analog to Digital Converter](devices/Ads1115/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl345.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl345/) [ADXL345 - Accelerometer](devices/Adxl345/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl357.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl357/) [ADXL357 - Accelerometer](devices/Adxl357/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ags01db.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ags01db/) [AGS01DB - MEMS VOC Gas Sensor](devices/Ags01db/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ahtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ahtxx/) [AHT10/15/20 - Temperature and humidity sensor modules](devices/Ahtxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ak8963.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ak8963/) [AK8963 - Magnetometer](devices/Ak8963/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Am2320.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Am2320/) [AM2320 - Temperature and Humidity sensor](devices/Am2320/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Amg88xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Amg88xx/) [AMG88xx Infrared Array Sensor Family](devices/Amg88xx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Apa102.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Apa102/) [APA102 - Double line transmission integrated control LED](devices/Apa102/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.At24C128C.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.At24C128C/) [AT24C128C - I2C EEPROM read/write](devices/At24C128C/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Axp192.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Axp192/) [AXP192 - Enhanced single Cell Li-Battery and Power System Management IC](devices/Axp192/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bh1745.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bh1745/) [Bh1745 - RGB Sensor](devices/Bh1745/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bh1750fvi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bh1750fvi/) [BH1750FVI - Ambient Light Sensor](devices/Bh1750fvi/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmm150.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmm150/) [Bmm150 - Magnetometer](devices/Bmm150/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmp180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmp180/) [BMP180 - barometer, altitude and temperature sensor](devices/Bmp180/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmxx80.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmxx80/) [BMxx80 Device Family](devices/Bmxx80/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bno055.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bno055/) [BNO055 - inertial measurement unit](devices/Bno055/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Button.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Button/) [Button](devices/Button/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Buzzer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Buzzer/) [Buzzer - Piezo Buzzer Controller](devices/Buzzer/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ccs811.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ccs811/) [CCS811 Gas sensor](devices/Ccs811/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.CharacterLcd.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.CharacterLcd/) [Character LCD (Liquid Crystal Display)](devices/CharacterLcd/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Charlieplex.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Charlieplex/) [Charlieplex Segment binding](devices/Charlieplex/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Chsc6540.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Chsc6540/) [CHSC6540 - Touch screen controller](devices/Chsc6540/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.DCMotor.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.DCMotor/) [DC Motor Controller](devices/DCMotor/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.DhcpServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.DhcpServer/) [DHCP Server](devices/DhcpServer/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Dhtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Dhtxx/) [DHTxx - Digital-Output Relative Humidity & Temperature Sensor Module](devices/Dhtxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Dhtxx.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Dhtxx.Esp32/) [DHTxx.Esp32 - Digital-Output Relative Humidity & Temperature Sensor Module](devices/Dhtxx.Esp32/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.LiquidLevel.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.LiquidLevel/) [Digital liquid level switch](devices/LiquidLevel/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ds1302.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ds1302/) [DS1302 - Realtime Clock](devices/Ds1302/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ds1621.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ds1621/) [Ds1621 - 1-Wire Digital Thermometer with Programmable Resolution](devices/Ds1621/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ds18b20.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ds18b20/) [Ds18b20 - Temperature Sensor](devices/Ds18b20/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ft6xx6x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ft6xx6x/) [Ft6xx6x - Touch screen controller](devices/Ft6xx6x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.ShiftRegister.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.ShiftRegister/) [Generic shift register](devices/ShiftRegister/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr04.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr04/) [HC-SR04 - Ultrasonic Ranging Module](devices/Hcsr04/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr04.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr04.Esp32/) [HC-SR04 - Ultrasonic Ranging Module for ESP32 with RMT](devices/Hcsr04.Esp32/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr501.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr501/) [HC-SR501 - PIR Motion Sensor](devices/Hcsr501/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hdc1080.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hdc1080/) [Hdc1080 - temperature and humidity sensor](devices/Hdc1080/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hmc5883l.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hmc5883l/) [HMC5883L - 3 Axis Digital Compass](devices/Hmc5883l/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hts221.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hts221/) [HTS221 - Capacitive digital sensor for relative humidity and temperature](devices/Hts221/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hx711.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hx711/) [Hx711 (M5Stack WEIGHT)](devices/Hx711/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ina219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ina219/) [INA219 - Bidirectional Current/Power Monitor](devices/Ina219/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Multiplexing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Multiplexing/) [Iot.Device.Multiplexing](devices/Multiplexing/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.NumberHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.NumberHelper/) [Iot.Device.NumberHelper](devices/NumberHelper/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.WeatherHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.WeatherHelper/) [Iot.Device.WeatherHelper](devices/WeatherHelper/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ip5306.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ip5306/) [IP5306 - Power management](devices/Ip5306/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.KeyMatrix.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.KeyMatrix/) [Key Matrix](devices/KeyMatrix/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.SparkFunLcd.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.SparkFunLcd/) [LCD library for SparkFun RGB Serial Open LCD display (sizes 20x4 or 16x2) with I2C connection](devices/SparkFunLcd/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.LidarLiteV3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.LidarLiteV3/) [LidarLiteV3 - LIDAR Time of Flight Sensor](devices/LidarLiteV3/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lm75.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lm75/) [LM75 - Digital Temperature Sensor](devices/Lm75/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lp3943.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lp3943/) [Lp3943 LED driver](devices/Lp3943/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lps25h.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lps25h/) [LPS25H - Piezoresistive pressure and thermometer sensor](devices/Lps25h/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lsm9Ds1.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lsm9Ds1/) [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](devices/Lsm9Ds1/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.AtomQrCode.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.AtomQrCode/) [M5Stack ATOM QR Code reader](devices/AtomQrCode/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max31856.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max31856/) [Max31856 - cold-junction compensated thermocouple to digital converter](devices/Max31856/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max31865.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max31865/) [MAX31865 - Resistance Temperature Detector Amplifier](devices/Max31865/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max44009.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max44009/) [MAX44009 - Ambient Light Sensor](devices/Max44009/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max7219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max7219/) [Max7219 (LED Matrix driver)](devices/Max7219/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mbi5027.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mbi5027/) [MBI5027 -- 16-bit shift register with error detection](devices/Mbi5027/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp23xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp23xxx/) [Mcp23xxx - I/O Expander device family](devices/Mcp23xxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp25xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp25xxx/) [Mcp25xxx device family - CAN bus](devices/Mcp25xxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp3428.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp3428/) [Mcp3428 - Analog to Digital Converter (I2C)](devices/Mcp3428/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp3xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp3xxx/) [MCP3xxx family of Analog to Digital Converters](devices/Mcp3xxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp7940xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp7940xx/) [Mcp7940xx - I2C Real-Time Clock/Calendar with SRAM](devices/Mcp7940xx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp960x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp960x/) [MCP960X - device family of cold-junction compensated thermocouple to digital converter](devices/Mcp960x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp9808.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp9808/) [MCP9808 - Digital Temperature Sensor](devices/Mcp9808/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mfrc522.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mfrc522/) [MFRC522 - RFID reader](devices/Mfrc522/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mhz19b.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mhz19b/) [MH-Z19B CO2-Sensor](devices/Mhz19b/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mlx90614.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mlx90614/) [MLX90614 - Infra Red Thermometer](devices/Mlx90614/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Relay4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Relay4/) [Module and Unit 4 Relay - I2C relay](devices/Relay4/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpr121.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpr121/) [MPR121 - Proximity Capacitive Touch Sensor Controller](devices/Mpr121/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpu9250.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpu9250/) [MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](devices/Mpu9250/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpu6886.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpu6886/) [Mpu6886 - accelerometer and gyroscope](devices/Mpu6886/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.MS5611.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.MS5611/) [Ms5611 in GY-63 module - temperature and pressure sensor](devices/MS5611/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Nrf24l01.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Nrf24l01/) [nRF24L01 - Single Chip 2.4 GHz Transceiver](devices/Nrf24l01/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pcx857x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pcx857x/) [NXP/TI PCx857x](devices/Pcx857x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pca95x4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pca95x4/) [Pca95x4 - I2C GPIO Expander](devices/Pca95x4/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pcd8544.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pcd8544/) [PCD8544 - 48 × 84 pixels matrix LCD, famous Nokia 5110 screen](devices/Pcd8544/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pn5180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pn5180/) [PN5180 - RFID and NFC reader](devices/Pn5180/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pn532.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pn532/) [PN532 - RFID and NFC reader](devices/Pn532/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RotaryEncoder.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RotaryEncoder/) [Quadrature Rotary Encoder](devices/RotaryEncoder/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RadioReceiver.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RadioReceiver/) [Radio Receiver](devices/RadioReceiver/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RadioTransmitter.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RadioTransmitter/) [Radio Transmitter](devices/RadioTransmitter/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Rtc.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Rtc/) [Realtime Clock](devices/Rtc/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Card.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Card/) [RFID shared elements](devices/Card/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Scd30.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Scd30/) [Sensirion SCD30 Particulate Matter Sensor](devices/Scd30/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sps30.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sps30/) [Sensirion SPS30 Particulate Matter Sensor](devices/Sps30/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.ServoMotor.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.ServoMotor/) [Servo Motor](devices/ServoMotor/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sht3x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sht3x/) [SHT3x - Temperature & Humidity Sensor](devices/Sht3x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Shtc3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Shtc3/) [SHTC3 - Temperature & Humidity Sensor](devices/Shtc3/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Si7021.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Si7021/) [Si7021 - Temperature & Humidity Sensor](devices/Si7021/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sn74hc595.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sn74hc595/) [SN74HC595 -- 8-bit shift register](devices/Sn74hc595/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ssd13xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ssd13xx/) [SSD13xx & SSH1106 OLED display family](devices/Ssd13xx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.SwarmTile.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.SwarmTile/) [Swarm Tile](devices/SwarmTile/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Buffers.Binary.BinaryPrimitives.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Buffers.Binary.BinaryPrimitives/) [System.Buffers.Binary.BinaryPrimitives](devices/System.Buffers.Binary.BinaryPrimitives/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Device.Model.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.Model/) [System.Device.Model - attributes for device bindings](devices/System.Device.Model/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Diagnostics.Stopwatch.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Diagnostics.Stopwatch/) [System.Diagnostics.Stopwatch and DelayHelper](devices/System.Diagnostics.Stopwatch/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Drawing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Drawing/) [System.Drawing](devices/System.Drawing/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Numerics.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Numerics/) [System.Numerics](devices/System.Numerics/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tcs3472x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tcs3472x/) [TCS3472x Sensors](devices/Tcs3472x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tlc1543.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tlc1543/) [TLC1543 - 10-bit ADC with 11 input channels](devices/Tlc1543/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tm1637.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tm1637/) [TM1637 - Segment Display](devices/Tm1637/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tsl256x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tsl256x/) [TSL256x - Illuminance sensor](devices/Tsl256x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Vl53L0X.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Vl53L0X/) [VL53L0X - distance sensor](devices/Vl53L0X/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ws28xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ws28xx/) [Ws28xx LED drivers](devices/Ws28xx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ws28xx.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ws28xx.Esp32/) [Ws28xx, SK6812 LED drivers](devices/Ws28xx.Esp32/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Yx5300.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Yx5300/) [YX5200/YX5300 - MP3 Player](devices/Yx5300/README.md)
</devices>

## 文件夹结构

[/src/devices/](/src/devices/)包含已清除且应该可以开箱即用的设备。

[/src/devices_generated/](/src/devices_generated/)包含被自动移植的设备 [. NET Core Iot 库设备](https://github.com/dotnet/iot/tree/main/src/devices)此时它们可能无法工作或编译，但如果您需要支持这里包含的其中一个设备，但[/src/devices/](/src/devices/)文件夹中缺少该设备，则它们是一个很好的起点。

[/src/nanoFramework.IoT.Device.CodeConverter](/src/nanoFramework.IoT.Device.CodeConverter)包含用于从中生成设备的工具
[. NET Core Iot 库设备](https://github.com/dotnet/iot/tree/main/src/devices).

[/src](/src)中的其他文件夹包含纳米框架项目，您可以在创建/更新设备时引用这些项目，提供诸如StopWatach、DelayHelper、BinaryPrimitives或各种System.Device.Model属性等功能。

## 贡献

**重要提示:** 如果您计划清理[/src/devices_generated/](/src/devices_generated/)中的代码，请将您的工作复制到[/src/devices/](/src/devices/)文件夹中，因为[/src/devices_generated/](/src/devices_generated/)的内容将被生成器工具覆盖。

请查看[提示和技巧详细列表](./tips-trick.md)以方便迁移。发电机承担了一些繁重的起重工作，但总需要一些手动调整。

我们使用以下结构进行绑定:

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

## 使用代码转换器

代码转换器允许简化.net Core/的迁移.NET 5.0代码到.NET纳米框架。更多信息以及如何操作 [在这里自定义并运行它](./src/nanoFramework.IoT.Device.CodeConverter/README.md).

## 将.NET nanoFramework绑定移植到.net Iot

你知道吗?你可以用最少的努力为.net物联网提供一个纳米框架绑定。更多关于步骤的信息和指导，可以在 [这篇文章](migrate-binding-to-dotnetiot.md).

## 反馈和文档

关于文档，提供反馈，问题和找出如何贡献，请参考 [首页](https://github.com/nanoframework/Home).

加入我们的Discord社区[这里](https://discord.gg/gCyBu8T).

## Credits

这个项目的贡献者名单可以在 [贡献者](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

**nanoFramework** 类库是根据 [MIT license](LICENSE.md).

## Code of Conduct

本项目采用了《贡献者盟约》所规定的行为准则，以澄清我们社区的预期行为。
有关更多信息，请参阅 [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

这个项目是由 [.NET Foundation](https://dotnetfoundation.org) 支持.
