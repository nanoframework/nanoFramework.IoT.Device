// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common;
using UnitsNet;
using nanoFramework.TestFramework;


namespace Iot.Device.Common.Tests
{
    [TestClass]
    public class WeatherTests
    {
        [TestMethod]
        public void HeatIndexIsCalculatedCorrectlyTest()
        {
            HeatIndexIsCalculatedCorrectly(35, 30, 70);
            HeatIndexIsCalculatedCorrectly(20, 20, 60);
            HeatIndexIsCalculatedCorrectly(26, 25, 80);
            HeatIndexIsCalculatedCorrectly(38, 32.22, 60);
            HeatIndexIsCalculatedCorrectly(32, 29.44, 60);
            HeatIndexIsCalculatedCorrectly(28, 29.5, 12);
        }

        //[Theory]
        //[InlineData(35, 30, 70)]
        //[InlineData(20, 20, 60)]
        //[InlineData(26, 25, 80)]
        //[InlineData(38, 32.22, 60)]
        //[InlineData(32, 29.44, 60)]
        //[InlineData(28, 29.5, 12)]
        public void HeatIndexIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            Temperature heatIndex = WeatherHelper.CalculateHeatIndex(Temperature.FromDegreesCelsius(celsius), RelativeHumidity.FromPercent(relativeHumidity));
            Assert.AreEqual(expected, Math.Round(heatIndex.DegreesCelsius));
        }

        [TestMethod]
        public void SaturatedVaporPressureOverWaterTest()
        {
            SaturatedVaporPressureOverWater(4245.20, 30);
            SaturatedVaporPressureOverWater(3168.74, 25);
            SaturatedVaporPressureOverWater(2644.42, 22);
            SaturatedVaporPressureOverWater(1705.32, 15);
            SaturatedVaporPressureOverWater(611.213, 0);
        }

        //[Theory]
        //// Test values from https://de.wikibooks.org/wiki/Tabellensammlung_Chemie/_Stoffdaten_Wasser
        //[InlineData(4245.20, 30)]
        //[InlineData(3168.74, 25)]
        //[InlineData(2644.42, 22)]
        //[InlineData(1705.32, 15)]
        //[InlineData(611.213, 0)]
        public void SaturatedVaporPressureOverWater(double expected, double celsius)
        {
            Pressure saturatedVaporPressure = WeatherHelper.CalculateSaturatedVaporPressureOverWater(Temperature.FromDegreesCelsius(celsius));
            Assert.AreEqual((int)(expected * 10), (int)(saturatedVaporPressure.Pascals * 10));
        }

        [TestMethod]
        public void SaturatedVaporPressureOverIceTest()
        {
            SaturatedVaporPressureOverIce(611.1, 0);
            SaturatedVaporPressureOverIce(259.6, -10);
            SaturatedVaporPressureOverIce(103.06, -20);
            SaturatedVaporPressureOverIce(22.273, -35);
        }

        //[Theory]
        //// Test values from https://de.wikibooks.org/wiki/Tabellensammlung_Chemie/_Stoffdaten_Wasser
        //[InlineData(611.1, 0)]
        //[InlineData(259.6, -10)]
        //[InlineData(103.06, -20)]
        //[InlineData(22.273, -35)]
        public void SaturatedVaporPressureOverIce(double expected, double celsius)
        {
            Pressure saturatedVaporPressure = WeatherHelper.CalculateSaturatedVaporPressureOverIce(Temperature.FromDegreesCelsius(celsius));
            Assert.AreEqual((int)(expected * 10), (int)(saturatedVaporPressure.Pascals * 10));
        }

        [TestMethod]
        public void ActualVaporPressureIsCalculatedCorrectlyTest()
        {
            ActualVaporPressureIsCalculatedCorrectly(1061, 30, 25);
            ActualVaporPressureIsCalculatedCorrectly(1616, 25, 51);
            ActualVaporPressureIsCalculatedCorrectly(1904, 22, 72);
        }

        //[Theory]
        //[InlineData(1061, 30, 25)]
        //[InlineData(1616, 25, 51)]
        //[InlineData(1904, 22, 72)]
        public void ActualVaporPressureIsCalculatedCorrectly(double expected, double celsius, double relativeHumidity)
        {
            Pressure actualVaporPressure = WeatherHelper.CalculateActualVaporPressure(Temperature.FromDegreesCelsius(celsius), RelativeHumidity.FromPercent(relativeHumidity));
            Assert.AreEqual(expected, Math.Round(actualVaporPressure.Pascals));
        }

        [TestMethod]
        public void DewPointIsCalculatedCorrectlyTest()
        {
            DewPointIsCalculatedCorrectly(77.71, 100, 50);
            DewPointIsCalculatedCorrectly(45.79, 80, 30);
            DewPointIsCalculatedCorrectly(27.68, 60, 29);
        }

        //[Theory]
        //// Compare with https://en.wikipedia.org/wiki/Dew_point#/media/File:Dewpoint-RH.svg
        //[InlineData(77.71, 100, 50)]
        //[InlineData(45.79, 80, 30)]
        //[InlineData(27.68, 60, 29)]
        public void DewPointIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        {
            Temperature dewPoint = WeatherHelper.CalculateDewPoint(Temperature.FromDegreesFahrenheit(fahrenheit), RelativeHumidity.FromPercent(relativeHumidity));
            Assert.AreEqual(Math.Round(expected * 100), Math.Round(dewPoint.DegreesFahrenheit * 100));
        }

        // RTODO: uncomment once the Density UnitsNet will be there
        //[TestMethod]
        //public void AbsoluteHumidityIsCalculatedCorrectlyTest()
        //{
        //    AbsoluteHumidityIsCalculatedCorrectly(23, 100, 50); 
        //    AbsoluteHumidityIsCalculatedCorrectly(15, 80, 59);
        //    AbsoluteHumidityIsCalculatedCorrectly(5, 40, 75);
        //}

        ////[Theory]
        ////[InlineData(23, 100, 50)]
        ////[InlineData(15, 80, 59)]
        ////[InlineData(5, 40, 75)]
        //public void AbsoluteHumidityIsCalculatedCorrectly(double expected, double fahrenheit, double relativeHumidity)
        //{
        //    Density absoluteHumidity = WeatherHelper.CalculateAbsoluteHumidity(Temperature.FromDegreesFahrenheit(fahrenheit), RelativeHumidity.FromPercent(relativeHumidity));
        //    Assert.AreEqual(expected, absoluteHumidity.GramsPerCubicMeter, 0);
        //}

        [TestMethod]
        public void AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTempTest()
        {
            AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTemp(1011.22, 900);
            AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTemp(111.18, 1000);
            AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTemp(547.1, 950);
        }

        //[Theory]
        //[InlineData(1011.22, 900)]
        //[InlineData(111.18, 1000)]
        //[InlineData(547.1, 950)]
        public void AltitudeIsCalculatedCorrectlyAtMslpAndDefaultTemp(double expected, double hpa)
        {
            Length altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascals(hpa));
            Assert.AreEqual(expected * 100, Math.Round(altitude.Meters * 100));
        }

        [TestMethod]
        public void AltitudeIsCalculatedCorrectlyAtDefaultTempTest()
        {
            AltitudeIsCalculatedCorrectlyAtDefaultTemp(1011.22, 900, 1013.25);
            AltitudeIsCalculatedCorrectlyAtDefaultTemp(111.18, 1000, 1013.25);
            AltitudeIsCalculatedCorrectlyAtDefaultTemp(547.1, 950, 1013.25);
        }

        //[Theory]
        //[InlineData(1011.22, 900, 1013.25)]
        //[InlineData(111.18, 1000, 1013.25)]
        //[InlineData(547.1, 950, 1013.25)]
        public void AltitudeIsCalculatedCorrectlyAtDefaultTemp(double expected, double hpa, double seaLevelHpa)
        {
            Length altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascals(hpa), Pressure.FromHectopascals(seaLevelHpa));
            Assert.AreEqual(expected * 100, Math.Round(altitude.Meters * 100));
        }

        [TestMethod]
        public void AltitudeIsCalculatedCorrectlyTest()
        {
            AltitudeIsCalculatedCorrectly(1011.22, 900, 1013.25, 15);
            AltitudeIsCalculatedCorrectly(111.18, 1000, 1013.25, 15);
            AltitudeIsCalculatedCorrectly(547.1, 950, 1013.25, 15);
        }

        //[Theory]
        //[InlineData(1011.22, 900, 1013.25, 15)]
        //[InlineData(111.18, 1000, 1013.25, 15)]
        //[InlineData(547.1, 950, 1013.25, 15)]
        public void AltitudeIsCalculatedCorrectly(double expected, double hpa, double seaLevelHpa, double celsius)
        {
            Length altitude = WeatherHelper.CalculateAltitude(Pressure.FromHectopascals(hpa), Pressure.FromHectopascals(seaLevelHpa), Temperature.FromDegreesCelsius(celsius));
            Assert.AreEqual(expected * 100, Math.Round(altitude.Meters * 100));
        }

        [TestMethod]
        public void SeaLevelPressureIsCalculatedCorrectly()
        {
            SeaLevelPressureIsCalculatedCorrectly(1013.2, 900, 1010.83, 15);
            SeaLevelPressureIsCalculatedCorrectly(1013.25, 1000, 111.14, 15);
            SeaLevelPressureIsCalculatedCorrectly(1013.23, 950, 546.89, 15);
        }

        //[Theory]
        //[InlineData(1013.2, 900, 1010.83, 15)]
        //[InlineData(1013.25, 1000, 111.14, 15)]
        //[InlineData(1013.23, 950, 546.89, 15)]
        public void SeaLevelPressureIsCalculatedCorrectly(double expected, double pressure, double altitude, double celsius)
        {
            Pressure seaLevelPressure = WeatherHelper.CalculateSeaLevelPressure(Pressure.FromHectopascals(pressure), Length.FromMeters(altitude), Temperature.FromDegreesCelsius(celsius));
            Assert.AreEqual(expected * 100, Math.Round(seaLevelPressure.Hectopascals * 100));
        }

        [TestMethod]
        public void PressureIsCalculatedCorrectlyTest()
        {
            PressureIsCalculatedCorrectly(900.04, 1013.25, 1010.83, 15);
            PressureIsCalculatedCorrectly(1000, 1013.25, 111.14, 15);
            PressureIsCalculatedCorrectly(950.02, 1013.25, 546.89, 15);
        }

        //[Theory]
        //[InlineData(900.04, 1013.25, 1010.83, 15)]
        //[InlineData(1000, 1013.25, 111.14, 15)]
        //[InlineData(950.02, 1013.25, 546.89, 15)]
        public void PressureIsCalculatedCorrectly(double expected, double seaLevelPressure, double altitude, double celsius)
        {
            Pressure pressure = WeatherHelper.CalculatePressure(Pressure.FromHectopascals(seaLevelPressure), Length.FromMeters(altitude), Temperature.FromDegreesCelsius(celsius));
            Assert.AreEqual(expected * 100, Math.Round(pressure.Hectopascals * 100));
        }

        [TestMethod]
        public void TemperatureIsCalculatedCorrectlyTest()
        {
            TemperatureIsCalculatedCorrectly(15, 900, 1013.25, 1010.83);
            TemperatureIsCalculatedCorrectly(15, 1000, 1013.25, 111.14);
            TemperatureIsCalculatedCorrectly(15, 950, 1013.25, 546.89);
        }

        //[Theory]
        //[InlineData(15, 900, 1013.25, 1010.83)]
        //[InlineData(15, 1000, 1013.25, 111.14)]
        //[InlineData(15, 950, 1013.25, 546.89)]
        public void TemperatureIsCalculatedCorrectly(double expected, double pressure, double seaLevelPressure, double altitude)
        {
            Temperature temperature = WeatherHelper.CalculateTemperature(Pressure.FromHectopascals(pressure), Pressure.FromHectopascals(seaLevelPressure), Length.FromMeters(altitude));
            Assert.AreEqual(expected, Math.Round(temperature.DegreesCelsius));
        }

        [TestMethod]
        public void CalculateBarometricPressureTest()
        {
            CalculateBarometricPressure(948.17, 24.0, 650, 1020.739);
            CalculateBarometricPressure(948.17, 9.0, 650, 1025.12);
            CalculateBarometricPressure(999, 10, 0, 999);
            CalculateBarometricPressure(1020, 15, -200, 996.17);
            CalculateBarometricPressure(950, 15, 546.89, 1012.9);
        }

        //[Theory]
        //// This is quite close to what my GPS says
        //[InlineData(948.17, 24.0, 650, 1020.739)]
        //// Should give a similar result, but uses the low temperature vapor pressure formula
        //[InlineData(948.17, 9.0, 650, 1025.12)]
        //// When no height difference is given, the input should equal the output
        //[InlineData(999, 10, 0, 999)]
        //// When the altitude is negative, the result is less than the input
        //[InlineData(1020, 15, -200, 996.17)]
        //// To compare with the above formulas
        //[InlineData(950, 15, 546.89, 1012.9)] // result is changed to 1012.9 from 1013.23
        public void CalculateBarometricPressure(double measuredValue, double temperature, double altitude,
            double expected)
        {
            Pressure result = WeatherHelper.CalculateBarometricPressure(Pressure.FromHectopascals(measuredValue),
                Temperature.FromDegreesCelsius(temperature), Length.FromMeters(altitude));
            Assert.AreEqual((long)(Math.Round(expected * 100)), (long)(Math.Round(result.Hectopascals * 100)));
        }

        [TestMethod]
        public void CalculateBarometricPressureWithHumidityTest()
        {
            CalculateBarometricPressureWithHumidity(950, 15, 546.89, 10, 1013.19);
            CalculateBarometricPressureWithHumidity(950, 15, 546.89, 50, 1013.01);
            CalculateBarometricPressureWithHumidity(950, 15, 546.89, 100, 1012.78);
            CalculateBarometricPressureWithHumidity(950, -5, 546.89, 100, 1017.95);
            CalculateBarometricPressureWithHumidity(950, -35, 546.89, 100, 1026.92);
        }

        //[Theory]
        //[InlineData(950, 15, 546.89, 10, 1013.19)]
        //[InlineData(950, 15, 546.89, 50, 1013.01)]
        //[InlineData(950, 15, 546.89, 100, 1012.78)]
        //[InlineData(950, -5, 546.89, 100, 1017.95)]
        //[InlineData(950, -35, 546.89, 100, 1026.92)]
        public void CalculateBarometricPressureWithHumidity(double measuredValue, double temperature, double altitude, double relativeHumidity,
            double expected)
        {
            Pressure result = WeatherHelper.CalculateBarometricPressure(Pressure.FromHectopascals(measuredValue),
                Temperature.FromDegreesCelsius(temperature), Length.FromMeters(altitude), RelativeHumidity.FromPercent(relativeHumidity));
            Assert.AreEqual((long)Math.Round(expected * 100), (long)Math.Round(result.Hectopascals * 100));
        }

        // TODO: uncomment once Dentisity will be added to UnitsNet
        //[TestMethod]
        //public void GetRelativeHumidityFromActualAirTemperatureTest()
        //{
        //    GetRelativeHumidityFromActualAirTemperature(20, 40.2, 20, 40.2);
        //    GetRelativeHumidityFromActualAirTemperature(30, 40.2, 20, 70.569);
        //    GetRelativeHumidityFromActualAirTemperature(27.8, 38.1, 20.0, 59.317);
        //}

        ////[Theory]
        ////[InlineData(20, 40.2, 20, 40.2)]
        ////[InlineData(30, 40.2, 20, 70.569)]
        ////[InlineData(27.8, 38.1, 20.0, 59.317)] // in data from BMP280 (in case), thermometer 1 meter away shows 20.0�, 57%
        //public void GetRelativeHumidityFromActualAirTemperature(double inTemp, double inHumidity, double outTemp, double outHumidityExpected)
        //{
        //    RelativeHumidity result = WeatherHelper.GetRelativeHumidityFromActualAirTemperature(
        //        Temperature.FromDegreesCelsius(inTemp),
        //        RelativeHumidity.FromPercent(inHumidity), Temperature.FromDegreesCelsius(outTemp));

        //    Assert.AreEqual((long)(outHumidityExpected * 1000), (long)(result.Percent * 1000));
        //}
    }
}
