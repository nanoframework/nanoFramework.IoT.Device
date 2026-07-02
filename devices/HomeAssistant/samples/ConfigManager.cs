// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using nanoFramework.Json;

namespace nanoSprinkler
{
    /// <summary>
    /// Loads and saves <see cref="DeviceConfig"/> from/to the internal storage file I:\config.json.
    /// Survives deep sleep and hard reboots because the file is on the ESP32 internal flash.
    /// </summary>
    public static class ConfigManager
    {
        private const string ConfigPath = "I:\\config.json";
        private const string ConfigBackupPath = "I:\\config.json.bak";
        private const string ConfigTempPath = "I:\\config.json.tmp";

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
                if (string.IsNullOrEmpty(json))
                {
                    // Recover from interrupted save where backup exists but primary is missing/corrupt.
                    json = ReadAllText(ConfigBackupPath);
                }

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
                if (config == null)
                {
                    config = new DeviceConfig();
                }

                string json = JsonConvert.SerializeObject(config);
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

            // Two-phase write: persist temp first, then rotate primary to backup, then promote temp.
            using (FileStream stream = new FileStream(ConfigTempPath, FileMode.Create, FileAccess.Write))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }

            try
            {
                if (File.Exists(ConfigBackupPath))
                {
                    File.Delete(ConfigBackupPath);
                }

                if (File.Exists(path))
                {
                    File.Move(path, ConfigBackupPath);
                }

                File.Move(ConfigTempPath, path);

                if (File.Exists(ConfigBackupPath))
                {
                    File.Delete(ConfigBackupPath);
                }
            }
            catch
            {
                // Best effort rollback: if promotion failed and only backup exists, restore primary.
                try
                {
                    if (File.Exists(ConfigTempPath))
                    {
                        File.Delete(ConfigTempPath);
                    }
                }
                catch
                {
                }

                try
                {
                    if (!File.Exists(path) && File.Exists(ConfigBackupPath))
                    {
                        File.Move(ConfigBackupPath, path);
                    }
                }
                catch
                {
                }

                throw;
            }
        }

        private static DeviceConfig ParseJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            DeviceConfig cfg = (DeviceConfig)JsonConvert.DeserializeObject(json, typeof(DeviceConfig));
            if (cfg == null)
            {
                return null;
            }

            // Keep string members safe for consumers even if JSON contained null values.
            cfg.WifiSsid = cfg.WifiSsid ?? string.Empty;
            cfg.WifiPassword = cfg.WifiPassword ?? string.Empty;
            cfg.MqttBroker = cfg.MqttBroker ?? string.Empty;
            cfg.HomeAssistantDeviceName = cfg.HomeAssistantDeviceName ?? string.Empty;
            cfg.HomeAssistantDeviceId = cfg.HomeAssistantDeviceId ?? string.Empty;

            return cfg;
        }
    }
}
