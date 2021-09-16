# Bmm150 - Magnetometer

The Bmm150 is a magnetometer that can be controlled either thru I2C either thru SPI. 
This implementation was tested in a ESP32 platform, specificaly in a  [M5Stack Gray](https://shop.m5stack.com/products/grey-development-core).

## Reference

Documentation for the Bmm150 can be [found here](https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmm150-ds001.pdf)

## Usage

You can find an example in the [sample](./samples/Bmm150.sample.cs) directory. Usage is straight forward including the possibility to have a calibration.

```csharp
// The I2C pins 21 and 22 in the sample below are ESP32 specific and may differ from other platforms.
// Please double check your device datasheet.
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cConnectionSettings mpui2CConnectionSettingmpus = new(1, Bmm150.DefaultI2cAddress);

using Bmm150 bmm150 = new Bmm150(I2cDevice.Create(mpui2CConnectionSettingmpus));

Debug.WriteLine($"Please move your device in all directions...");

bmm150.CalibrateMagnetometer();

Debug.WriteLine($"Calibration completed.");

while (true)
{
    Vector3 magne = bmm150.ReadMagnetometer(true, TimeSpan.FromMilliseconds(11));

    var offset = bmm150.CalibrationCompensation;
    var head_dir = Math.Atan2(magne.X - offset.X, magne.Y - offset.Y) * 180.0 / Math.PI;

    Debug.WriteLine($"Mag data: X={magne.X,15}, Y={magne.Y,15}, Z={magne.Z,15}, head_dir: {head_dir}");

    Thread.Sleep(100);
}
```

### Expected output

```console
Please move your device in all directions...
Calibration completed.
Mag data: X=   -14.47845935, Y=   -25.84130096, Z=     3.21418666, head_dir: 53.048122434
Mag data: X=    -8.61376667, Y=   -34.27179336, Z=      5.6248269, head_dir: 53.20645987
Mag data: X=     8.24737358, Y=   -24.37557029, Z=   -14.46572494, head_dir: 53.23982193
Mag data: X=    16.67771911, Y=   -13.74537277, Z=   -21.69576072, head_dir: 53.1867377
Mag data: X=    25.47522163, Y=     0.54982489, Z=   -23.30589103, head_dir: 53.092716739
Mag data: X=    38.30377197, Y=    29.87327575, Z=    -5.22305345, head_dir: 52.85469147
Mag data: X=   39.036857604, Y=    34.63833999, Z=    8.035467147, head_dir: 52.80400464
Mag data: X=    40.50302886, Y=    37.93722915, Z=     17.6780281, head_dir: 52.77772207
Mag data: X=    39.40196228, Y=    34.63707351, Z=   22.091781616, head_dir: 52.80735485
Mag data: X=    37.20346069, Y=    30.60580253, Z=     23.7015419, head_dir: 52.83581966
Mag data: X=    41.23611831, Y=   28.040559768, Z=    20.89221382, head_dir: 52.90353415
Mag data: X=   43.069618225, Y=     26.9414196, Z=    18.88580703, head_dir: 52.93349136
Mag data: X=    47.83301925, Y=    15.57780361, Z=    15.26539993, head_dir: 53.11403375
Mag data: X=    46.73426437, Y=   10.079939842, Z=    14.46384048, head_dir: 53.17062529
Mag data: X=      47.100811, Y=     6.41450691, Z=    15.66916084, head_dir: 53.21839222
Mag data: X=  46.0020179748, Y=   -2.016024589, Z=    20.49311065, head_dir: 53.3108490
```

## Calibration

You can get access perfom calibration thru the ```CalibrateMagnetometer``` function which will. Be aware that the calibration takes a few seconds.

```csharp
bmm150.CalibrateMagnetometer();
```

If no calibration is performed, you will get a raw data cloud which looks like this:

![raw data](./rawcalib.png)

Running the calibration properly require to **move the sensor in all the possible directions** while performing the calibration. You should consider running it with enough samples, at least few hundreds. The default is set to 100. While moving the sensor in all direction, far from any magnetic field, you will get the previous clouds. Calculating the average from those clouds and subtracting it from the read value will give you a centered cloud of data like this:

![raw data](./corrcalib.png)

To create those cloud point graphs, every cloud is a coordinate of X-Y, Y-Z and Z-X. 

Once the calibration is done, you will be able to read the data with the bias corrected using the ```ReadMagnetometer``` function. You will still be able to read the data without any calibration using the ```ReadMagnetometerWithoutCalibration``` function.

## Not supported/implemented features of the Bmm150

* Device Self-Tests
* Device Reset
* Toggle operation modes (defaults to normal mode)


## Testing

Unit tests project is in \Bmm150.tests. You can use VS2019 built-in test capabilites as follows:

![unit tests](./vs2019_unit_tests.png)

```csharp
[TestMethod]
public void TestCompensateVector3()
{
    uint rhall = 42;
    Vector3 rawMagnetormeterData = new Vector3 { X = 13.91375923, Y = -28.74289894, Z = 10.16711997 };
    Bmm150TrimRegisterData trimRegisterData = new Bmm150TrimRegisterData()
    {
        dig_x1 = 0,
        dig_x2 = 26,
        dig_xy1 = 29,
        dig_xy2 = -3,
        dig_xyz1 = 7053,
        dig_y1 = 0,
        dig_y2 = 26,
        dig_z1 = 24747,
        dig_z2 = 763,
        dig_z3 = 0,
        dig_z4 = 0
    };

    double x = Bmm150Compensation.Compensate_x(rawMagnetormeterData.X, rhall, trimRegisterData);
    double y = Bmm150Compensation.Compensate_y(rawMagnetormeterData.Y, rhall, trimRegisterData);
    double z = Bmm150Compensation.Compensate_z(rawMagnetormeterData.Z, rhall, trimRegisterData);

    // Calculated value should be: -1549.91882323
    Assert.Equal(Math.Ceiling(x), Math.Ceiling(-1549.918823), "Unexpected x-axis value.");

    // Calculated value should be: 3201.80615234
    Assert.Equal(Math.Ceiling(y), Math.Ceiling(3201.80615234), "Unexpected y-axis value.");

    // Calculated value should be: 26.20077896
    Assert.Equal(Math.Ceiling(z), Math.Ceiling(26.20077896), "Unexpected z-axis value.");
}
```

