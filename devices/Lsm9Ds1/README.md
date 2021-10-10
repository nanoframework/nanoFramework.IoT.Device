# LSM9DS1 - 3D accelerometer, gyroscope and magnetometer

LSM9DS1 internally uses 2 buses:

- Accelerometer and gyroscope
- Magnetometer

and therefore functionality has been split into 2 classes which allows using them independently.

## Documentation

- You can find the datasheet [here](https://www.st.com/resource/en/datasheet/lsm9ds1.pdf)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

### Accelerometer and gyroscope

```csharp
class Program
{
    public const int I2cAddress = 0x6A;

    static void Main(string[] args)
    {
        using (var ag = new Lsm9Ds1AccelerometerAndGyroscope(CreateI2cDevice()))
        {
            while (true)
            {
                Debug.WriteLine($"Acceleration={ag.Acceleration}");
                Debug.WriteLine($"AngularRate={ag.AngularRate}");
                Thread.Sleep(100);
            }
        }
    }

    private static I2cDevice CreateI2cDevice()
    {
        var settings = new I2cConnectionSettings(1, I2cAddress);
        return I2cDevice.Create(settings);
    }
}
```

### Magnetometer

```csharp
class Magnetometer
{
    public const int I2cAddress = 0x1C;

    public static void Run()
    {
        using (var m = new Lsm9Ds1Magnetometer(CreateI2cDevice()))
        {
            Debug.WriteLine("Calibrating...");
            Debug.WriteLine("Move the sensor around Z for the next 20 seconds, try covering every angle");
            Stopwatch sw = Stopwatch.StartNew();
            Vector3 min = m.MagneticInduction;
            Vector3 max = m.MagneticInduction;
            while (sw.ElapsedMilliseconds < 20 * 1000)
            {
                Vector3 sample = m.MagneticInduction;
                min = Vector3.Min(min, sample);
                max = Vector3.Max(max, sample);
                Thread.Sleep(50);
            }
            Debug.WriteLine("Stop moving for some time...");
            Thread.Sleep(3000);
            const int intervals = 32;
            bool[,] data = new bool[32,32];
            Vector3 size = max - min;
            int n = 0;
            while (true)
            {
                n++;
                Vector3 sample = m.MagneticInduction;
                Vector3 pos = Vector3.Divide(Vector3.Multiply((sample - min), intervals - 1), size);
                int x = Math.Clamp((int)pos.X, 0, intervals - 1);
                int y = Math.Clamp((int)pos.Y, 0, intervals - 1);
                data[x, y] = true;
                if (n % 10 == 0)
                {
                    Debug.WriteLine("Now move the sensor around again but slower...");
                    for (int i = 0; i < intervals; i++)
                    {
                        for (int j = 0; j < intervals; j++)
                        {
                            if (i == x && y == j)
                            {
                                Debug.Write('#');
                            }
                            else
                            {
                                Debug.Write(data[i, j] ? '#' : ' ');
                            }
                        }
                        Debug.WriteLine();
                    }
                }
                Thread.Sleep(50);
            }
        }
    }

    private static I2cDevice CreateI2cDevice()
    {
        var settings = new I2cConnectionSettings(1, I2cAddress);
        return I2cDevice.Create(settings);
    }
}
```
