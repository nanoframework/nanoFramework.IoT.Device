// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing BlueCrashInfoEventArgs.
    /// </summary>
    public class BlueCrashInfoEventArgs : EventArgs
    {
        /// <summary>
        /// Crash type.
        /// </summary>
        public readonly CrashType CrashType;

        /// <summary>
        /// Stack pointer.
        /// </summary>
        public readonly ushort StackPointer;

        /// <summary>
        /// Register R0.
        /// </summary>
        public readonly ushort Register0;

        /// <summary>
        /// Register R1.
        /// </summary>
        public readonly ushort Register1;

        /// <summary>
        /// Register R2.
        /// </summary>
        public readonly ushort Register2;

        /// <summary>
        /// Register R3.
        /// </summary>
        public readonly ushort Register3;

        /// <summary>
        /// Register R12.
        /// </summary>
        public readonly ushort Register12;

        /// <summary>
        /// Link register.
        /// </summary>
        public readonly ushort LinkRegister;

        /// <summary>
        /// Program counter where crash occured.
        /// </summary>
        public readonly ushort ProgramCounter;

        /// <summary>
        /// xPSR register.
        /// </summary>
        public readonly ushort XPsrRegister;

        /// <summary>
        /// Length of <see cref="DebugData"/> field.
        /// </summary>
        public readonly byte DebugDataLength;

        /// <summary>
        /// Debug data.
        /// </summary>
        public readonly byte[] DebugData;

        internal BlueCrashInfoEventArgs(
            CrashType crashType,
            ushort stackPointer,
            ushort register0,
            ushort register1,
            ushort register2,
            ushort register3,
            ushort register12,
            ushort linkRegister,
            ushort programCounter,
            ushort xPsrRegister,
            byte debugDataLength,
            byte[] debugData)
        {
            CrashType = crashType;
            StackPointer = stackPointer;
            Register0 = register0;
            Register1 = register1;
            Register2 = register2;
            Register3 = register3;
            Register12 = register12;
            LinkRegister = linkRegister;
            ProgramCounter = programCounter;
            XPsrRegister = xPsrRegister;
            DebugDataLength = debugDataLength;
            DebugData = debugData;
        }
    }
}
