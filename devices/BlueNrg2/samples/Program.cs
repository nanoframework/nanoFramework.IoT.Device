// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.BlueNrg2.Aci;

namespace Iot.Device.BlueNrg2.Samples
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            Debug.WriteLine("Program Start");

            var chipSelect = Utilities.GetPinNumber('E', 13);

            var blueNrg2 = new BlueNrg2(
                new SpiConnectionSettings(2, chipSelect),
                Utilities.GetPinNumber('B', 11),
                Utilities.GetPinNumber('C', 6)
            );

            blueNrg2.StartBluetoothThread();

            blueNrg2.Gatt.Init();

            var deviceName = "NanoFramework bluetooth test!";
            ushort deviceNameServiceHandle;
            ushort deviceNameCharacteristicHandle;
            ushort appearanceCharacteristicHandle;

            blueNrg2.Gap.Init(
                Role.Central,
                false,
                (byte)deviceName.Length,
                out deviceNameServiceHandle,
                out deviceNameCharacteristicHandle,
                out appearanceCharacteristicHandle
            );

            blueNrg2.Gatt.UpdateCharacteristicValue(
                deviceNameServiceHandle,
                deviceNameCharacteristicHandle,
                0,
                (byte)deviceName.Length,
                Encoding.UTF8.GetBytes(deviceName)
            );

            Guid serviceUuid;
            Guid characteristicUuid;
            Guid.TryParseGuidWithDashes("f162b0d0-1715-11ed-861d-0242ac120002", out serviceUuid);
            Guid.TryParseGuidWithDashes("b9702b10-1717-11ed-861d-0242ac120002", out characteristicUuid);

            ushort serviceHandle;
            ushort characteristicHandle;
            if (blueNrg2.Gatt.AddService(UuidType.Uuid128, serviceUuid.ToByteArray(), ServiceType.Primary, 1, out serviceHandle) != BleStatus.Success)
                return;

            if (blueNrg2.Gatt.AddCharacteristic(
                    serviceHandle,
                    UuidType.Uuid128,
                    characteristicUuid.ToByteArray(),
                    1,
                    CharacteristicProperties.Read | CharacteristicProperties.Write,
                    SecurityPermissions.None,
                    CharacteristicEventMask.NotifyReadRequestAndWaitForApprovalResponse |
                    CharacteristicEventMask.NotifyWriteRequestAndWaitForApprovalResponse,
                    0,
                    true,
                    out characteristicHandle
                ) != BleStatus.Success)
                return;

            blueNrg2.Gatt.UpdateCharacteristicValue(serviceHandle, characteristicHandle, 0, 0, Encoding.UTF8.GetBytes("Hello World from BlueNRG-2"));
            blueNrg2.Hci.LeSetAdvertiseEnable(true);

            while (true)
            {
                Thread.Sleep(100);
            }

            Debug.WriteLine("Program End");
        }
    }
}
