// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Tests;
using nanoFramework.TestFramework;
using System.Threading;

namespace Tests
{
    [TestClass]
    class Buttontests
    {
        [TestMethod]
        public void If_Button_Is_Once_Pressed_Press_Event_Fires()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TestButton button = new TestButton();

            button.Press += (sender, e) =>
            {
                Pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                Holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                DoublePressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_Is_Held_Holding_Event_Fires()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TestButton button = new TestButton();
            button.IsHoldingEnabled = true;

            button.Press += (sender, e) =>
            {
                Pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                Holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                DoublePressed = true;
            };

            button.PressButton();

            // Wait longer than default holding threshold milliseconds, for the click to be recognized as a holding event.
            Thread.Sleep(2100);

            button.ReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, true);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_Is_Held_And_Holding_Is_Disabled_Holding_Event_Does_Not_Fire()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TestButton button = new TestButton();
            button.IsHoldingEnabled = false;

            button.Press += (sender, e) =>
            {
                Pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                Holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                DoublePressed = true;
            };

            button.PressButton();

            // Wait longer than default holding threshold milliseconds, for the press to be recognized as a holding event.
            Thread.Sleep(2100);

            button.ReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_Is_Double_Pressed_DoublePress_Event_Fires()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TestButton button = new TestButton();
            button.IsDoublePressEnabled = true;

            button.Press += (sender, e) =>
            {
                Pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                Holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                DoublePressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(200);

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, true);
        }

        [TestMethod]
        public void If_Button_Is_Pressed_Twice_DoublePress_Event_Does_Not_Fire()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TestButton button = new TestButton();

            button.IsDoublePressEnabled = true;

            button.Press += (sender, e) =>
            {
                Pressed = true;
            };

            button.Holding += (sender, e) =>
            {
                Holding = true;
            };

            button.DoublePress += (sender, e) =>
            {
                DoublePressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            // Wait longer than default double press threshold milliseconds, for the press to be recognized as two separate presses.
            Thread.Sleep(3000);

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_Is_Double_Pressed_And_DoublePress_Is_Disabled_DoublePress_Event_Does_Not_Fire()
        {
            bool Pressed = false;

            TestButton button = new TestButton();
            button.IsDoublePressEnabled = false;

            button.DoublePress += (sender, e) =>
            {
                Pressed = true;
            };

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(200);

            button.PressButton();

            // Wait a little bit to mimic actual user behavior.
            Thread.Sleep(100);

            button.ReleaseButton();

            Assert.Equal(Pressed, false);
        }
    }
}
