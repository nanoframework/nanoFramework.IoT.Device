//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;

namespace System.Net.Http.Headers
{
    // The purpose of this type is to extract the handling of general headers in one place rather than duplicating
    // functionality in both HttpRequestHeaders and HttpResponseHeaders.
    internal sealed class HttpGeneralHeaders
    {
        private readonly HttpHeaders _parent;

        internal HttpGeneralHeaders(HttpHeaders parent)
        {
            Debug.Assert(parent != null);

            _parent = parent;
        }
    }
}
