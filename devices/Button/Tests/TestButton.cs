// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.Button;

namespace Iot.Device.Tests
{
    public class TestButton : ButtonBase
    {
        public TestButton() : base()
        {
        }

        public TestButton(TimeSpan doublePress, TimeSpan holding, TimeSpan debounceTime)
            : base(doublePress, holding, debounceTime)
        {
        }

        public void PressButton()
        {
            HandleButtonPressed();
        }

        public void ReleaseButton()
        {
            HandleButtonReleased();
        }

        public void PressThenReleaseButton()
        {
            // Wait a little bit to mimic actual user behavior
            TimeSpan holding = TimeSpan.FromMilliseconds(100);

            PressThenReleaseButton(holding);
        }

        public void PressThenReleaseButton(TimeSpan holding)
        {
            HandleButtonPressed();
            Thread.Sleep(holding);
            HandleButtonReleased();
        }
    }
}
