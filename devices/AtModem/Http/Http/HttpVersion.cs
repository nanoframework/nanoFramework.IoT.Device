//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{

    /// <summary>
    /// Defines the HTTP version numbers that are supported by the
    /// </summary>
    public class HttpVersion
    {

        /// <summary>
        /// Defines a <see cref="Version"/> instance for HTTP 1.0.
        /// </summary>
        public static readonly Version Version10 = new Version(1, 0);

        /// <summary>
        /// Defines a <see cref="Version"/> instance for HTTP 1.1.
        /// </summary>
        public static readonly Version Version11 = new Version(1, 1);

    }
}