# KT0803 - Radio Transmitter
KT0803, a new generation of low cost Monolithic Digital FM Transmitter, is designed to process high-fidelity stereo audio signal and transmit modulated FM signal over a short range. 

## Sensor Image
![](sensor.jpg)

## Usage
```C#
I2cConnectionSettings settings = new I2cConnectionSettings(1, Kt0803.DefaultI2cAddress);
I2cDevice device = I2cDevice.Create(settings);

// The radio is running on FM 106.6MHz
using (Kt0803 radio = new Kt0803(device, 106.6, Region.China))
{
    // Connect Raspberry Pi or other sound sources to the 3.5mm earphone jack of the module
}
```