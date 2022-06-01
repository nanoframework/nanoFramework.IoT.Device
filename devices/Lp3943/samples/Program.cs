// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Threading;
using Iot.Device.Lp3943;

Debug.WriteLine("Hello");

var lp3943 = new Lp3943();

Thread.Sleep(Timeout.Infinite);
