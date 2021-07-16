// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Iot.Device.SenseHat.Samples
{
    internal class Joystick
    {
        public static void Run()
        {
            using SenseHatJoystick j = new();
            while (true)
            {
                j.Read();

                Console.Clear();
                if (j.HoldingUp)
                {
                    Debug.Write("U");
                }

                if (j.HoldingDown)
                {
                    Debug.Write("D");
                }

                if (j.HoldingLeft)
                {
                    Debug.Write("L");
                }

                if (j.HoldingRight)
                {
                    Debug.Write("R");
                }

                if (j.HoldingButton)
                {
                    Debug.Write("!");
                }
            }
        }
    }
}
