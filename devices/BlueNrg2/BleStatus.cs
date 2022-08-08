// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    public enum BleStatus : byte
    {
        Success = 0x00,

        UnknownHciCommand = 0x01,
        UnknownConnectionId = 0x02,

        HardwareFailure = 0x03,

        AuthenticationFailure = 0x05,
        KeyMissing = 0x06,
        MemoryCapacityExceeded = 0x07,
        ConnectionTimeout = 0x08,

        CommandDisallowed = 0x0C,

        UnsupportedFeature = 0x11,

        InvalidHciCommandParameters = 0x12,

        TerminatedRemoteUser = 0x13,

        TerminatedLocalHost = 0x16,

        UnsupportedRemoteFeature = 0x1A,

        UnspecifiedError = 0x1F,

        ProcedureTimeout = 0x22,

        InstantPassed = 0x28,

        ParameterOutOfRange = 0x30,

        HostBusyPairing = 0x38,

        ControllerBusy = 0x3A,

        DirectedAdvertisingTimeout = 0x3C,

        ConnectionEndWithMicFailure = 0x3D,
        ConnectionFailedToEstablish = 0x3E,

        // Generic/System error codes
        UnknownConnectionIdStatus = 0x40,
        Failed = 0x41,
        InvalidParameters = 0x42,
        Busy = 0x43,

        //InvalidPduLength = 0x44,
        Pending = 0x45,
        NotAllowed = 0x46,
        Error = 0x47,
        OutOfMemory = 0x48,

        // L2Cap error codes
        InvalidCid = 0x50,

        // Security Manager error codes
        DeviceInBlacklist = 0x59,
        CsrkNotFound = 0x5A,
        IrkNotFound = 0x5B,
        DeviceNotFound = 0x5C,
        SecurityDatabaseFull = 0x5D,
        DeviceNotBonded = 0x5E,
        InsufficientEncryptionKeySize = 0x5F,

        // Gatt layer error codes
        InvalidHandle = 0x60,
        OutOfHandles = 0x61,
        InvalidOperation = 0x62,
        CharacteristicAlreadyExists = 0x63,
        InsufficientResources = 0x64,
        SecurityPermissionError = 0x65,

        // Gap layer error codes
        AddressNotResolved = 0x70,

        // Link layer error codes
        NoValidSlot = 0x82,
        ScanWindowTooShort = 0x83,
        NewIntervalFailed = 0x84,
        IntervalTooLarge = 0x85,
        LenghtFailed = 0x86,

        // Flash error codes
        FlashReadFailed = 0xFA,
        FlashWriteFailed = 0xFB,
        FlashEraseFailed = 0xFC,

        // Profile library error codes
        Timeout = 0xFF,
        ProfileAlreadyInitialized = 0xF0,
        NullParam = 0xF1,
    }
}
