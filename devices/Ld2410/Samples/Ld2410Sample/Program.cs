using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

using Iot.Device.Ld2410;
using Iot.Device.Ld2410.Reporting;

#if ESP32
using nanoFramework.Hardware.Esp32;
#endif

namespace Ld2410Sample
{
    public class Program
    {
        public static void Main()
        {
#if DEBUG
            // get available ports
            foreach (string port in SerialPort.GetPortNames())
            {
                Debug.WriteLine($" {port}");
            }
#endif

#if ESP32
			Configuration.SetPinFunction(32, DeviceFunction.COM2_RX);
			Configuration.SetPinFunction(33, DeviceFunction.COM2_TX);
#endif


            using var radar = new Radar("COM2");
            radar.OnMeasurementReceived += OnMeasurement;
            radar.Connect();

            Debug.WriteLine($"Radar Firmware Version: {radar.FirmwareVersion}");

            radar.EnterConfigurationMode();
            radar.SetEngineeringMode(true);
            radar.ExitConfigurationMode();

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        private static void OnMeasurement(object sender, ReportFrame report)
        {
            if (report is EngineeringModeReportFrame engineeringModeReportFrame)
            {
                Debug.WriteLine($"Target State (Engineering Mode): {engineeringModeReportFrame.TargetState} | Movement Target Distance (CM): {engineeringModeReportFrame.MovementTargetDistance.Centimeters} | " +
                    $"Movement Target Energy: {engineeringModeReportFrame.MovementTargetEnergy} | Stationary Target Distance (CM): {engineeringModeReportFrame.StationaryTargetDistance.Centimeters} | " +
                    $"Stationary Target Energy: {engineeringModeReportFrame.StationaryTargetEnergy} | Detection Distance (CM): {engineeringModeReportFrame.DetectionDistance.Centimeters}");

                var gateNumber = 0;
                foreach (var gate in engineeringModeReportFrame.GateData)
                {
                    Debug.WriteLine($"Gate: {gateNumber} - Movement Energy: {gate.MovementEnergy} - Static Energy: {gate.StaticEnergy}");
                    gateNumber++;
                }
            }
            else if (report is BasicReportFrame basicReportFrame)
            {
                Debug.WriteLine($"Target State: {basicReportFrame.TargetState} | Movement Target Distance (CM): {basicReportFrame.MovementTargetDistance.Centimeters} | " +
                    $"Movement Target Energy: {basicReportFrame.MovementTargetEnergy} | Stationary Target Distance (CM): {basicReportFrame.StationaryTargetDistance.Centimeters} | " +
                    $"Stationary Target Energy: {basicReportFrame.StationaryTargetEnergy} | Detection Distance (CM): {basicReportFrame.DetectionDistance.Centimeters}");
            }
        }
    }
}
