﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Button;

namespace Iot.Device.Tests
{
    public class TestButton : ButtonBase
    {
        public TestButton() : base()
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
    }
}
