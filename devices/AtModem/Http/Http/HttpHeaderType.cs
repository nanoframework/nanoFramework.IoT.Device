//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Http.Headers
{
    [Flags]
    internal enum HttpHeaderType : byte
    {
        General = 0b0000_0001,
        Request = 0b0000_0010,
        Response = 0b0000_0100,
        Content = 0b0000_1000,
        Custom = 0b0001_0000,
        NonTrailing = 0b0010_0000,

        All = 0b0011_1111,
        None = 0
    }
}
