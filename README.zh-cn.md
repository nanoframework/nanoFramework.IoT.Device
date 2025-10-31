[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/.github/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://github.com/nanoframework/Home/blob/main/resources/logo/nanoFramework-repo-logo.png)

-----

### [English](README.md) | 中文

-----

# 欢迎来到 **nanoFramework** IoT.Device 库仓库！

此仓库包含可连接到 nanoFramework 芯片的传感器、小屏幕和其他设备的绑定！

大部分绑定已从 [.NET IoT 仓库](https://github.com/dotnet/iot/tree/main/src/devices) 迁移过来。并非所有绑定都适合迁移到 .NET nanoFramework，因此迁移工作主要集中在可以与 .NET nanoFramework 配合使用的设备上。另请注意，某些设备在迁移时未经测试，因此可能存在问题。

## 设备列表

<devices>

* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Uln2003.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Uln2003/) [28BYJ-48 Stepper Motor 5V 4-Phase 5-Wire & ULN2003 Driver Board](devices/Uln2003)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.A4988.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.A4988/) [4-Wire stepper motor & A4988 driver board](devices/A4988)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Drv8825.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Drv8825/) [4-Wire stepper motor & Drv8825 driver board](devices/Drv8825)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.AD5328.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.AD5328/) [AD5328 - Digital to Analog Convertor](devices/AD5328)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Seesaw.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Seesaw/) [Adafruit Seesaw - extension board (ADC, PWM, GPIO expander)](devices/Seesaw)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Adc128D818.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Adc128D818/) [ADC128D818 - Analog to Digital Converter](devices/Adc128D818)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ads1115.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ads1115/) [ADS1115 - Analog to Digital Converter](devices/Ads1115)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Adxl343.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Adxl343/) [ADXL343 - Accelerometer](devices/Adxl343)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Adxl345.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Adxl345/) [ADXL345 - Accelerometer](devices/Adxl345)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Adxl357.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Adxl357/) [ADXL357 - Accelerometer](devices/Adxl357)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ags01db.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ags01db/) [AGS01DB - MEMS VOC Gas Sensor](devices/Ags01db)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ahtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ahtxx/) [AHT10/15/20 - Temperature and humidity sensor modules](devices/Ahtxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ak8963.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ak8963/) [AK8963 - Magnetometer](devices/Ak8963)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Am2320.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Am2320/) [AM2320 - Temperature and Humidity sensor](devices/Am2320)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Amg88xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Amg88xx/) [AMG8833/AMG8834/AMG8853/AMG8854 Infrared Array Sensor Family](devices/Amg88xx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Apa102.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Apa102/) [APA102 - Double line transmission integrated control LED](devices/Apa102)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.At24cxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.At24cxx/) [AT24C32/AT24C64/AT24C128/AT24C256 family of I2C EEPROM](devices/At24cxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Axp192.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Axp192/) [AXP192 - Enhanced single Cell Li-Battery and Power System Management IC](devices/Axp192)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bh1745.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bh1745/) [Bh1745 - RGB Sensor](devices/Bh1745)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bh1750fvi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bh1750fvi/) [BH1750FVI - Ambient Light Sensor](devices/Bh1750fvi)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bmm150.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bmm150/) [Bmm150 - Magnetometer](devices/Bmm150)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bmp180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bmp180/) [BMP180 - barometer, altitude and temperature sensor](devices/Bmp180)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bmxx80.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bmxx80/) [BMP280/BME280/BME680 Device Family](devices/Bmxx80)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bno055.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bno055/) [BNO055 - inertial measurement unit](devices/Bno055)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Bq2579x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Bq2579x/) [BQ2579x/BQ25792/BQ25798 - Buck-boost battery charger](devices/Bq2579x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Button.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Button/) [Button](devices/Button)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Buzzer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Buzzer/) [Buzzer - Piezo Buzzer Controller](devices/Buzzer)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ccs811.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ccs811/) [CCS811 Gas sensor](devices/Ccs811)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.CharacterLcd.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.CharacterLcd/) [Character LCD (Liquid Crystal Display)](devices/CharacterLcd)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Charlieplex.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Charlieplex/) [Charlieplex Segment binding](devices/Charlieplex)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Chsc6540.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Chsc6540/) [CHSC6540 - Touch screen controller](devices/Chsc6540)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Dac63004.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Dac63004/) [DAC63004/DAC63004W - Ultra-low-power quad-channel 12-bit smart DAC with I²C, SPI and PWM](devices/Dac63004)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.DCMotor.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.DCMotor/) [DC Motor Controller](devices/DCMotor)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.DhcpServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.DhcpServer/) [DHCP Server](devices/DhcpServer)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Dhtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Dhtxx/) [DHT10/DHT11/DHT12/DHT21/DHT22 - Digital-Output Relative Humidity & Temperature Sensor Module](devices/Dhtxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Dhtxx.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Dhtxx.Esp32/) [DHT10/DHT11/DHT12/DHT21/DHT22 for Esp32 using RMT - Digital-Output Relative Humidity & Temperature Sensor Module](devices/Dhtxx.Esp32)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.LiquidLevel.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.LiquidLevel/) [Digital liquid level switch](devices/LiquidLevel)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.DnsServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.DnsServer/) [DNS Server](devices/DnsServer)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ds1302.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ds1302/) [DS1302 - Realtime Clock](devices/Ds1302)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ds1621.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ds1621/) [Ds1621 - 1-Wire Digital Thermometer with Programmable Resolution](devices/Ds1621)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ds18b20.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ds18b20/) [Ds18b20 - Temperature Sensor](devices/Ds18b20)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.ePaper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.ePaper/) [ePaper drivers for .NET nanoFramework](devices/ePaper)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ft6xx6x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ft6xx6x/) [Ft6xx6x/Ft6336GU - Touch screen controller](devices/Ft6xx6x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.AtModem.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.AtModem/) [Generic AT Modem SIM800 and SIM7070, SIM7080, SIM7090 - Dual Mode Wireless Module CatM, LTE modems](devices/AtModem)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.ShiftRegister.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.ShiftRegister/) [Generic shift register](devices/ShiftRegister)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Common.GnssDevice.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Common.GnssDevice/) [Global Navigation Satellite System Device NMEA 0183 - Including Generic Serial Module with GPS, GNSS, BeiDou - NEO6-M, NEO-M8P-2, NEO-M9N from u-blox, ATGM336H, Minewsemi, ZED-F9P, ZOE-M8Q, SAM-M8Q, SARA-R5 and many many more](devices/GnssDevice)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hcsr04.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hcsr04/) [HC-SR04 - Ultrasonic Ranging Module](devices/Hcsr04)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hcsr04.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hcsr04.Esp32/) [HC-SR04 for ESP32 with RMT - Ultrasonic Ranging Module](devices/Hcsr04.Esp32)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hcsr501.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hcsr501/) [HC-SR501 - PIR Motion Sensor](devices/Hcsr501)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hdc1080.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hdc1080/) [Hdc1080 - temperature and humidity sensor](devices/Hdc1080)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ld2410.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ld2410/) [HLK-LD2410 24Ghz Human Presence Radar Sensor Module](devices/Ld2410)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hmc5883l.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hmc5883l/) [HMC5883L - 3 Axis Digital Compass](devices/Hmc5883l)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hts221.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hts221/) [HTS221 - Capacitive digital sensor for relative humidity and temperature](devices/Hts221)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Hx711.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Hx711/) [Hx711 (M5Stack WEIGHT)](devices/Hx711)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ina219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ina219/) [INA219 - Bidirectional Current/Power Monitor](devices/Ina219)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Multiplexing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Multiplexing/) [Iot.Device.Multiplexing](devices/Multiplexing)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Common.NumberHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Common.NumberHelper/) [Iot.Device.NumberHelper](devices/NumberHelper)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Common.WeatherHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Common.WeatherHelper/) [Iot.Device.WeatherHelper](devices/WeatherHelper)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ip5306.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ip5306/) [IP5306 - Power management](devices/Ip5306)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.KeyMatrix.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.KeyMatrix/) [Key Matrix](devices/KeyMatrix)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.SparkFunLcd.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.SparkFunLcd/) [LCD library for SparkFun RGB Serial Open LCD display (sizes 20x4 or 16x2) with I2C connection](devices/SparkFunLcd)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.LidarLiteV3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.LidarLiteV3/) [LidarLiteV3 - LIDAR Time of Flight Sensor](devices/LidarLiteV3)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Lis2Mdl.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Lis2Mdl/) [LIS2MDL - Ultra-low-power, high-performance 3-axis digital magnetic sensor](devices/Lis2Mdl)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Lm75.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Lm75/) [LM75 - Digital Temperature Sensor](devices/Lm75)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Lp3943.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Lp3943/) [Lp3943 LED driver](devices/Lp3943)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Lps22Hb.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Lps22Hb/) [LPS22HB - MEMS nano pressure sensor: 260-1260 hPa absolute digital output barometer](devices/Lps22Hb)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Lps25h.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Lps25h/) [LPS25H - Piezoresistive pressure and thermometer sensor](devices/Lps25h)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Lsm9Ds1.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Lsm9Ds1/) [LSM9DS1 - 3D accelerometer, gyroscope and magnetometer](devices/Lsm9Ds1)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.AtomQrCode.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.AtomQrCode/) [M5Stack ATOM QR Code reader](devices/AtomQrCode)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Max1704x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Max1704x/) [MAX1704x/MAX17043/MAX17044/MAX17048/MAX17049 - Battery gauge](devices/Max1704x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Max31856.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Max31856/) [Max31856 - cold-junction compensated thermocouple to digital converter](devices/Max31856)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Max31865.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Max31865/) [MAX31865 - Resistance Temperature Detector Amplifier](devices/Max31865)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Max44009.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Max44009/) [MAX44009 - Ambient Light Sensor](devices/Max44009)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Max7219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Max7219/) [Max7219 (LED Matrix driver)](devices/Max7219)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mbi5027.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mbi5027/) [MBI5027 -- 16-bit shift register with error detection](devices/Mbi5027)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp23xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp23xxx/) [Mcp23xxx/MCP23008/MCP23009/MCP23017/MCP23018 - I/O Expander device family](devices/Mcp23xxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp25xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp25xxx/) [Mcp25xxx/MCP2515/MCP2565 device family - CAN bus](devices/Mcp25xxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp3xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp3xxx/) [MCP3001/MCP3002/MCP3004/MCP3008/MCP3201/MCP3202/MCP3204/MCP3208/MCP3301/MCP3302/MCP3304 family of Analog to Digital Converters](devices/Mcp3xxx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp3428.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp3428/) [Mcp3428 - Analog to Digital Converter (I2C)](devices/Mcp3428)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp7940xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp7940xx/) [Mcp7940xx/MCP79400/MCP79401/MCP79402 - I2C Real-Time Clock/Calendar with SRAM](devices/Mcp7940xx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp960x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp960x/) [MCP960X/MCP9600/MCP9601 - device family of cold-junction compensated thermocouple to digital converter](devices/Mcp960x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mcp9808.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mcp9808/) [MCP9808 - Digital Temperature Sensor](devices/Mcp9808)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mfrc522.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mfrc522/) [MFRC522 - RFID reader](devices/Mfrc522)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mhz19b.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mhz19b/) [MH-Z19B CO2-Sensor](devices/Mhz19b)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mlx90614.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mlx90614/) [MLX90614 - Infra Red Thermometer](devices/Mlx90614)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Modbus.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Modbus/) [Modbus - Machine to machine communication protocol](devices/Modbus)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Relay4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Relay4/) [Module and Unit 4 Relay - I2C relay](devices/Relay4)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mpr121.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mpr121/) [MPR121 - Proximity Capacitive Touch Sensor Controller](devices/Mpr121)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mpu9250.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mpu9250/) [MPU6050/MPU6500/MPU9250 - Gyroscope, Accelerometer, Temperature and Magnetometer (MPU9250 only)](devices/Mpu9250)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Mpu6886.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Mpu6886/) [Mpu6886 - accelerometer and gyroscope](devices/Mpu6886)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ms5611.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ms5611/) [Ms5611 in GY-63 module - temperature and pressure sensor](devices/MS5611)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.MulticastDns.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.MulticastDns/) [Multicast DNS](devices/MulticastDns)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Nrf24l01.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Nrf24l01/) [nRF24L01 - Single Chip 2.4 GHz Transceiver](devices/Nrf24l01)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Pca95x4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Pca95x4/) [Pca95x4/PCA9534/PCA9534A/PCA9554/PCA9554A - I2C GPIO Expander](devices/Pca95x4)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Pcd8544.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Pcd8544/) [PCD8544 - 48 × 84 pixels matrix LCD, famous Nokia 5110 screen](devices/Pcd8544)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Pcx857x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Pcx857x/) [PCx857x/PCF8574/PCF8575/PCA8574/PCA8575 - NXP/TI GPIO expansion](devices/Pcx857x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Pn5180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Pn5180/) [PN5180 - RFID and NFC reader](devices/Pn5180)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Pn532.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Pn532/) [PN532 - RFID and NFC reader](devices/Pn532)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.QtrSensors.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.QtrSensors/) [QTR Sensors - Pololu QTR Reflectance Sensors](devices/QtrSensors)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.RotaryEncoder.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.RotaryEncoder/) [Quadrature Rotary Encoder](devices/RotaryEncoder)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.RotaryEncoder.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.RotaryEncoder.Esp32/) [Quadrature Rotary Encoder (ESP32)](devices/RotaryEncoder.Esp32)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.RadioReceiver.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.RadioReceiver/) [Radio Receiver](devices/RadioReceiver)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.RadioTransmitter.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.RadioTransmitter/) [Radio Transmitter](devices/RadioTransmitter)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Rtc.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Rtc/) [Realtime Clock](devices/Rtc)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Card.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Card/) [RFID shared elements](devices/Card)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.RgbDiode.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.RgbDiode/) [RGB diode - PWM](devices/RgbDiode)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Scd4x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Scd4x/) [SCD4x - Temperature & Humidity & CO2 Sensor](devices/Scd4x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Scd30.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Scd30/) [Sensirion SCD30 Particulate Matter Sensor](devices/Scd30)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Sen5x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Sen5x/) [Sensirion SEN5x series module](devices/Sen5x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Sps30.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Sps30/) [Sensirion SPS30 Particulate Matter Sensor](devices/Sps30)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.ServoMotor.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.ServoMotor/) [Servo Motor](devices/ServoMotor)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Sht3x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Sht3x/) [SHT3x/SHT30/SHT31/SHT35 - Temperature & Humidity Sensor](devices/Sht3x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Sht4x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Sht4x/) [Sht4x/SHT40/SHT41/SHT45 - Temperature & Humidity Sensor with internal heater](devices/Sht4x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Shtc3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Shtc3/) [SHTC3 - Temperature & Humidity Sensor](devices/Shtc3)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Si7021.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Si7021/) [Si7021 - Temperature & Humidity Sensor](devices/Si7021)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Sn74hc595.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Sn74hc595/) [SN74HC595 -- 8-bit shift register](devices/Sn74hc595)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ssd13xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ssd13xx/) [SSD13xx/SSD1306/SSD1327 & SSH1106 - OLED display family](devices/Ssd13xx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.SwarmTile.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.SwarmTile/) [Swarm Tile](devices/SwarmTile)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Buffers.Binary.BinaryPrimitives.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Buffers.Binary.BinaryPrimitives/) [System.Buffers.Binary.BinaryPrimitives](devices/System.Buffers.Binary.BinaryPrimitives)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Buffers.Helpers.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Buffers.Helpers/) [System.Buffers.Helpers](devices/System.Buffers.Helpers)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Device.Model.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.Model/) [System.Device.Model - attributes for device bindings](devices/System.Device.Model)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Diagnostics.Stopwatch.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Diagnostics.Stopwatch/) [System.Diagnostics.Stopwatch and DelayHelper](devices/System.Diagnostics.Stopwatch)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Drawing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Drawing/) [System.Drawing](devices/System.Drawing)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Numerics.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Numerics/) [System.Numerics](devices/System.Numerics)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Tcs3472x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Tcs3472x/) [TCS3472x/TCS34721/TCS34723/TCS34725/TCS34727 Sensors](devices/Tcs3472x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Tlc1543.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Tlc1543/) [TLC1543 - 10-bit ADC with 11 input channels](devices/Tlc1543)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Tm1637.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Tm1637/) [TM1637 - Segment Display](devices/Tm1637)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Tsl256x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Tsl256x/) [TSL256x/TSL2560/TSL2561 - Illuminance sensor](devices/Tsl256x)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Vl53L0X.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Vl53L0X/) [VL53L0X - distance sensor](devices/Vl53L0X)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Vl6180X.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Vl6180X/) [Vl6180X - distance sensor](devices/Vl6180X)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ws28xx.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ws28xx.Esp32/) [Ws28xx/WS2812B/WS2815B/WS2808/SK6812/Neo pixel for ESP32 using RMT - LED drivers](devices/Ws28xx.Esp32)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Ws28xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Ws28xx/) [Ws28xx/WS2812B/WS2815B/WS2808/SK6812/Neo pixel using SPI - LED drivers](devices/Ws28xx)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.XPT2046.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.XPT2046/) [XPT2046 - Touch screen controller](devices/XPT2046)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.Iot.Device.Yx5300.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.Iot.Device.Yx5300/) [YX5200/YX5300 - MP3 Player](devices/Yx5300)
</devices>

## 文件夹结构

[/devices/](./devices/) 包含已清理且应该可以直接使用的设备。

[/src/devices_generated/](./src/devices_generated/) 包含从 [.NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices) 自动移植的设备。它们此时可能无法工作或编译，但如果您需要支持某个设备，而 [/devices/](./devices/) 文件夹中没有该设备，这些设备是一个很好的起点。

[/src/nanoFramework.IoT.Device.CodeConverter](./src/nanoFramework.IoT.Device.CodeConverter) 包含用于从 [.NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices) 生成设备的工具。

[/src](./src) 中的其他文件夹包含 nanoFramework 项目，您可以在创建/更新设备时引用这些项目，它们提供诸如 StopWatch、DelayHelper、BinaryPrimitives 或各种 System.Device.Model 属性等功能。

## 贡献

**重要提示：** 如果您计划清理 [/src/devices_generated/](./src/devices_generated/) 中的代码，请将您的工作复制到 [/devices/](./devices/) 文件夹，因为 [/src/devices_generated/](./src/devices_generated/) 的内容将被生成器工具覆盖。

请查看[详细的提示和技巧列表](./tips-trick.md)以促进迁移。生成器会处理一些繁重的工作，但总是需要一些手动调整。

我们为绑定使用以下结构：

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

代码转换器有助于将 .NET Core/.NET 5.0 代码迁移到 .NET nanoFramework。更多信息以及如何[自定义和运行它请点击这里](./src/nanoFramework.IoT.Device.CodeConverter/README.md)。

## 将 .NET nanoFramework 绑定移植到 .NET IoT

您知道吗，通过最小的努力，您可以使 nanoFramework 绑定也可用于 .NET IoT？有关要采取的步骤的更多信息和指导，请参阅[这篇文章](migrate-binding-to-dotnetiot.md)。

## 反馈和文档

有关文档、提供反馈、问题以及如何贡献的信息，请参阅 [Home 仓库](https://github.com/nanoframework/Home)。

在[这里](https://discord.gg/gCyBu8T)加入我们的 Discord 社区。

## 致谢

此项目的贡献者列表可以在 [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md) 找到。

## 许可证

**nanoFramework** 类库根据 [MIT 许可证](LICENSE.md) 授权。

## 行为准则

本项目采用了贡献者公约定义的行为准则，以阐明我们社区中的预期行为。
有关更多信息，请参阅 [.NET Foundation 行为准则](https://dotnetfoundation.org/code-of-conduct)。

### .NET Foundation

本项目由 [.NET Foundation](https://dotnetfoundation.org) 支持。
