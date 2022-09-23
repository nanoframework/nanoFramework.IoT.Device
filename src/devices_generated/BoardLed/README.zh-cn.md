文档语言: [English](README.md) | [简体中文](README.zh-cn.md)

# On-board LED 驱动程序

该绑定用于控制板上led。

## 使用

```C#
// 获取板上led的所有boarded实例。
IEnumerable<BoardLed> leds = BoardLed.EnumerateLeds();

// 打开具有指定名称的LED。
BoardLed led = new BoardLed("led0");

// 获取当前LED的所有触发器。
IEnumerable<string> triggers = led.EnumerateTriggers();

// 设置触发器。
// 内核提供了一些触发器，让内核控制LED。
// 例如树莓派的红灯，它的触发器是“default-on”，它会一直亮着。
// 如果要操作LED，需要去掉触发器，即将其触发器设置为“none”。
led.Trigger = "none";

// 获取当前LED的最大亮度。
int maxBrightness = led.MaxBrightness;

// 设置亮度。
led.Brightness = 255;
```
