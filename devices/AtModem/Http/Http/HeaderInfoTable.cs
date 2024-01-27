//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{
    using System.Collections;

    /// <summary>
    /// Internal class with utilities to validate HTTP headers.
    /// </summary>
    internal class HeaderInfoTable
    {
        private static HeaderInfo[] HeaderTable;

        private static readonly HeaderParser _singleParser = new(ParseSingleValue);
        private static readonly HeaderParser _multiParser = new(ParseMultiValue);

        private static string[] ParseSingleValue(string value)
        {
            return new string[1] { value };
        }

        /// <summary>
        /// Parses single HTTP header and separates values delimited by comma.
        /// Like "Content-Type: text, HTML". The value string "text, HTML" will se parsed into 2 strings.
        /// </summary>
        /// <param name="value">Value string with possible multivalue</param>
        /// <returns>Array of strings with single value in each. </returns>
        private static string[] ParseMultiValue(string value)
        {
            ArrayList tempCollection = new ArrayList();

            bool inquote = false;
            int chIndex = 0;
            char[] vp = new char[value.Length];
            string singleValue;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\"')
                {
                    inquote = !inquote;
                }
                else if ((value[i] == ',') && !inquote)
                {
                    singleValue = new String(vp, 0, chIndex);
                    tempCollection.Add(singleValue.Trim());
                    chIndex = 0;
                    continue;
                }

                vp[chIndex++] = value[i];
            }

            //
            // Now add the last of the header values to the stringtable.
            //

            if (chIndex != 0)
            {
                singleValue = new String(vp, 0, chIndex);
                tempCollection.Add(singleValue.Trim());
            }

            return (string[])tempCollection.ToArray(typeof(string));
        }

        /// <summary>
        /// Header info for non-standard headers.
        /// </summary>
        private static HeaderInfo UnknownHeaderInfo =
            new HeaderInfo(String.Empty, false, false, _singleParser);

        private static bool m_Initialized = Initialize();

        /// <summary>
        /// Initialize table with infomation for HTTP WEB headers.
        /// </summary>
        /// <returns></returns>
        private static bool Initialize()
        {

            HeaderTable = new HeaderInfo[] {
                new HeaderInfo(HttpKnownHeaderNames.Age, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Allow, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Accept, true, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Authorization, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptRanges, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptCharset, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptEncoding, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptLanguage, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Cookie, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Connection, true, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentMD5, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentType, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.CacheControl, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentRange, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLength, true, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentEncoding, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLanguage, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLocation, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Date, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.ETag, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Expect, true, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Expires, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.From, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Host, true, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfMatch, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.IfRange, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfNoneMatch, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.IfModifiedSince, true, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfUnmodifiedSince, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Location, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.LastModified, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.MaxForwards, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Pragma, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyAuthenticate, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyAuthorization, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyConnection, true, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Range, true, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Referer, true, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.RetryAfter, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Server, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.SetCookie, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.SetCookie2, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.TE, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Trailer, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.TransferEncoding, true , true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Upgrade, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.UserAgent, true, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.Via, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Vary, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.Warning, false, true, _multiParser),
                new HeaderInfo(HttpKnownHeaderNames.WWWAuthenticate, false, true, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.SecWebSocketAccept, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.SecWebSocketProtocol, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.SecWebSocketVersion, false, false, _singleParser),
                new HeaderInfo(HttpKnownHeaderNames.SecWebSocketKey, false, false, _singleParser)
            };

            return true;
        }

        /// <summary>
        /// Return HTTP header information from specified name of HTTP header.
        /// </summary>
        /// <param name="name">Name for HTTP header </param>
        /// <returns>HTTP header information</returns>
        internal HeaderInfo this[string name]
        {
            get
            {   // Return headerInfo with the same name
                string lowerCaseName = name.ToLower();
                for (int i = 0; i < HeaderTable.Length; i++)
                {
                    if (HeaderTable[i].HeaderName.ToLower() == lowerCaseName)
                    {
                        return HeaderTable[i];
                    }
                }

                // Return unknownInfo, instead of NULL
                return UnknownHeaderInfo;
            }
        }
    }
}
