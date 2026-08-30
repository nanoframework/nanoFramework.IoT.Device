//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Device.Model;
using System.Threading;
using nanoFramework.Hardware.Esp32.Rmt;
using UnitsNet;

namespace Iot.Device.Hcsr04.Esp32
{
    /// <summary>
    /// HC-SR04 - Ultrasonic Ranging Module
    /// </summary>
    [Interface("HC-SR04 - Ultrasonic Ranging Module")]
    public class Hcsr04 : IDisposable
    {
        ReceiverChannel _rxChannel;
        TransmitterChannel _txChannel;
        long _lastMeasurment;
        RmtSymbols _triggerPulse = new RmtSymbols();

        const double _speedOfSound = 340.29;
        const int _resolutionHz = 1_000_000;

        /// <summary>
        /// Gets the current distance, usual range from 2 cm to 400 cm.
        /// </summary>
        [Telemetry]
        public Length Distance => GetDistance();

        /// <summary>
        /// Create an instance of the SR04 device class
        /// </summary>
        /// <param name="trigger">GPIO pin number for trigger pin</param>
        /// <param name="echo">GPIO pin number of echo pin</param>
        public Hcsr04(int trigger, int echo)
        {
            // Set-up TX & RX channels
            // We need to send a 10us pulse to initiate measurement
            var txChannelSettings = new TransmitChannelSettings(pinNumber: trigger)
            {
                // 1us clock = 1Mhz
                ResolutionHz = _resolutionHz,
                EnableCarrierWave = false,
                IdleLevel = false,
            };

            _txChannel = new TransmitterChannel(txChannelSettings);

            // we only need 1 pulse of 10 us high
            _triggerPulse.Add(new RmtSymbol(10, true, 0, false));

            // The received echo pulse width represents the distance to obstacle
            // 150us to 38ms
            var rxChannelSettings = new ReceiverChannelSettings(pinNumber: echo)
            {
                // 1us clock  = 1Mhz
                ResolutionHz = _resolutionHz,

                // filter out 50ns / noise
                FilterThreshold = 50,

                // 30ms 
                IdleThreshold = 30_000_000,

                // 100 millisecond timeout is enough
                ReceiveTimeout = TimeSpan.FromMilliseconds(100)
            };

            _rxChannel = new ReceiverChannel(rxChannelSettings);
        }

        /// <summary>
        /// Gets the current distance, usual range from 2 cm to 400 cm.
        /// </summary>
        private Length GetDistance()
        {
            // Retry at most 10 times.
            // Try method will fail when context switch occurs in the wrong moment
            // or something else (i.e. JIT, extra workload) causes extra delay.
            // Other situation is when distance is changing rapidly (i.e. moving hand in front of the sensor)
            // which is causing invalid readings.
            for (int i = 0; i < 10; i++)
            {
                if (TryGetDistance(out Length result))
                {
                    return result;
                }
            }

            throw new InvalidOperationException("Could not get reading from the sensor");
        }

        /// <summary>
        /// Try to gets the current distance, , usual range from 2 cm to 400 cm
        /// </summary>
        /// <param name="result">Length</param>
        /// <returns>True if success</returns>
        public bool TryGetDistance(out Length result)
        {
            RmtSymbols response = null;
            // Make sure we don't measure before the 60 ms
            while (DateTime.UtcNow.Ticks - _lastMeasurment < 60 * TimeSpan.TicksPerMillisecond)
            {
               if (DateTime.UtcNow.Ticks <= _lastMeasurment )
               {
                   _lastMeasurment = DateTime.UtcNow.Ticks;
                   continue;
               }
               
               Thread.Sleep(TimeSpan.FromTicks(DateTime.UtcNow.Ticks - _lastMeasurment));
            }

            _lastMeasurment = DateTime.UtcNow.Ticks;

            _rxChannel.Start();

            // Send 10us pulse
            _txChannel.Send(_triggerPulse, true);

            // Try 5 times to get valid response
            for (int count = 0; count < 5; count++)
            {
                response = _rxChannel.TryGetReceivedSymbols();
                if (response != null)
                {
                    break;
                }

                // Retry every 20 ms
                Thread.Sleep(20);
            }

            _rxChannel.Stop();

            if (response == null)
            {
                result = default;
                return false;
            }

            // Echo pulse width in micro seconds
            int duration = response[0].Duration0;

            // Calculate distance in meters
            // Distance calculated as  (speed of sound) * duration(meters) / 2 
            result = Length.FromMeters(_speedOfSound * duration / (1000000 * 2));

            if (result.Value > 4)
            {
                // result is more than sensor supports
                // something went wrong
                result = default;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_txChannel != null)
            {
                _txChannel.Dispose();
                _txChannel = null;
            }

            if (_rxChannel != null)
            {
                _rxChannel.Dispose();
                _rxChannel = null;
            }

        }
    }
}
