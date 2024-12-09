// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;

namespace Iot.Device.MulticastDNS.Package
{
    internal static class ArrayListUtility
    {
        public static void AddRange(this ArrayList arrayList, Array bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                arrayList.Add(bytes.GetValue(i));
            }
        }

        public static void AddRange(this ArrayList target, ArrayList source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                target.Add(source[i]);
            }
        }
    }
}
