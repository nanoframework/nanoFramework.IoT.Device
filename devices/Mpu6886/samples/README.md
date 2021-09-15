# Mpu6886 - accelerometer and gyroscope

```csharp
public class Program
{
    public static void Main()
    {
        // I2C pins need to be configured, for example for pin 22 & 21 for 
        // the M5StickC Plus. These pins might be different for other boards.
        Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
        Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

        I2cConnectionSettings settings = new(1, 0x68);

        using (Mpu6886AccelerometerGyroscope ag = new(I2cDevice.Create(settings)))
        {
            Debug.WriteLine($"Internal temperature: {ag.GetTemperature()} C");

            while (true)
            {
                var acc = ag.GetAccelerometer();
                var gyr = ag.GetGyroscope();
                Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
                Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
                Thread.Sleep(100);
            }
        }
    }
}

```

## Output

```text
Internal temperature: 64.10954712 C
Accelerometer data x:15.95117187 y:0 z:1.060791015
Gyroscope data x:2.68554687 y:3994.38476562 z:3990.35644531

Accelerometer data x:15.95190429 y:0 z:1.05859375
Gyroscope data x:2.74658203 y:3993.89648437 z:3989.8071289

Accelerometer data x:15.95019531 y:0 z:1.059814453
Gyroscope data x:2.68554687 y:3994.75097656 z:3990.60058593

Accelerometer data x:15.95361328 y:15.99853515 z:1.061035156
Gyroscope data x:3.4790039 y:3993.95751953 z:3989.92919921

Accelerometer data x:15.95703125 y:15.99902343 z:1.063232421
Gyroscope data x:3.051757812 y:3994.44580078 z:3989.8071289
```
