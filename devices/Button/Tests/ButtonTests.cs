// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Tests;
using nanoFramework.TestFramework;

namespace Tests
{
    [TestClass]
    class Buttontests
    {
        internal const long DefaultDoublePressMilliseconds = 900;
        internal const long DefaultHoldingMilliseconds = 2000;
        internal const long DefaultDebounceMilliseconds = 300;

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

            button.PressThenReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_With_Debounce_Is_Once_Pressed_Press_Event_Fires()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);

            TestButton button = new(doublePress, holding, debounce);

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

            button.PressThenReleaseButton();

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

            // Wait longer than default holding threshold milliseconds, for the click to be recognized as a holding event.
            button.PressThenReleaseButton(TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds + 100));

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, true);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_With_Debounce_Is_Held_Holding_Event_Fires()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);

            TestButton button = new(doublePress, holding, debounce);
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

            // Wait longer than default holding threshold milliseconds, for the click to be recognized as a holding event.
            button.PressThenReleaseButton(TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds + 100));

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

            // Wait longer than default holding threshold milliseconds, for the press to be recognized as a holding event.
            button.PressThenReleaseButton(TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds + 100));

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_With_Debounce_Is_Held_And_Holding_Is_Disabled_Holding_Event_Does_Not_Fire()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);

            TestButton button = new(doublePress, holding, debounce);
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

            // Wait longer than default holding threshold milliseconds, for the press to be recognized as a holding event.
            button.PressThenReleaseButton(TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds + 100));

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

            button.PressThenReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(200);

            button.PressThenReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, true);
        }

        [TestMethod]
        public void If_Button_With_Debounce_Is_Double_Pressed_DoublePress_Event_Fires()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);
            int pauseAfterFirstPress = (int)DefaultDebounceMilliseconds * 2;

            TestButton button = new(doublePress, holding, debounce);
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

            button.PressThenReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(pauseAfterFirstPress);

            button.PressThenReleaseButton();

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

            button.PressThenReleaseButton();

            // Wait longer than default double press threshold milliseconds, for the press to be recognized as two separate presses.
            Thread.Sleep(3000);

            button.PressThenReleaseButton();

            Assert.Equal(Pressed, true);
            Assert.Equal(Holding, false);
            Assert.Equal(DoublePressed, false);
        }

        [TestMethod]
        public void If_Button_With_Debounce_Is_Pressed_Twice_DoublePress_Event_Does_Not_Fire()
        {
            bool Pressed = false;
            bool Holding = false;
            bool DoublePressed = false;

            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);

            TestButton button = new(doublePress, holding, debounce);

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

            button.PressThenReleaseButton();

            // Wait longer than default double press threshold milliseconds, for the press to be recognized as two separate presses.
            Thread.Sleep(3000);

            button.PressThenReleaseButton();

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

            button.PressThenReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(200);

            button.PressThenReleaseButton();

            Assert.Equal(Pressed, false);
        }

        [TestMethod]
        public void If_Button_With_Debounce_Is_Double_Pressed_And_DoublePress_Is_Disabled_DoublePress_Event_Does_Not_Fire()
        {
            bool Pressed = false;

            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);
            int pauseAfterFirstPress = (int)DefaultDebounceMilliseconds * 2;

            TestButton button = new(doublePress, holding, debounce);
            button.IsDoublePressEnabled = false;

            button.DoublePress += (sender, e) =>
            {
                Pressed = true;
            };

            button.PressThenReleaseButton();

            // Wait shorter than default double press threshold milliseconds, for the press to be recognized as a double press event.
            Thread.Sleep(pauseAfterFirstPress);

            button.PressThenReleaseButton();

            Assert.Equal(Pressed, false);
        }

        [TestMethod]
        public void If_Button_Is_Instantiated_With_Wrong_DoublePress_And_Debounce_Order_Of_Magnitude_Ctor_Throws()
        {
            // Arrange
            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = doublePress;

            Action instantiation = () => new TestButton(doublePress, holding, debounce);

            Type exceptionType = typeof(ArgumentException);

            // Act & Assert
            Assert.Throws(exceptionType, instantiation);
        }

        [TestMethod]
        public void If_Button_Is_Pressed_Twice_Within_Debounce_Time_Second_Press_Is_Discarded()
        {
            // Arrange
            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);
            TimeSpan buttonPressInterval = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds / 2);

            TestButton button = new(doublePress, holding, debounce);

            const int expectedPressCount = 1;
            int actualPressCount = 0;

            button.Press += (sender, e) =>
            {
                actualPressCount++;
            };

            // Act
            button.PressThenReleaseButton();
            Thread.Sleep(buttonPressInterval);
            button.PressThenReleaseButton();

            // Assert
            Assert.Equal(actualPressCount, expectedPressCount);
        }

        [TestMethod]
        public void If_Button_Is_Pressed_Twice_Outside_Debounce_Time_Second_Press_Counts()
        {
            // Arrange
            TimeSpan doublePress = TimeSpan.FromMilliseconds(DefaultDoublePressMilliseconds);
            TimeSpan holding = TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds);
            TimeSpan debounce = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds);
            TimeSpan buttonPressInterval = TimeSpan.FromMilliseconds(DefaultDebounceMilliseconds * 2);

            TestButton button = new(doublePress, holding, debounce);

            const int expectedPressCount = 2;
            int actualPressCount = 0;

            button.Press += (sender, e) =>
            {
                actualPressCount++;
            };

            // Act
            button.PressThenReleaseButton();
            Thread.Sleep(buttonPressInterval);
            button.PressThenReleaseButton();

            // Assert
            Assert.Equal(actualPressCount, expectedPressCount);
        }
    }
}
