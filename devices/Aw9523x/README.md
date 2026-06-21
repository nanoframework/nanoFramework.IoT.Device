# AW9523X - I2C GPIO and LED controller

AW9523X is a generic 16-channel I2C GPIO expander and LED controller. This binding intentionally exposes generic register and port operations so it can be used on any AW9523X-based board.

## Documentation

- [AW9523X datasheet](https://www.awinic.com/en/searchAll?keyword=AW9523)
- M5Stack CoreS3 [reference implementation](https://github.com/m5stack/M5Unified/blob/master/src/utility/Power_Class.cpp) (Power class)

## Usage

```csharp
using System.Device.I2c;
using Iot.Device.Aw9523x;

I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, Aw9523x.DefaultI2cAddress));
Aw9523x aw9523 = new Aw9523x(i2c);

byte chipId = aw9523.ChipId;
```

### Generic port access

```csharp
// Configure all pins on port 0 as outputs
aw9523.WriteDirectionPort(Port.Port0, 0x00);

// Set bit 1 on output port 0
aw9523.SetOutputBits(Port.Port0, OutputMask.PortBit1);

// Read current input state on port 1
byte inputs = aw9523.ReadInputPort(Port.Port1);
```

### CoreS3 usage pattern

On CoreS3, board-level functions are wired through AW9523X output bits. You can apply the same policy used by M5Unified in the application/sample layer:

```csharp
const OutputMask coreS3BusEn = OutputMask.PortBit1;
const OutputMask coreS3UsbOtgEn = OutputMask.PortBit5;
const OutputMask coreS3BoostEn = OutputMask.PortBit7;

// Enable internal bus power
aw9523.SetOutputBits(Port.Port0, coreS3BusEn);
aw9523.SetOutputBits(Port.Port1, coreS3BoostEn);

// Enable USB OTG output
aw9523.SetOutputBits(Port.Port0, coreS3UsbOtgEn);
aw9523.SetOutputBits(Port.Port1, coreS3BoostEn);
```
