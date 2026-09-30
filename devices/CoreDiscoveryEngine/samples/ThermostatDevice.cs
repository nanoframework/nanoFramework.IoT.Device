// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;

namespace Iot.Device.CoreDiscoveryEngine.Samples
{
    [Interface("Synthetic thermostat with controllable heating")]
    internal class ThermostatDevice
    {
        private readonly Random _random = new Random();
        private double _targetTemperature = 22.0;
        private bool _isHeating;

        [Property]
        public double TargetTemperature
        {
            get
            {
                return _targetTemperature;
            }

            set
            {
                _targetTemperature = value;
            }
        }

        public double CurrentTemperature
        {
            [Telemetry]
            get
            {
                if (_isHeating)
                {
                    return _targetTemperature - ((_random.Next(20) + 1) / 10.0);
                }

                return 18.0 + ((_random.Next(11) - 5) / 10.0);
            }
        }

        [Telemetry]
        public bool IsHeating
        {
            get
            {
                return _isHeating;
            }
        }

        [Command]
        public void StartHeating()
        {
            _isHeating = true;
        }

        [Command]
        public void StopHeating()
        {
            _isHeating = false;
        }
    }
}