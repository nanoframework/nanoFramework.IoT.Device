// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Rfid;

namespace Iot.Device.Pn5180
{
    internal class SelectedPiccInformation
    {
        public SelectedPiccInformation(Data106kbpsTypeB card, bool lastBlockMark)
        {
            Card = card;
            LastBlockMark = lastBlockMark;
        }

        public Data106kbpsTypeB Card { get; set; }
        public bool LastBlockMark { get; set; }
    }
}
