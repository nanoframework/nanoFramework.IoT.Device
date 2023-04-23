[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/.github/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://github.com/nanoframework/Home/blob/main/resources/logo/nanoFramework-repo-logo.png)

文档语言: [English](README.md) | [简体中文](README.zh-cn.md)

-----

欢迎来到 **nanoFramework** IoT.Device 库仓库！

本仓库包含了可以连接到你的 nanoFramework 芯片的传感器、小屏幕和其他设备的绑定。

大部分的绑定已经从 [.NET IoT 仓库](https://github.com/dotnet/iot/tree/main/src/devices) 迁移过来。并不是所有的绑定都适合迁移到 .NET nanoFramework，因此迁移的工作重点放在了可以与 .NET nanoFramework 兼容的设备上。请注意，有些设备已经迁移但未经过测试，因此可能存在问题。
设备列表

<devices>

* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Uln2003.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Uln2003/) [28BYJ-48步进电机5V 4相5线和ULN2003驱动板](devices/Uln2003/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.A4988.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.A4988/) [4线步进电机和A4988驱动板](devices/A4988/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.AD5328.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.AD5328/) [AD5328 - 数字到模拟转换器](devices/AD5328/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ads1115.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ads1115/) [ADS1115 - 模拟到数字转换器](devices/Ads1115/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl343.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl343/) [ADXL343 - 加速度计](devices/Adxl343/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl345.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl345/) [ADXL345 - 加速度计](devices/Adxl345/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Adxl357.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Adxl357/) [ADXL357 - 加速度计](devices/Adxl357/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ags01db.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ags01db/) [AGS01DB - MEMS VOC气体传感器](devices/Ags01db/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ahtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ahtxx/) [AHT10/15/20 - 温湿度传感器模块](devices/Ahtxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ak8963.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ak8963/) [AK8963 - 磁力计](devices/Ak8963/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Am2320.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Am2320/) [AM2320 - 温湿度传感器](devices/Am2320/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Amg88xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Amg88xx/) [AMG88xx红外阵列传感器系列](devices/Amg88xx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Apa102.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Apa102/) [APA102 - 双线传输集成控制LED](devices/Apa102/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.At24cxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.At24cxx/) [At24cxx系列I2C EEPROM](devices/At24cxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Axp192.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Axp192/) [AXP192 - 增强型单节Li电池和电源管理IC](devices/Axp192/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bh1745.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bh1745/) [Bh1745 - RGB传感器](devices/Bh1745/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bh1750fvi.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bh1750fvi/) [BH1750FVI - 环境光传感器](devices/Bh1750fvi/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmm150.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmm150/) [Bmm150 - 磁力计](devices/Bmm150/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmp180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmp180/) [BMP180 - 气压计、高度计和温度传感器](devices/Bmp180/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bmxx80.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bmxx80/) [BMxx80设备系列](devices/Bmxx80/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Bno055.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Bno055/) [BNO055 - 惯性测量单元](devices/Bno055/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Button.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Button/) [按钮](devices/Button/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Buzzer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Buzzer/) [蜂鸣器 - 压电蜂鸣器控制器](devices/Buzzer/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ccs811.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ccs811/) [CCS811气体传感器](devices/Ccs811/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.CharacterLcd.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.CharacterLcd/) [字符液晶显示器](devices/CharacterLcd/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Charlieplex.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Charlieplex/) [Charlieplex段绑定](devices/Charlieplex/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Chsc6540.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Chsc6540/) [CHSC6540 - 触摸屏控制器](devices/Chsc6540/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.DCMotor.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.DCMotor/) [直流电机控制器](devices/DCMotor/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.DhcpServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.DhcpServer/) [DHCP服务器](devices/DhcpServer/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Dhtxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Dhtxx/) [DHTxx - 数字输出相对湿度和温度传感器模块](devices/Dhtxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Dhtxx.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Dhtxx.Esp32/) [DHTxx.Esp32 - 数字输出相对湿度和温度传感器模块](devices/Dhtxx.Esp32/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.LiquidLevel.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.LiquidLevel/) [数字液位开关](devices/LiquidLevel/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ds1302.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ds1302/) [DS1302 - 实时时钟](devices/Ds1302/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ds1621.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ds1621/) [Ds1621 - 带可编程分辨率的1线数字温度计](devices/Ds1621/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ds18b20.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ds18b20/) [Ds18b20 - 温度传感器](devices/Ds18b20/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.ePaper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.ePaper/) [.NET nanoFramework的ePaper驱动程序](devices/ePaper/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ft6xx6x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ft6xx6x/) [Ft6xx6x - 触摸屏控制器](devices/Ft6xx6x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.ShiftRegister.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.ShiftRegister/) [通用移位寄存器](devices/ShiftRegister/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr04.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr04/) [HC-SR04 - 超声波测距模块](devices/Hcsr04/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr04.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr04.Esp32/) [HC-SR04 - 带 RMT 的 ESP32 超声波测距模块](devices/Hcsr04.Esp32/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hcsr501.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hcsr501/) [HC-SR501 - 人体红外传感器](devices/Hcsr501/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hdc1080.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hdc1080/) [Hdc1080 - 温湿度传感器](devices/Hdc1080/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hmc5883l.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hmc5883l/) [HMC5883L - 三轴数字罗盘](devices/Hmc5883l/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hts221.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hts221/) [HTS221 - 相对湿度和温度电容数字传感器](devices/Hts221/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Hx711.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Hx711/) [Hx711 (M5Stack WEIGHT)](devices/Hx711/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ina219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ina219/) [INA219 - 双向电流/功率监视器](devices/Ina219/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Multiplexing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Multiplexing/) [Iot.Device.Multiplexing](devices/Multiplexing/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.NumberHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.NumberHelper/) [Iot.Device.NumberHelper](devices/NumberHelper/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.WeatherHelper.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.WeatherHelper/) [Iot.Device.WeatherHelper](devices/WeatherHelper/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ip5306.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ip5306/) [IP5306 - 电源管理](devices/Ip5306/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.KeyMatrix.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.KeyMatrix/) [键盘矩阵](devices/KeyMatrix/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.SparkFunLcd.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.SparkFunLcd/) [SparkFun RGB 串行开放式 LCD 显示屏库（20x4 或 16x2 尺寸），带 I2C 连接](devices/SparkFunLcd/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.LidarLiteV3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.LidarLiteV3/) [LidarLiteV3 - 飞行时间激光雷达传感器](devices/LidarLiteV3/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lm75.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lm75/) [LM75 - 数字温度传感器](devices/Lm75/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lp3943.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lp3943/) [Lp3943 LED驱动器](devices/Lp3943/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lps25h.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lps25h/) [LPS25H - 压阻式压力和温度计传感器](devices/Lps25h/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Lsm9Ds1.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Lsm9Ds1/) [LSM9DS1 - 三轴加速度计、陀螺仪和磁力计](devices/Lsm9Ds1/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.AtomQrCode.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.AtomQrCode/) [M5Stack ATOM QR码阅读器](devices/AtomQrCode/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max31856.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max31856/) [Max31856 - 冷端补偿热电偶到数字转换器](devices/Max31856/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max31865.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max31865/) [MAX31865 - 电阻温度检测放大器](devices/Max31865/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max44009.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max44009/) [MAX44009 - 环境光传感器](devices/Max44009/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Max7219.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Max7219/) [Max7219 (LED 点阵驱动器)](devices/Max7219/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mbi5027.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mbi5027/) [MBI5027--带错误检测的16位移位寄存器](devices/Mbi5027/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp23xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp23xxx/) [Mcp23xxx - I/O 扩展器设备系列](devices/Mcp23xxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp25xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp25xxx/) [Mcp25xxx 设备系列 - CAN 总线](devices/Mcp25xxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp3428.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp3428/) [Mcp3428 - 模拟到数字转换器（I2C）](devices/Mcp3428/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp3xxx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp3xxx/) [MCP3xxx 系列的模拟到数字转换器](devices/Mcp3xxx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp7940xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp7940xx/) [Mcp7940xx - I2C 实时时钟/日历与 SRAM](devices/Mcp7940xx/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp960x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp960x/) [MCP960X - 冷端补偿热电偶到数字转换器系列设备](devices/Mcp960x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mcp9808.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mcp9808/) [MCP9808 - 数字温度传感器](devices/Mcp9808/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mfrc522.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mfrc522/) [MFRC522 - RFID 读取器](devices/Mfrc522/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mhz19b.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mhz19b/) [MH-Z19B CO2 传感器](devices/Mhz19b/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mlx90614.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mlx90614/) [MLX90614 - 红外温度计](devices/Mlx90614/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Relay4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Relay4/) [模块和单元 4 路继电器 - I2C 继电器](devices/Relay4/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpr121.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpr121/) [MPR121 - 接近电容触摸传感器控制器](devices/Mpr121/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpu9250.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpu9250/) [MPU6050/MPU6500/MPU9250 - 陀螺仪、加速度计、温度和磁力计（仅限 MPU9250）](devices/Mpu9250/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Mpu6886.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Mpu6886/) [Mpu6886 - 加速度计和陀螺仪](devices/Mpu6886/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.MS5611.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.MS5611/) [GY-63 模块中的 Ms5611 - 温度和压力传感器](devices/MS5611/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Nrf24l01.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Nrf24l01/) [nRF24L01 - 单芯片 2.4 GHz 收发器](devices/Nrf24l01/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pcx857x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pcx857x/) [NXP/TI PCx857x](devices/Pcx857x/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pca95x4.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pca95x4/) [Pca95x4 - I2C GPIO 扩展器](devices/Pca95x4/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pcd8544.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pcd8544/) [PCD8544 - 48 × 84 像素矩阵 LCD，著名的 Nokia 5110 屏幕](devices/Pcd8544/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pn5180.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pn5180/) [PN5180 - RFID 和 NFC 读写器](devices/Pn5180/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Pn532.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Pn532/) [PN532 - RFID 和 NFC 读写器](devices/Pn532/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.QtrSensors.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.QtrSensors/) [QTR 传感器 - Pololu QTR 反射式传感器](devices/QtrSensors/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RotaryEncoder.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RotaryEncoder/) [正交旋转编码器](devices/RotaryEncoder/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RotaryEncoder.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RotaryEncoder.Esp32/) [正交旋转编码器 (ESP32)](devices/RotaryEncoder.Esp32/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RadioReceiver.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RadioReceiver/) [无线电接收器](devices/RadioReceiver/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.RadioTransmitter.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.RadioTransmitter/) [无线电发射器](devices/RadioTransmitter/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Rtc.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Rtc/) [实时时钟](devices/Rtc/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Card.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Card/) [RFID 共用元件](devices/Card/README.md)
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Scd30.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Scd30/) Sensirion SCD30颗粒物传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sps30.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sps30/) Sensirion SPS30颗粒物传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.ServoMotor.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.ServoMotor/) 伺服电机
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sht3x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sht3x/) SHT3x温湿度传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Shtc3.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Shtc3/) SHTC3温湿度传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Si7021.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Si7021/) Si7021温湿度传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Sn74hc595.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Sn74hc595/) SN74HC595 8位移位寄存器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ssd13xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ssd13xx/) SSD13xx和SSH1106 OLED显示屏
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.SwarmTile.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.SwarmTile/) Swarm Tile
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Buffers.Binary.BinaryPrimitives.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Buffers.Binary.BinaryPrimitives/) System.Buffers.Binary.BinaryPrimitives
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Device.Model.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Device.Model/) System.Device.Model-设备绑定的属性
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Diagnostics.Stopwatch.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Diagnostics.Stopwatch/) System.Diagnostics.Stopwatch和DelayHelper
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Drawing.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Drawing/) System.Drawing
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Numerics.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Numerics/) System.Numerics
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tcs3472x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tcs3472x/) TCS3472x传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tlc1543.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tlc1543/) TLC1543-10位ADC与11个输入通道
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tm1637.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tm1637/) TM1637 - 数码管显示
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Tsl256x.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Tsl256x/) TSL256x - 光照传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Vl53L0X.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Vl53L0X/) VL53L0X - 距离传感器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ws28xx.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ws28xx/) Ws28xx LED驱动器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Ws28xx.Esp32.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Ws28xx.Esp32/) Ws28xx、SK6812 LED驱动器
* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Yx5300.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Yx5300/) YX5200/YX5300 - MP3播放器

## 文件夹结构

[/devices/](./devices/) 包含已经清理过的设备，应当可以直接使用。

[/src/devices_generated/](./src/devices_generated/) 包含自动从 [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices) 转移过来的设备。它们可能无法工作或编译，但如果您需要支持此处包含但 [/devices/](./devices/) 文件夹中缺失的设备，这些设备是不错的起点。

[/src/nanoFramework.IoT.Device.CodeConverter](./src/nanoFramework.IoT.Device.CodeConverter) 包含生成 [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices) 设备所用的工具。

[/src](./src) 中的其他文件夹包含了您创建/更新设备时可以引用的nanoFramework项目，提供了多种功能，例如StopWatach、DelayHelper、BinaryPrimitives或各种System.Device.Model属性。

## 贡献

**重要提示：** 如果您计划清理 [/src/devices_generated/](./src/devices_generated/) 中的代码，请将您的工作复制到 [/devices/](./devices/) 文件夹中，因为生成工具会覆盖 [/src/devices_generated/](./src/devices_generated/) 的内容。

请查看 [技巧详细列表](./tips-trick.md)，以方便迁移。生成器会处理一些沉重的工作，但仍需要一些手动调整。

我们使用以下结构进行绑定：

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

代码转换器可以帮助将 .NET Core/.NET 5.0 代码迁移到 .NET nanoFramework。有关更多信息和[如何定制和运行它的信息在这里](./src/nanoFramework.IoT.Device.CodeConverter/README.md)。

## 将 .NET nanoFramework 绑定移植到 .NET IoT

您知道吗？通过极少的努力，您可以使一个 nanoFramework 绑定也适用于 .NET IoT。有关采取的步骤的更多信息和指导，请参见[此文章](migrate-binding-to-dotnetiot.md)。

## 反馈和文档

有关文档、提供反馈、问题和了解如何贡献的信息，请参阅[主页存储库](https://github.com/nanoframework/Home)。

加入我们的 Discord 社区[此处](https://discord.gg/gCyBu8T)。

## 鸣谢

本项目的贡献者列表可以在 [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md) 中找到。

## 许可证

**nanoFramework** 类库基于 [MIT 许可证](LICENSE.md) 许可。

## 行为守则

本项目已采用贡献者公约定义的行为守则，以澄清我们社区中期望的行为。有关更多信息，请参见 [.NET 基金会行为守则](https://dotnetfoundation.org/code-of-conduct)。

### .NET 基金会

本项目得到了 [.NET 基金会](https://dotnetfoundation.org) 的支持。