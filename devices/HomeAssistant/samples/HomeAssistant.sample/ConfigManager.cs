// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace nanoSprinkler
{
    /// <summary>
    /// Loads and saves <see cref="DeviceConfig"/> from/to the internal storage file I:\config.json.
    /// Survives deep sleep and hard reboots because the file is on the ESP32 internal flash.
    /// </summary>
    public static class ConfigManager
    {
        private const string ConfigPath = "I:\\config.json";

        /// <summary>
        /// Loads the persisted device configuration from flash storage.
        /// Returns defaults when no valid config exists.
        /// </summary>
        /// <returns>The loaded or default <see cref="DeviceConfig"/> instance.</returns>
        public static DeviceConfig Load()
        {
            try
            {
                string json = ReadAllText(ConfigPath);
                if (!string.IsNullOrEmpty(json))
                {
                    DeviceConfig cfg = ParseJson(json);
                    if (cfg != null)
                    {
                        Debug.WriteLine("Config loaded: SSID=" + cfg.WifiSsid
                            + " broker=" + cfg.MqttBroker + ":" + cfg.MqttPort
                            + " haName=" + cfg.HomeAssistantDeviceName
                            + " haId=" + cfg.HomeAssistantDeviceId
                            + " on=" + cfg.TimerOnSeconds + "s off=" + cfg.TimerOffSeconds + "s"
                            + " timerMode=" + (cfg.TimerModeEnabled ? "ON" : "OFF")
                            + " relayPin=" + cfg.RelayPin
                            + " relayActiveHigh=" + (cfg.RelayActiveHigh ? "true" : "false"));
                        return cfg;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Config load error: " + ex.Message);
            }

            Debug.WriteLine("No valid config found – using defaults.");
            return new DeviceConfig();
        }

        /// <summary>
        /// Saves the specified device configuration to flash storage.
        /// </summary>
        /// <param name="config">The configuration to persist.</param>
        public static void Save(DeviceConfig config)
        {
            try
            {
                string json = ToJson(config);
                WriteAllText(ConfigPath, json);
                Debug.WriteLine("Config saved to " + ConfigPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Config save error: " + ex.Message);
            }
        }

        private static string ReadAllText(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
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

                    return Encoding.UTF8.GetString(buffer, 0, offset);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void WriteAllText(string path, string content)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }

        private static string ToJson(DeviceConfig config)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.Append("\"WifiSsid\":\"");
            AppendEscaped(builder, config.WifiSsid);
            builder.Append("\",");
            builder.Append("\"WifiPassword\":\"");
            AppendEscaped(builder, config.WifiPassword);
            builder.Append("\",");
            builder.Append("\"MqttBroker\":\"");
            AppendEscaped(builder, config.MqttBroker);
            builder.Append("\",");
            builder.Append("\"MqttPort\":");
            builder.Append(config.MqttPort);
            builder.Append(",");
            builder.Append("\"HomeAssistantDeviceName\":\"");
            AppendEscaped(builder, config.HomeAssistantDeviceName);
            builder.Append("\",");
            builder.Append("\"HomeAssistantDeviceId\":\"");
            AppendEscaped(builder, config.HomeAssistantDeviceId);
            builder.Append("\",");
            builder.Append("\"TimerOnSeconds\":");
            builder.Append(config.TimerOnSeconds);
            builder.Append(",");
            builder.Append("\"TimerOffSeconds\":");
            builder.Append(config.TimerOffSeconds);
            builder.Append(",");
            builder.Append("\"TimerModeEnabled\":");
            builder.Append(config.TimerModeEnabled ? "true" : "false");
            builder.Append(",");
            builder.Append("\"RelayPin\":");
            builder.Append(config.RelayPin);
            builder.Append(",");
            builder.Append("\"RelayActiveHigh\":");
            builder.Append(config.RelayActiveHigh ? "true" : "false");
            builder.Append("}");
            return builder.ToString();
        }

        private static void AppendEscaped(StringBuilder builder, string value)
        {
            if (value == null)
            {
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '\\' || c == '"')
                {
                    builder.Append('\\');
                }

                builder.Append(c);
            }
        }

        private static DeviceConfig ParseJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            DeviceConfig cfg = new DeviceConfig();
            cfg.WifiSsid = ParseString(json, "WifiSsid", cfg.WifiSsid);
            cfg.WifiPassword = ParseString(json, "WifiPassword", cfg.WifiPassword);
            cfg.MqttBroker = ParseString(json, "MqttBroker", cfg.MqttBroker);
            cfg.MqttPort = ParseInt(json, "MqttPort", cfg.MqttPort);
            cfg.HomeAssistantDeviceName = ParseString(json, "HomeAssistantDeviceName", cfg.HomeAssistantDeviceName);
            cfg.HomeAssistantDeviceId = ParseString(json, "HomeAssistantDeviceId", cfg.HomeAssistantDeviceId);
            cfg.TimerOnSeconds = ParseInt(json, "TimerOnSeconds", cfg.TimerOnSeconds);
            cfg.TimerOffSeconds = ParseInt(json, "TimerOffSeconds", cfg.TimerOffSeconds);
            cfg.TimerModeEnabled = ParseBool(json, "TimerModeEnabled", cfg.TimerModeEnabled);
            cfg.RelayPin = ParseInt(json, "RelayPin", cfg.RelayPin);
            cfg.RelayActiveHigh = ParseBool(json, "RelayActiveHigh", cfg.RelayActiveHigh);
            return cfg;
        }

        private static string ParseString(string json, string key, string defaultValue)
        {
            string token = "\"" + key + "\"";
            int keyPos = json.IndexOf(token);
            if (keyPos < 0)
            {
                return defaultValue;
            }

            int colon = json.IndexOf(':', keyPos + token.Length);
            if (colon < 0)
            {
                return defaultValue;
            }

            int startQuote = json.IndexOf('"', colon + 1);
            if (startQuote < 0)
            {
                return defaultValue;
            }

            int endQuote = startQuote + 1;
            while (endQuote < json.Length)
            {
                if (json[endQuote] == '"' && json[endQuote - 1] != '\\')
                {
                    break;
                }

                endQuote++;
            }

            if (endQuote >= json.Length)
            {
                return defaultValue;
            }

            string value = json.Substring(startQuote + 1, endQuote - startQuote - 1);
            return UnescapeJson(value);
        }

        private static string UnescapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool escaping = false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (escaping)
                {
                    builder.Append(c);
                    escaping = false;
                }
                else if (c == '\\')
                {
                    escaping = true;
                }
                else
                {
                    builder.Append(c);
                }
            }

            if (escaping)
            {
                builder.Append('\\');
            }

            return builder.ToString();
        }

        private static int ParseInt(string json, string key, int defaultValue)
        {
            string token = "\"" + key + "\"";
            int keyPos = json.IndexOf(token);
            if (keyPos < 0)
            {
                return defaultValue;
            }

            int colon = json.IndexOf(':', keyPos + token.Length);
            if (colon < 0)
            {
                return defaultValue;
            }

            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t' || json[start] == '\r' || json[start] == '\n'))
            {
                start++;
            }

            int end = start;
            while (end < json.Length && (json[end] == '-' || (json[end] >= '0' && json[end] <= '9')))
            {
                end++;
            }

            if (end <= start)
            {
                return defaultValue;
            }

            int value;
            if (int.TryParse(json.Substring(start, end - start), out value))
            {
                return value;
            }

            return defaultValue;
        }

        private static bool ParseBool(string json, string key, bool defaultValue)
        {
            string token = "\"" + key + "\"";
            int keyPos = json.IndexOf(token);
            if (keyPos < 0)
            {
                return defaultValue;
            }

            int colon = json.IndexOf(':', keyPos + token.Length);
            if (colon < 0)
            {
                return defaultValue;
            }

            int start = colon + 1;
            while (start < json.Length && (json[start] == ' ' || json[start] == '\t' || json[start] == '\r' || json[start] == '\n'))
            {
                start++;
            }

            if (start >= json.Length)
            {
                return defaultValue;
            }

            string remainder = json.Substring(start).ToLower();
            if (remainder.IndexOf("true") == 0)
            {
                return true;
            }

            if (remainder.IndexOf("false") == 0)
            {
                return false;
            }

            if (remainder.IndexOf("1") == 0)
            {
                return true;
            }

            if (remainder.IndexOf("0") == 0)
            {
                return false;
            }

            return defaultValue;
        }
    }
}
