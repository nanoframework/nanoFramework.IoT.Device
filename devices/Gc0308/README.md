# GC0308 - 0.3MP CMOS Camera Sensor

The GC0308 is a 0.3 megapixel (VGA, 640×480) CMOS camera sensor from GalaxyCore. It communicates via SCCB (Serial Camera Control Bus, I2C-compatible) for register configuration and outputs pixel data through an 8-bit DVP (Digital Video Port) parallel interface. This sensor is found on the M5Stack CoreS3 board.

## Documentation

- [GC0308 Datasheet (PDF)](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/K128%20CoreS3/GC0308.PDF)
- [M5Stack CoreS3 Documentation](https://docs.m5stack.com/en/core/CoreS3)
- [CoreS3 Schematic (PDF)](https://m5stack-doc.oss-cn-shenzhen.aliyuncs.com/490/Sch_M5_CoreS3_v1.0.pdf)

## Usage

**Important**: Make sure you properly set up the I2C pins especially for ESP32 before creating the `I2cDevice`. Install the `nanoFramework.Hardware.ESP32` NuGet:

```csharp
//////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the I2C GPIOs
// On M5Stack CoreS3: SDA = GPIO 12, SCL = GPIO 11
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, use the preset pins for the I2C bus you want to use.

### Basic Initialization

```csharp
using Gc0308 camera = new(I2cDevice.Create(new I2cConnectionSettings(1, Gc0308.DefaultI2cAddress)));

// Verify chip identity
byte chipId = camera.ChipId;
Debug.WriteLine($"Chip ID: 0x{chipId:X2} (expected 0x9B)");
```

The constructor automatically initializes the sensor with default settings: VGA (640×480), YCbCr 4:2:2, auto exposure, auto white balance.

### Output Format

```csharp
// RGB565 (2 bytes/pixel, good for displays)
camera.OutputFormat = OutputFormat.Rgb565;

// YCbCr 4:2:2 (2 bytes/pixel, default)
camera.OutputFormat = OutputFormat.YCbCr422;

// Grayscale (Y channel only, 1 byte/pixel)
camera.OutputFormat = OutputFormat.Grayscale;
```

### Resolution

```csharp
// VGA 640x480 (default, full sensor)
camera.SetResolution(Resolution.Vga640x480);

// QVGA 320x240 (good balance of detail and memory)
camera.SetResolution(Resolution.Qvga320x240);

// QQVGA 160x120 (minimal memory footprint)
camera.SetResolution(Resolution.Qqvga160x120);

// CIF 352x288
camera.SetResolution(Resolution.Cif352x288);
```

### Image Quality

```csharp
// AEC target (controls brightness when auto-exposure is enabled)
// 0x48 = default, higher = brighter, lower = darker
camera.AecTarget = 0x58;    // Slightly brighter

// Contrast (0x40 = 1.0x default)
camera.Contrast = 0x50;    // Increased contrast

// Saturation (0x40 = 1.0x default, 0x00 = grayscale)
camera.Saturation = 0x60;  // More vivid colors
```

### White Balance

```csharp
// Automatic (sensor adjusts continuously)
camera.SetWhiteBalance(WhiteBalanceMode.Auto);

// Manual presets for specific lighting conditions
camera.SetWhiteBalance(WhiteBalanceMode.Sunny);
camera.SetWhiteBalance(WhiteBalanceMode.Cloudy);
camera.SetWhiteBalance(WhiteBalanceMode.Office);
camera.SetWhiteBalance(WhiteBalanceMode.Home);
```

### Special Effects

```csharp
camera.SetSpecialEffect(SpecialEffect.Normal);     // No effect
camera.SetSpecialEffect(SpecialEffect.Grayscale);   // Black & white
camera.SetSpecialEffect(SpecialEffect.Sepia);        // Antique brown tone
camera.SetSpecialEffect(SpecialEffect.Negative);     // Color inversion
camera.SetSpecialEffect(SpecialEffect.GreenTint);    // Green overlay
camera.SetSpecialEffect(SpecialEffect.BlueTint);     // Blue overlay
camera.SetSpecialEffect(SpecialEffect.RedTint);      // Red overlay
```

### Mirror / Flip

```csharp
camera.Orientation = MirrorFlip.None;              // Normal
camera.Orientation = MirrorFlip.HorizontalMirror;  // Left-right swap
camera.Orientation = MirrorFlip.VerticalFlip;       // Top-bottom swap
camera.Orientation = MirrorFlip.Both;               // Both mirror and flip
```

### Custom Window (Region of Interest)

```csharp
// Capture a 320x240 region starting at offset (100, 50) on the sensor
camera.SetWindow(100, 50, 320, 240);
```

### Test Pattern

```csharp
// Enable built-in color bar pattern (useful for testing DVP data path)
camera.SetTestPattern(true);

// Back to normal camera output
camera.SetTestPattern(false);
```

### Working with Frame Data

The `CameraFrame` class provides pixel access for captured frame data with support for RGB565 and YCbCr 4:2:2 color space conversion:

```csharp
// After capturing frame data from the DVP interface / camera controller:
byte[] rawFrameData = GetFrameFromCameraController(); // Platform-specific

// Wrap in CameraFrame for pixel access
CameraFrame frame = CameraFrame.FromRgb565(rawFrameData, 320, 240);

// Read a pixel color
Color pixel = frame.GetPixel(160, 120);
Debug.WriteLine($"Center pixel: R={pixel.R}, G={pixel.G}, B={pixel.B}");

// Modify a pixel
frame.SetPixel(0, 0, Color.Red);
```

## Features

- **SCCB/I2C register configuration**: Full control over all sensor settings
- **Multiple output formats**: YCbCr 4:2:2, RGB565, Grayscale
- **Resolution presets**: VGA (640×480), QVGA (320×240), QQVGA (160×120), CIF (352×288)
- **Custom windowing**: Arbitrary region-of-interest capture
- **Auto exposure control** (AEC): Automatic brightness adjustment
- **Auto white balance** (AWB): Automatic color temperature correction with manual presets
- **Image adjustments**: Contrast, saturation, AEC target (brightness)
- **Special effects**: Grayscale, sepia, negative, color tinting
- **Mirror/flip**: Horizontal mirror, vertical flip, or both
- **Test pattern**: Built-in color bar generator for debugging
- **Frame data processing**: `CameraFrame` class with RGB565/YCbCr pixel conversion

## Limitations

- **Frame capture**: This driver provides SCCB register configuration only. Actual frame capture from the DVP interface requires platform-specific camera controller support (e.g., ESP32-S3 LCD_CAM peripheral). The 8-bit parallel data at pixel-clock speeds cannot be read via GPIO.
- **ESP32 camera API**: Native nanoFramework camera support is required for end-to-end image capture. This driver is designed to integrate with such support when available.

## M5Stack CoreS3

On the M5Stack CoreS3, the GC0308 is at I2C address 0x21 on the internal system I2C bus, shared with other peripherals (AXP2101, BMI270, BM8563, LTR-553ALS-WA, etc.). The sensor is paired with the LTR-553ALS-WA proximity sensor on the same ribbon cable.

| Signal | GPIO |
| --- | --- |
| I2C SDA | G12 |
| I2C SCL | G11 |
| PCLK | G45 |
| VSYNC | G46 |
| HREF | G38 |
| D0–D7 | G39, G40, G41, G42, G15, G16, G48, G47 |
| RESET | -1 (not connected) |
| PWDN | -1 (not connected) |

Related CoreS3 device drivers in this repository:

- [LTR-553ALS-WA](../Ltr553AlsWa) (proximity/ambient light, shared ribbon cable)
- [AXP2101](../Axp2101) (power management)
- [BMI270](../Bmi270) (6-axis IMU)
- [BMM150](../Bmm150) (magnetometer)
