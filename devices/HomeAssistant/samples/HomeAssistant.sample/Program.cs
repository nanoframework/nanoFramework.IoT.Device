// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using nanoFramework.M2Mqtt.Messages;
using nanoFramework.Runtime.Native;
using nanoFramework.HomeAssistant;
using nanoFramework.Hardware.Esp32;

namespace nanoSprinkler
{
    /// <summary>
    /// Headless sprinkler controller application without web server or AP provisioning.
    /// Requires valid Wi-Fi credentials in config.json.
    /// </summary>
    public class Program
    {
        private const int HealthCheckIntervalMs = 5_000;
        private const int MqttReconnectDelayMs = 1_000;
        private const int MaxMqttReconnectAttempts = 3;
        private const int MaxConsecutiveRecoveryFailures = 3;
        private const int RecoveryDeepSleepSeconds = 1;
        private const int ForegroundIdleSleepMs = 1_000;
        private const int RegularGcIntervalMs = 30_000;
        private const int MinTimerSeconds = 1;
        private const int MaxTimerSeconds = 3600;
        private const int DefaultTimerOnSeconds = 30;
        private const int DefaultTimerOffSeconds = 60;
        private const string BootMarkerPath = "I:\\boot-marker.txt";

        private static readonly object StateLock = new object();
        private static readonly object ConnectivityLock = new object();
        private static readonly object ConfigLock = new object();

        private static GpioController _gpio;
        private static GpioPin _relay;
        private static HomeAssistantClient _homeAssistant;

        // Entity references for state publishing
        private static HomeAssistantSwitch _sprinklerSwitch;
        private static HomeAssistantSwitch _timerSwitch;
        private static HomeAssistantNumber _timerOnNumber;
        private static HomeAssistantNumber _timerOffNumber;
        private static HomeAssistantNumber _relayPinNumber;
        private static HomeAssistantSwitch _relayActiveHighSwitch;
        private static HomeAssistantTextItem _bootMarkerSensor;

        private static DeviceConfig _config;

        private static bool _sprinklerOn;
        private static bool _timerModeEnabled;
        private static int _timerOnSeconds = DefaultTimerOnSeconds;
        private static int _timerOffSeconds = DefaultTimerOffSeconds;
        private static int _relayPin;
        private static bool _relayActiveHigh;

        private static bool _mqttReconnectRequested;
        private static int _consecutiveRecoveryFailures;

        private static string _bootMarker;
        private static long _nextGcTicks;

        /// <summary>
        /// Application entry point.
        /// </summary>
        public static void Main()
        {
            try
            {
                Debug.WriteLine("nanoSprinkler (headless) starting...");

                LoadConfiguration();
                _bootMarker = ReadAndClearBootMarker();

                // In this headless app, Wi-Fi credentials must be provided in config.json.
                if (string.IsNullOrEmpty(_config.WifiSsid))
                {
                    throw new InvalidOperationException("Wi-Fi SSID is missing in config.json.");
                }

                bool credentialsSaved = Wireless80211.SaveCredentials(_config.WifiSsid, _config.WifiPassword);
                if (!credentialsSaved)
                {
                    Debug.WriteLine("Failed to save Wi-Fi credentials. Startup aborted before reconnect.");
                    throw new InvalidOperationException("Unable to persist Wi-Fi credentials.");
                }

                InitializeRelay();
                InitializeHomeAssistantComponent();

                if (!Wireless80211.Reconnect(20_000))
                {
                    Debug.WriteLine("Initial Wi-Fi reconnect failed. Maintenance loop will retry.");
                }

                RequestMqttReconnect("startup");

                new Thread(MqttMaintenanceLoop).Start();
                new Thread(TimerWorker).Start();

                Debug.WriteLine("nanoSprinkler (headless) is running.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Global exception: " + ex.Message);
                RebootByTimedDeepSleep("Unhandled exception in Main (headless).");
            }

            RunForegroundIdleLoop();
        }

        private static void RunForegroundIdleLoop()
        {
            while (true)
            {
                Thread.Sleep(ForegroundIdleSleepMs);
                RunPeriodicGarbageCollection(false);
            }
        }

        private static void RunPeriodicGarbageCollection(bool force)
        {
            long nowTicks = DateTime.UtcNow.Ticks;

            if (!force && nowTicks < _nextGcTicks)
            {
                return;
            }

            try
            {
                nanoFramework.Runtime.Native.GC.Run(true);
            }
            catch
            {
            }

            _nextGcTicks = nowTicks + (RegularGcIntervalMs * TimeSpan.TicksPerMillisecond);
        }

        #region Relay Control

        private static void InitializeRelay()
        {
            _gpio = new GpioController();
            ForceRelayOffOnBootPins();
            _relay = _gpio.OpenPin(_relayPin, PinMode.Output);
            WriteRelay(false);
            _sprinklerOn = false;
            _timerModeEnabled = _config != null && _config.TimerModeEnabled;
        }

        private static void ForceRelayOffOnBootPins()
        {
            // Safety on boot: force relay outputs to OFF regardless of previous app state.
            ForceRelayOffOnPin(_relayPin);
        }

        private static void ForceRelayOffOnPin(int pin)
        {
            if (!IsValidRelayPin(pin))
            {
                return;
            }

            GpioPin pinHandle = null;

            try
            {
                pinHandle = _gpio.OpenPin(pin, PinMode.Output);
                pinHandle.Write(_relayActiveHigh ? PinValue.Low : PinValue.High);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Relay boot-off failed on pin " + pin + ": " + ex.Message);
            }
            finally
            {
                try
                {
                    if (pinHandle != null)
                    {
                        pinHandle.Dispose();
                    }
                }
                catch
                {
                }
            }
        }

        private static void SetRelayPin(int pin)
        {
            if (!IsValidRelayPin(pin))
            {
                return;
            }

            bool changed = false;
            bool sprinklerOn;
            GpioPin newRelay = null;
            GpioPin oldRelay = null;

            lock (StateLock)
            {
                if (_relayPin == pin)
                {
                    return;
                }

                sprinklerOn = _sprinklerOn;

                try
                {
                    // Open and configure the new pin first. This avoids dropping relay control
                    // if opening the new pin fails for any reason.
                    newRelay = _gpio.OpenPin(pin, PinMode.Output);
                    WriteRelayToPin(newRelay, sprinklerOn);

                    oldRelay = _relay;
                    _relay = newRelay;
                    _relayPin = pin;
                    changed = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to switch relay pin to " + pin + ": " + ex.Message);

                    try
                    {
                        if (newRelay != null)
                        {
                            newRelay.Dispose();
                        }
                    }
                    catch
                    {
                    }

                    return;
                }
            }

            // Dispose the previous pin handle only after successful swap.
            try
            {
                if (oldRelay != null)
                {
                    // Ensure the previous relay pin is de-energized before releasing it.
                    WriteRelayToPin(oldRelay, false);
                    oldRelay.Dispose();
                }
            }
            catch
            {
            }

            if (changed)
            {
                lock (ConfigLock)
                {
                    _config.RelayPin = pin;
                    ConfigManager.Save(_config);
                }

                PublishAllState();
            }
        }


        private static void SetRelayActiveHigh(bool activeHigh)
        {
            bool changed = false;
            bool sprinklerOn = false;

            lock (StateLock)
            {
                if (_relayActiveHigh != activeHigh)
                {
                    _relayActiveHigh = activeHigh;
                    sprinklerOn = _sprinklerOn;
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            if (_relay != null)
            {
                WriteRelay(sprinklerOn);
            }

            lock (ConfigLock)
            {
                _config.RelayActiveHigh = activeHigh;
                ConfigManager.Save(_config);
            }

            PublishAllState();
        }

        private static bool IsValidRelayPin(int pin)
        {
            // ESP32 GPIO34-GPIO39 are input-only, so they can't drive relay outputs.
            return pin >= 0 && pin <= 33;
        }

        private static void WriteRelay(bool on)
        {
            if (_relay == null)
            {
                return;
            }

            WriteRelayToPin(_relay, on);
        }

        private static void WriteRelayToPin(GpioPin relayPin, bool on)
        {
            PinValue value;
            if (_relayActiveHigh)
            {
                value = on ? PinValue.High : PinValue.Low;
            }
            else
            {
                value = on ? PinValue.Low : PinValue.High;
            }

            relayPin.Write(value);
        }

        #endregion

        #region Configuration Management

        private static void LoadConfiguration()
        {
            _config = ConfigManager.Load();
            DeviceConfig defaults = new DeviceConfig();

            if (string.IsNullOrEmpty(_config.MqttBroker))
            {
                _config.MqttBroker = defaults.MqttBroker;
            }

            if (_config.MqttPort <= 0)
            {
                _config.MqttPort = defaults.MqttPort;
            }

            if (_config.TimerOnSeconds <= 0)
            {
                _config.TimerOnSeconds = DefaultTimerOnSeconds;
            }
            else
            {
                _config.TimerOnSeconds = ClampTimerSeconds(_config.TimerOnSeconds);
            }

            if (_config.TimerOffSeconds <= 0)
            {
                _config.TimerOffSeconds = DefaultTimerOffSeconds;
            }
            else
            {
                _config.TimerOffSeconds = ClampTimerSeconds(_config.TimerOffSeconds);
            }

            if (!IsValidRelayPin(_config.RelayPin))
            {
                _config.RelayPin = defaults.RelayPin;
            }

            lock (StateLock)
            {
                _timerOnSeconds = _config.TimerOnSeconds;
                _timerOffSeconds = _config.TimerOffSeconds;
                _timerModeEnabled = _config.TimerModeEnabled;
                _relayPin = _config.RelayPin;
                _relayActiveHigh = _config.RelayActiveHigh;
            }
        }

        #endregion

        #region Timer Worker

        private static void ToggleTimerMode()
        {
            bool enable;

            lock (StateLock)
            {
                enable = !_timerModeEnabled;
            }

            SetTimerMode(enable);
        }

        private static void SetTimerMode(bool enable)
        {
            bool shouldPublish = false;
            bool shouldSave = false;

            lock (StateLock)
            {
                if (_timerModeEnabled != enable)
                {
                    _timerModeEnabled = enable;
                    shouldPublish = true;
                    shouldSave = true;
                }

                if (!enable && _sprinklerOn)
                {
                    _sprinklerOn = false;
                    WriteRelay(false);
                    shouldPublish = true;
                }
            }

            if (shouldSave)
            {
                lock (ConfigLock)
                {
                    _config.TimerModeEnabled = enable;
                    ConfigManager.Save(_config);
                }
            }

            if (shouldPublish)
            {
                PublishAllState();
            }
        }

        private static void SetSprinkler(bool on)
        {
            bool changed = false;

            lock (StateLock)
            {
                if (_sprinklerOn != on)
                {
                    _sprinklerOn = on;
                    WriteRelay(on);
                    changed = true;
                }
            }

            if (changed)
            {
                PublishAllState();
            }
        }

        private static void SetTimerOnSeconds(int seconds)
        {
            seconds = ClampTimerSeconds(seconds);

            bool changed = false;
            lock (StateLock)
            {
                if (_timerOnSeconds != seconds)
                {
                    _timerOnSeconds = seconds;
                    changed = true;
                }
            }

            if (changed)
            {
                lock (ConfigLock)
                {
                    _config.TimerOnSeconds = seconds;
                    ConfigManager.Save(_config);
                }
                PublishAllState();
            }
        }

        private static void SetTimerOffSeconds(int seconds)
        {
            seconds = ClampTimerSeconds(seconds);

            bool changed = false;
            lock (StateLock)
            {
                if (_timerOffSeconds != seconds)
                {
                    _timerOffSeconds = seconds;
                    changed = true;
                }
            }

            if (changed)
            {
                lock (ConfigLock)
                {
                    _config.TimerOffSeconds = seconds;
                    ConfigManager.Save(_config);
                }
                PublishAllState();
            }
        }

        private static void TimerWorker()
        {
            while (true)
            {
                bool enabled;
                int onSeconds;
                int offSeconds;

                lock (StateLock)
                {
                    enabled = _timerModeEnabled;
                    onSeconds = _timerOnSeconds;
                    offSeconds = _timerOffSeconds;
                }

                if (!enabled)
                {
                    Thread.Sleep(250);
                    continue;
                }

                SetSprinkler(true);
                if (!SleepWhileTimerEnabled(onSeconds * 1000))
                {
                    continue;
                }

                SetSprinkler(false);
                SleepWhileTimerEnabled(offSeconds * 1000);
            }
        }

        private static int ClampTimerSeconds(int seconds)
        {
            if (seconds < MinTimerSeconds)
            {
                return MinTimerSeconds;
            }

            if (seconds > MaxTimerSeconds)
            {
                return MaxTimerSeconds;
            }

            return seconds;
        }

        private static bool SleepWhileTimerEnabled(int milliseconds)
        {
            int remaining = milliseconds;
            while (remaining > 0)
            {
                int step = remaining > 250 ? 250 : remaining;
                Thread.Sleep(step);
                remaining -= step;

                lock (StateLock)
                {
                    if (!_timerModeEnabled)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Home Assistant

        private static string SanitizeObjectId(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_')
                {
                    sb.Append(c);
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    sb.Append((char)(c + 32));
                }
                else if (c == ' ' || c == '-')
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }

        private static void InitializeHomeAssistantComponent()
        {
            DeviceConfig defaults = new DeviceConfig();

            string deviceName = string.IsNullOrEmpty(_config.HomeAssistantDeviceName)
                ? defaults.HomeAssistantDeviceName
                : _config.HomeAssistantDeviceName;

            string deviceId = SanitizeObjectId(_config.HomeAssistantDeviceId);
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = SanitizeObjectId(defaults.HomeAssistantDeviceId);
            }

            var device = new HomeAssistantDeviceInfo(
                deviceId,
                deviceName,
                "ESP32 Sprinkler",
                "nanoFramework");

            string mqttClientIdPrefix = deviceId + "-";

            _homeAssistant = new HomeAssistantClient(
                deviceName,
                device,
                _config.MqttBroker,
                _config.MqttPort,
                mqttClientIdPrefix,
                null,
                null,
                OnHomeAssistantMessageReceived,
                OnHomeAssistantConnectionClosed);

            // Add entities - topics auto-generated from entity names
            _sprinklerSwitch = _homeAssistant.AddSwitch(deviceId + "_main", "Sprinkler");
            _timerSwitch = _homeAssistant.AddSwitch(deviceId + "_timer", "Timer");
            _timerOnNumber = _homeAssistant.AddNumber(deviceId + "_timer_on", "Timer ON Seconds", min: "1", max: "3600", step: "1", unitOfMeasurement: "s");
            _timerOffNumber = _homeAssistant.AddNumber(deviceId + "_timer_off", "Timer OFF Seconds", min: "1", max: "3600", step: "1", unitOfMeasurement: "s");
            _relayPinNumber = _homeAssistant.AddNumber(deviceId + "_relay_pin", "Relay GPIO Pin", min: "0", max: "33", step: "1");
            _relayActiveHighSwitch = _homeAssistant.AddSwitch(deviceId + "_relay_active_high", "Relay Active High");
            _bootMarkerSensor = _homeAssistant.AddDiagnosticStringSensor(deviceId + "_boot_marker", "Last Boot Marker", GetBootMarkerState());

            // Wire business logic to entity state changes
            _sprinklerSwitch.OnStateChange += (sender, oldState, newState) =>
            {
                string normalized = newState.ToUpper();
                if (normalized == "ON")
                {
                    SetTimerMode(false);
                    SetSprinkler(true);
                }
                else if (normalized == "OFF")
                {
                    SetTimerMode(false);
                    SetSprinkler(false);
                }
            };

            _timerSwitch.OnStateChange += (sender, oldState, newState) =>
            {
                string normalized = newState.ToUpper();
                if (normalized == "ON" || normalized == "START")
                {
                    SetTimerMode(true);
                }
                else if (normalized == "OFF" || normalized == "STOP")
                {
                    SetTimerMode(false);
                }
            };

            _timerOnNumber.OnStateChange += (sender, oldState, newState) =>
            {
                int onSeconds;
                if (int.TryParse(newState, out onSeconds) && onSeconds > 0)
                {
                    SetTimerOnSeconds(onSeconds);
                }
            };

            _timerOffNumber.OnStateChange += (sender, oldState, newState) =>
            {
                int offSeconds;
                if (int.TryParse(newState, out offSeconds) && offSeconds > 0)
                {
                    SetTimerOffSeconds(offSeconds);
                }
            };

            _relayPinNumber.OnStateChange += (sender, oldState, newState) =>
            {
                int relayPin;
                if (int.TryParse(newState, out relayPin))
                {
                    SetRelayPin(relayPin);
                }
            };

            _relayActiveHighSwitch.OnStateChange += (sender, oldState, newState) =>
            {
                string normalized = newState.ToUpper();
                if (normalized == "ON")
                {
                    SetRelayActiveHigh(true);
                }
                else if (normalized == "OFF")
                {
                    SetRelayActiveHigh(false);
                }
            };
        }

        private static void OnHomeAssistantConnectionClosed(object sender, EventArgs e)
        {
            Debug.WriteLine("MQTT connection closed event received.");
            RequestMqttReconnect("connection closed event");
        }

        private static void OnHomeAssistantMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // Used to detect Home Assistant restart and re-publish discovery/state.
            string topic = e.Topic;

            try
            {
                if (topic == HomeAssistantTopics.StatusTopic)
                {
                    string payload = Encoding.UTF8.GetString(e.Message, 0, e.Message.Length).Trim();

                    if (_homeAssistant.IsHomeAssistantOnlineEvent(topic, payload))
                    {
                        Debug.WriteLine("HA came online, re-publishing discovery and state.");
                        _homeAssistant.PublishOnline();
                        _homeAssistant.PublishDiscovery();
                        PublishAllState();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MQTT message processing error: " + ex.Message);
                RequestMqttReconnect("message handler exception");
            }
        }

        private static void PublishAllState()
        {
            if (!_homeAssistant.IsConnected || _sprinklerSwitch == null)
            {
                return;
            }

            bool sprinklerOn;
            bool timerOn;
            int onSeconds;
            int offSeconds;
            int relayPin;
            bool relayActiveHigh;

            lock (StateLock)
            {
                sprinklerOn = _sprinklerOn;
                timerOn = _timerModeEnabled;
                onSeconds = _timerOnSeconds;
                offSeconds = _timerOffSeconds;
                relayPin = _relayPin;
                relayActiveHigh = _relayActiveHigh;
            }

            // Publish local device state without re-triggering command handlers.
            _sprinklerSwitch.PublishState(sprinklerOn ? "ON" : "OFF");
            _timerSwitch.PublishState(timerOn ? "ON" : "OFF");
            _timerOnNumber.PublishState(onSeconds.ToString());
            _timerOffNumber.PublishState(offSeconds.ToString());
            _relayPinNumber.PublishState(relayPin.ToString());
            if (_relayActiveHighSwitch != null)
            {
                _relayActiveHighSwitch.PublishState(relayActiveHigh ? "ON" : "OFF");
            }
            if (_bootMarkerSensor != null)
            {
                _bootMarkerSensor.PublishState(GetBootMarkerState());
            }
        }

        #endregion

        #region Connection Event Handlers

        private static bool IsStationConnected()
        {
            return Wireless80211.IsConnected();
        }

        private static void RequestMqttReconnect(string reason)
        {
            _mqttReconnectRequested = true;
            Debug.WriteLine("MQTT reconnect requested: " + reason);
        }

        private static bool TryConnectMqtt()
        {
            if (!Wireless80211.IsEnabled())
            {
                Debug.WriteLine("Skipping MQTT connect: Wi-Fi configuration is incomplete.");
                return false;
            }

            if (!IsStationConnected())
            {
                Debug.WriteLine("Skipping MQTT connect: station is not connected.");
                return false;
            }

            try
            {
                // Connect to MQTT broker with LWT (Last Will Testament) for availability topic.
                bool connected = _homeAssistant.Connect(_homeAssistant.AvailabilityTopic, "offline");

                if (connected)
                {
                    _mqttReconnectRequested = false;
                    PublishAllState();
                    Debug.WriteLine("MQTT connected.");
                }

                return connected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MQTT connect failed: " + ex.Message);
                return false;
            }
        }

        private static void MqttMaintenanceLoop()
        {
            while (true)
            {
                try
                {
                    if (ShouldRecoverConnectivity())
                    {
                        if (RecoverConnectivity())
                        {
                            _consecutiveRecoveryFailures = 0;
                        }
                        else
                        {
                            RegisterRecoveryFailure("Unable to recover connectivity.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    RegisterRecoveryFailure("MQTT maintenance error: " + ex.Message);
                }

                Thread.Sleep(HealthCheckIntervalMs);
            }
        }

        private static bool ShouldRecoverConnectivity()
        {
            if (!Wireless80211.IsEnabled())
            {
                return true;
            }

            if (_mqttReconnectRequested)
            {
                return true;
            }

            if (!IsStationConnected())
            {
                return true;
            }

            if (!_homeAssistant.IsConnected)
            {
                return true;
            }

            return false;
        }

        private static bool RecoverConnectivity()
        {
            lock (ConnectivityLock)
            {
                if (!IsStationConnected())
                {
                    Debug.WriteLine("STA disconnected. Trying reconnect...");
                    if (!Wireless80211.Reconnect(30_000))
                    {
                        Debug.WriteLine("Wi-Fi reconnect failed.");
                        return false;
                    }
                }

                for (int attempt = 1; attempt <= MaxMqttReconnectAttempts; attempt++)
                {
                    if (TryConnectMqtt())
                    {
                        return true;
                    }

                    Debug.WriteLine("MQTT reconnect attempt " + attempt + "/" + MaxMqttReconnectAttempts + " failed.");
                    Thread.Sleep(MqttReconnectDelayMs);
                }

                // Keep running and retry on next maintenance cycle.
                _mqttReconnectRequested = true;
                Debug.WriteLine("MQTT still unreachable. Will retry in maintenance loop.");
                return false;
            }
        }

        #endregion

        #region Boot Marker

        private static void WriteBootMarker(string marker)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(marker);
                using (FileStream stream = new FileStream(BootMarkerPath, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }

                // Make sure there is time to have it written to flash before deep sleep.
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to write boot marker: " + ex.Message);
            }
        }

        private static void WriteRebootMarker(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                reason = "unknown";
            }

            WriteBootMarker("REBOOT|" + reason + "|" + DateTime.UtcNow.Ticks);
        }

        private static string ReadAndClearBootMarker()
        {
            try
            {
                if (!File.Exists(BootMarkerPath))
                {
                    return string.Empty;
                }

                using (FileStream stream = new FileStream(BootMarkerPath, FileMode.Open, FileAccess.Read))
                {
                    int length = (int)stream.Length;
                    if (length <= 0)
                    {
                        return string.Empty;
                    }

                    byte[] buffer = new byte[length];
                    int offset = 0;
                    while (offset < length)
                    {
                        int read = stream.Read(buffer, offset, length - offset);
                        if (read <= 0)
                        {
                            break;
                        }

                        offset += read;
                    }

                    string marker = Encoding.UTF8.GetString(buffer, 0, offset);
                    if (!string.IsNullOrEmpty(marker))
                    {
                        Debug.WriteLine("Boot marker detected: " + marker);
                    }

                    return marker;
                }
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                try
                {
                    // Try to create the files again to clear the boot marker, but ignore any errors.
                    using (FileStream stream = new FileStream(BootMarkerPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.Flush();
                    }
                }
                catch
                {
                }
            }
        }

        private static string GetBootMarkerState()
        {
            if (string.IsNullOrEmpty(_bootMarker))
            {
                return "none";
            }

            return _bootMarker;
        }

        private static void RegisterRecoveryFailure(string message)
        {
            _consecutiveRecoveryFailures++;
            Debug.WriteLine(message + " Consecutive failures: " + _consecutiveRecoveryFailures);

            if (_consecutiveRecoveryFailures >= MaxConsecutiveRecoveryFailures)
            {
                RebootByTimedDeepSleep("Too many connectivity failures.");
            }
        }

        private static void RebootByTimedDeepSleep(string reason)
        {
            try
            {
                Debug.WriteLine("Recovery reboot requested: " + reason);
                WriteRebootMarker(reason);
                if (_homeAssistant != null)
                {
                    _homeAssistant.Disconnect();
                }

                Thread.Sleep(1000);
                Sleep.EnableWakeupByTimer(TimeSpan.FromSeconds(RecoveryDeepSleepSeconds));
                Sleep.StartDeepSleep();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Deep sleep reboot failed, forcing hard reboot: " + ex.Message);
                WriteRebootMarker("Deep sleep failed: " + ex.Message);
            }

            // In case deep sleep fails, keep running in foreground idle loop to avoid exiting the app.
            RunForegroundIdleLoop();
        }

        #endregion
    }
}
