using System;
using System.Numerics;

namespace Iot.Device.Imu
{
    public class GyroscopeAccelerometer

    {
        public GyroscopeAccelerometer(Vector3 gyro, Vector3 acc)
        {
            Gyroscope = gyro;
            Accelerometer = acc;
        }

        public Vector3 Gyroscope { get; set; }
        public Vector3 Accelerometer { get; set; }
    }
}
