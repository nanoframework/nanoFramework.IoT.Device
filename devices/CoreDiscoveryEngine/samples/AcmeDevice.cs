// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Model;

namespace Iot.Device.CoreDiscoveryEngine.Samples
{
    [Interface("Acme synthetic sensor/actuator")]
    internal class AcmeDevice
    {
        private int _samplingRate = 10;
        private double _threshold = 1.5;

        public double Temperature
        {
            [Telemetry]
            get
            {
                return 21.0;
            }
        }

        [Telemetry("Uptime")]
        public int GetUptimeSeconds()
        {
            return 42;
        }

        public string FirmwareVersion
        {
            [Property]
            get
            {
                return "1.0.0-synthetic";
            }
        }

        public int SamplingRate
        {
            [Property]
            get
            {
                return _samplingRate;
            }

            [Property]
            set
            {
                _samplingRate = value;
            }
        }

        [Property("Threshold")]
        public double GetThreshold()
        {
            return _threshold;
        }

        [Property("Threshold")]
        public void SetThreshold(double value)
        {
            _threshold = value;
        }

        [Command]
        public void Reset()
        {
            _samplingRate = 10;
        }

        [Command]
        public bool Calibrate(double offset, double scale, int iterations)
        {
            return iterations > 0 && scale != 0 && offset >= 0;
        }
    }
}