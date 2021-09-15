# Mpu6886 - accelerometer and gyroscope

```csharp
public class Program
{
    public static void Main()
    {
        Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
        Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

        I2cConnectionSettings settings = new(1, 0x68);

        using (Mpu6886AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
        {
            Debug.WriteLine($"Temp {ag.GetTemperature()} C");

            while (true)
            {
                var acc = ag.GetAccelerometer();
                var gyr = ag.GetGyroscope();
                Debug.WriteLine($"ACC x:{acc.X} y:{acc.Y} z:{acc.Z}");
                Debug.WriteLine($"GYR x:{gyr.X} y:{gyr.Y} z:{gyr.Z}");
                Thread.Sleep(100);
            }
        }
    }
}
```

