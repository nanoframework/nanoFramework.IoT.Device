// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing HalEndOfRadioActivityEventArgs.
    /// </summary>
    public class HalEndOfRadioActivityEventArgs : EventArgs
    {
        /// <summary>
        /// Completed radio events.
        /// </summary>
        public readonly RadioState LastState;

        /// <summary>
        /// Incoming radio events.
        /// </summary>
        public readonly RadioState NextState;

        /// <summary>
        /// 32bit absolute current time expressed in internal time units.
        /// </summary>
        public readonly uint NextStateSysTime;

        internal HalEndOfRadioActivityEventArgs(RadioState lastState, RadioState nextState, uint nextStateSysTime)
        {
            LastState = lastState;
            NextState = nextState;
            NextStateSysTime = nextStateSysTime;
        }
    }
}
