// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Runtime time entity for time-of-day values (for example, a schedule or alarm time).
    /// The state is exchanged with Home Assistant as an ISO time string (HH:MM:SS).
    /// </summary>
    public sealed class HomeAssistantTime : HomeAssistantRuntimeEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantTime" /> class.
        /// </summary>
        /// <param name="discovery">Discovery entity definition.</param>
        /// <param name="initialState">Initial time as an ISO time string (HH:MM:SS).</param>
        /// <param name="publisher">Callback to publish MQTT messages.</param>
        public HomeAssistantTime(
            HomeAssistantDiscoveryEntity discovery,
            string initialState,
            HomeAssistantPublishDelegate publisher)
        {
            Initialize(discovery, initialState, publisher);
        }

        /// <summary>
        /// Gets the time of day, or <see cref="TimeSpan.Zero"/> if state is not a valid time.
        /// </summary>
        public TimeSpan Value
        {
            get
            {
                TimeSpan result;
                if (TryParse(State, out result))
                {
                    return result;
                }

                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Sets the time of day. Only the hours, minutes and seconds components are used.
        /// </summary>
        /// <param name="value">Time of day to publish.</param>
        public void SetValue(TimeSpan value)
        {
            PublishState(Format(value));
        }

        /// <summary>
        /// Formats a time of day as an ISO time string (HH:MM:SS).
        /// </summary>
        /// <param name="value">Time of day to format.</param>
        /// <returns>The time formatted as HH:MM:SS.</returns>
        internal static string Format(TimeSpan value)
        {
            return Pad(value.Hours) + ":" + Pad(value.Minutes) + ":" + Pad(value.Seconds);
        }

        private static bool TryParse(string text, out TimeSpan result)
        {
            result = TimeSpan.Zero;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            string[] parts = text.Trim().Split(':');
            if (parts.Length < 2 || parts.Length > 3)
            {
                return false;
            }

            int hours;
            int minutes;
            int seconds = 0;

            if (!int.TryParse(parts[0], out hours) || !int.TryParse(parts[1], out minutes))
            {
                return false;
            }

            if (parts.Length == 3)
            {
                // Ignore any fractional seconds (for example, "07:30:15.250").
                string secondsText = parts[2];
                int dot = secondsText.IndexOf('.');
                if (dot >= 0)
                {
                    secondsText = secondsText.Substring(0, dot);
                }

                if (!int.TryParse(secondsText, out seconds))
                {
                    return false;
                }
            }

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59 || seconds < 0 || seconds > 59)
            {
                return false;
            }

            result = new TimeSpan(hours, minutes, seconds);
            return true;
        }

        private static string Pad(int value)
        {
            return value < 10 ? "0" + value.ToString() : value.ToString();
        }
    }
}
