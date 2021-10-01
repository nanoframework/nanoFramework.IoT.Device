using Iot.Device.Button;
using Iot.Device.Tests;
using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
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
            Thread.Sleep(1100);
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
            Thread.Sleep(200);
            button.ReleaseButton();

            Thread.Sleep(100);

            button.PressButton();
            Thread.Sleep(200);
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
            Thread.Sleep(200);
            button.ReleaseButton();

            Thread.Sleep(3000);

            button.PressButton();
            Thread.Sleep(200);
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
            Thread.Sleep(200);
            button.ReleaseButton();

            Thread.Sleep(100);

            button.PressButton();
            Thread.Sleep(200);
            button.ReleaseButton();

            Assert.Equal(Pressed, false);
        }
    }
}
