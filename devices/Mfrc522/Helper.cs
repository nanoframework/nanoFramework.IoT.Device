// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Card.Ultralight;

namespace Iot.Device.Mfrc522
{
    internal static class Helper
    {
        public static bool IsDefined(UltralightCommand ultralight)
        {
            switch (ultralight)
            {
                case UltralightCommand.GetVersion:
                case UltralightCommand.Read16Bytes:
                case UltralightCommand.ReadFast:
                case UltralightCommand.WriteCompatible:
                case UltralightCommand.Write4Bytes:
                case UltralightCommand.ReadCounter:
                case UltralightCommand.IncreaseCounter:
                case UltralightCommand.PasswordAuthentication:
                case UltralightCommand.ThreeDsAuthenticationPart1:
                case UltralightCommand.ThreeDsAuthenticationPart2:
                case UltralightCommand.ReadSignature:
                    return true;
                default:
                    return false;
            }
        }
    }
}
