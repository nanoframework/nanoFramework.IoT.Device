//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;

namespace System
{
    /// <summary>
    /// Defines the kinds of <see cref="Uri"/>s for the
    /// <see cref="Uri.IsWellFormedUriString"/> method and several
    /// <see cref="Uri"/> methods.
    /// </summary>
    public enum UriKind
    {
        /// <summary>
        /// The kind of the Uri is indeterminate.
        /// </summary>
        RelativeOrAbsolute = 0,

        /// <summary>
        /// The Uri is an absolute Uri.
        /// </summary>
        Absolute = 1,

        /// <summary>
        /// The Uri is a relative Uri.
        /// </summary>
        Relative = 2,
    }

    /// <summary>
    /// Defines host name types for the http and https protocols.
    /// method.
    /// </summary>
    public enum UriHostNameType
    {
        /// <summary>
        /// The type of the host name is not supplied.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The host is set, but the type cannot be determined.
        /// </summary>
        Basic = 1,

        /// <summary>
        /// The host name is a domain name system (DNS) style host name.
        /// </summary>
        Dns = 2,

        /// <summary>
        /// The host name is an Internet Protocol (IP) version 4 host address.
        /// </summary>
        IPv4 = 3,

        /// <summary>
        /// The host name is an Internet Protocol (IP) version 6 host address.
        /// </summary>
        IPv6 = 4,
    }

    /// <summary>
    /// Provides an object representation of a uniform resource identifier (URI)
    /// and easy access to the parts of the URI.
    /// </summary>
    public class Uri
    {
        /// <summary>
        /// Specifies that the URI is accessed through the Hypertext Transfer Protocol (HTTP). This field is read-only.
        /// </summary>
        public const string UriSchemeHttp = "http";

        /// <summary>
        /// Specifies that the URI is accessed through the Secure Hypertext Transfer Protocol (HTTPS). This field is read-only.
        /// </summary>
        public const string UriSchemeHttps = "https";

        internal const string UriSchemeWs = "ws";
        internal const string UriSchemeWss = "wss";

        private int DefaultPort(string scheme)
        {
            switch (scheme)
            {
                case UriSchemeHttp:
                case UriSchemeWs:
                    return HttpDefaultPort;

                case UriSchemeHttps:
                case UriSchemeWss:
                    return HttpsDefaultPort;

                case "ftp": return 21;
                case "gopher": return 70;
                case "nntp": return 119;
                case "telnet": return 23;
                case "ldap": return 389;
                case "mailto": return 25;
                case "net.tcp": return 808;
                default: return UnknownPort;
            }
        }

        /// <summary>
        /// Defines flags kept in m_Flags variable.
        /// </summary>
        [Flags]
        protected enum Flags
        {
            /// <summary>
            /// Flag value for loopback host
            /// </summary>
            LoopbackHost = 0x00400000
        }

        /// <summary>
        /// Default port for http protocol - 80
        /// </summary>
        public const int HttpDefaultPort = 80;

        /// <summary>
        /// Default port for https protocol - 443
        /// </summary>
        public const int HttpsDefaultPort = 443;

        /// <summary>
        /// Constant to indicate that port for this protocol is unknown
        /// </summary>
        protected const int UnknownPort = -1;

        /// <summary>
        /// Type of the host.
        /// </summary>
        protected UriHostNameType _hostNameType;

        /// <summary>
        /// Member variable that keeps port used by this uri.
        /// </summary>
        protected int _port = UnknownPort;

        /// <summary>
        /// Member variable that keeps internal flags/
        /// </summary>
        protected Flags _Flags;

        /// <summary>
        /// Member variable that keeps absolute path.
        /// </summary>
        protected string _AbsolutePath = null;

        /// <summary>
        /// Member variable that keeps original string passed to Uri constructor.
        /// </summary>
        protected string _OriginalUriString = null;

        /// <summary>
        /// Member variable that keeps scheme of Uri.
        /// </summary>
        protected string _scheme = null;

        /// <summary>
        /// Member variable that keeps host name ( http and https ).
        /// </summary>
        protected string _host = "";

        /// <summary>
        /// Member variable that keeps boolean if Uri is absolute.
        /// </summary>
        protected bool _isAbsoluteUri = false;

        /// <summary>
        /// Member variable that tells if path is UNC ( Universal Naming Convention )
        /// In this class it is always false, but can be changed in derived classes.
        /// </summary>
        protected bool _isUnc = false;

        /// <summary>
        /// Member variable that keeps absolute uri (generated in method ParseUriString)
        /// </summary>
        protected string _absoluteUri = null;


        /// <summary>
        /// Gets the type of the host name specified in the URI.
        /// </summary>
        /// <value>A member of the <see cref="UriHostNameType"/>
        /// enumeration.</value>
        public UriHostNameType HostNameType => _hostNameType;

        /// <summary>
        /// Gets the port number of this URI.
        /// </summary>
        /// <value>An <itemref>Int32</itemref> value containing the port number
        /// for this URI.</value>
        /// <exception cref="InvalidOperationException">
        /// This instance represents a relative URI, and this property is valid
        /// only for absolute URIs.
        /// </exception>
        public int Port
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _port;
            }
        }

        /// <summary>
        /// Gets whether the <see cref="Uri"/> instance is absolute.
        /// </summary>
        /// <value><itemref>true</itemref> if the <itemref>Uri</itemref>
        /// instance is absolute; otherwise, <itemref>false</itemref>.</value>
        public bool IsAbsoluteUri => _isAbsoluteUri;

        /// <summary>
        /// Gets whether the specified <see cref="Uri"/> is a universal
        /// naming convention (UNC) path.
        /// </summary>
        /// <value><itemref>true</itemref> if the <see cref="Uri"/> is a
        /// UNC path; otherwise, <itemref>false</itemref>.</value>
        /// <exception cref="InvalidOperationException">
        /// This instance represents a relative URI, and this property is valid
        /// only for absolute URIs.
        /// </exception>
        public bool IsUnc
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _isUnc;
            }
        }

        /// <summary>
        /// Gets a local operating-system representation of a file name.
        /// </summary>
        /// <value>A <itemref>String</itemref> containing the local
        /// operating-system representation of a file name.</value>
        /// <exception cref="InvalidOperationException">
        /// This instance represents a relative URI, and this property is valid
        /// only for absolute URIs.
        /// </exception>
        public string AbsolutePath
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _AbsolutePath;
            }
        }

        /// <summary>
        /// Gets the original URI string that was passed to the Uri constructor.
        /// </summary>
        public string OriginalString => _OriginalUriString;

        /// <summary>
        /// Gets a string containing the absolute uri or entire uri of this instance.
        /// </summary>
        /// <value>A <itemref>String</itemref> containing the entire URI.
        /// </value>
        public string AbsoluteUri
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _absoluteUri;
            }
        }

        /// <summary>
        /// Gets the scheme name for this URI.
        /// </summary>
        /// <value>A <itemref>String</itemref> containing the scheme for this
        /// URI, converted to lowercase.</value>
        /// <exception cref="InvalidOperationException">
        /// This instance represents a relative URI, and this property is valid only
        /// for absolute URIs.
        /// </exception>
        public string Scheme
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _scheme;
            }
        }

        /// <summary>
        /// Gets the host component of this instance.
        /// </summary>
        /// <value>A <itemref>String</itemref> containing the host name.  This
        /// is usually the DNS host name or IP address of the server.</value>
        public string Host
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _host;
            }
        }

        /// <summary>
        /// Gets whether the specified <see cref="Uri"/> refers to the local host.
        /// </summary>
        /// <value><see langword="true"/> if the host specified in the Uri is the local computer; otherwise, <see langword="false"/>.</value>
        public bool IsLoopback
        {
            get
            {
                if (_isAbsoluteUri == false)
                {
                    throw new InvalidOperationException();
                }

                return _Flags.HasFlag(Flags.LoopbackHost);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Uri"/> class
        /// with the specified URI.
        /// </summary>
        /// <remarks>
        /// This constructor parses the URI string, therefore it can be used to
        /// validate a URI.
        /// </remarks>
        /// <param name="uriString">A URI.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="uriString"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <p>The <paramref name="uriString"/> is empty.</p>
        /// <p>-or-</p><p>The scheme specified in <paramref name="uriString"/>
        /// is not correctly formed.  </p>
        /// <p>-or-</p><p><paramref name="uriString"/> contains too many
        /// slashes.</p>
        /// <p>-or-</p><p>The password specified in <paramref name="uriString"/>
        /// is not valid.</p>
        /// <p>-or-</p><p>The host name specified in
        /// <paramref name="uriString"/> is not valid.</p>
        /// <p>-or-</p><p>The file name specified in
        /// <paramref name="uriString"/> is not valid.</p>
        /// <p>-or-</p><p>The user name specified in
        /// <paramref name="uriString"/> is not valid.</p>
        /// <p>-or-</p><p>The host or authority name specified in
        /// <paramref name="uriString"/> cannot be terminated by backslashes.
        /// </p>
        /// <p>-or-</p><p>The port number specified in
        /// <paramref name="uriString"/> is not valid or cannot be parsed.</p>
        /// <p>-or-</p><p>The length of <paramref name="uriString"/> exceeds
        /// 65534 characters.</p>
        /// <p>-or-</p><p>The length of the scheme specified in
        /// <paramref name="uriString"/> exceeds 1023 characters.</p>
        /// <p>-or-</p><p>There is an invalid character sequence in
        /// <paramref name="uriString"/>.</p>
        /// <p>-or-</p><p>The MS-DOS path specified in
        /// <paramref name="uriString"/> must start with c:\\.</p>
        /// </exception>
        public Uri(string uriString)
        {
            if (uriString is null)
            {
                throw new ArgumentNullException();
            }

            if (!ConstructAbsoluteUri(uriString))
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Constructs an absolute Uri from a URI string.
        /// </summary>
        /// <param name="uriString">A URI.</param>
        /// <remarks>
        /// See <see cref="Uri(string)"/>.
        /// </remarks>
        protected bool ConstructAbsoluteUri(string uriString)
        {
            // ParseUriString provides full validation including testing for null.
            if (TryParseUriString(uriString))
            {
                _OriginalUriString = uriString;

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Constructs Uri from string and enumeration that tell what is the type of Uri.
        /// </summary>
        /// <param name="uriString">String to construct Uri from</param>
        /// <param name="kind">Type of Uri to construct</param>
        /// <exception cref="ArgumentException">The scheme specified in the URI formed by combining <paramref name="baseUri"/> and <paramref name="relativeUri"/> is not valid.</exception>
        public Uri(string uriString, UriKind kind)
        {
            // ParseUriString provides full validation including testing for null.
            switch (kind)
            {
                case UriKind.Absolute:
                    {
                        if (!ConstructAbsoluteUri(uriString))
                        {
                            throw new FormatException();
                        }
                        break;
                    }

                case UriKind.RelativeOrAbsolute:
                    {
                        // try first with a absolute
                        if (ConstructAbsoluteUri(uriString))
                        {
                            break;
                        }
                        else
                        {
                            // now try with relative
                            if (!ValidateUriPart(uriString, 0))
                            {
                                throw new FormatException();
                            }
                        }
                        break;
                    }

                // Relative Uri. Store in original string.
                case UriKind.Relative:
                    {
                        // Validates the relative Uri.
                        if (!ValidateUriPart(uriString, 0))
                        {
                            throw new FormatException();
                        }
                        break;
                    }
            }

            _OriginalUriString = uriString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Uri"/> class based on the specified base URI and relative URI <see cref="string"/>.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="relativeUri">The relative URI to add to the base URI.</param>
        /// <exception cref="ArgumentNullException"><paramref name="baseUri"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="baseUri"/> is not an absolute Uri instance.</exception>
        /// <exception cref="FormatException">The scheme specified in the URI formed by combining <paramref name="baseUri"/> and <paramref name="relativeUri"/> is not valid.</exception>
        public Uri(
            Uri baseUri,
            string relativeUri = null)
        {
            if (baseUri is null)
            {
                throw new ArgumentNullException();
            }

            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (!ValidateUriPart(relativeUri, 0))
            {
                throw new FormatException();
            }

            ConstructAbsoluteUri(baseUri.AbsoluteUri + relativeUri);
        }

        /// <summary>
        /// Validates that part of Uri after sheme is valid for unknown Uri scheme
        /// </summary>
        /// <param name="uriString">Uri string </param>
        /// <param name="startIndex">Index in the string where Uri part ( after scheme ) starts</param>
        protected bool ValidateUriPart(
            string uriString,
            int startIndex)
        {
            // Check for valid alpha numeric characters
            int pathLength = uriString.Length - startIndex;

            // This is unknown scheme. We do validate following rules:
            // 1. All character values are less than 128. For characters it means they are more than zero.
            // 2. All charaters are >= 32. Lower values are control characters.
            // 3. If there is %, then there should be 2 hex digits which are 0-10 and A-F or a-f.

            for (int i = startIndex; i < pathLength; ++i)
            {
                //if (!(IsAlphaNumeric(uriString[i]) || uriString[i] == '+' || uriString[i] == '-' || uriString[i] == '.'))
                // If character is upper ( in signed more than 127, then value is negative ).
                char value = uriString[i];
                if (value < 32)
                {
                    Debug.WriteLine($"Invalid char: {value}");
                    return false;
                }

                // If it is percent, then there should be 2 hex digits after.
                if (value == '%')
                {
                    if (pathLength - i < 3)
                    {
                        Debug.WriteLine("No data after %");
                        return false;
                    }

                    // There are at least 2 characters. Check their values
                    for (int j = 1; j < 3; j++)
                    {
                        char nextVal = uriString[i + j];
                        if (!((nextVal >= '0' && nextVal <= '9')
                              || (nextVal >= 'A' && nextVal <= 'F')
                              || (nextVal >= 'a' && nextVal <= 'f')
                              )
                           )
                        {
                            Debug.WriteLine($"Invalid char after %: {value}");
                            return false;
                        }
                    }

                    // Moves i by 2 up to bypass verified characters.
                    i += 2;
                }
            }

            // got here, so must be OK
            return true;
        }

        /// <summary>
        /// Internal method parses a URI string into Uri variables
        /// </summary>
        /// <param name="uriString">A Uri.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="uriString"/> is null.
        /// </exception>
        /// <exception cref="Exception">
        /// See constructor description.
        /// </exception>
        protected bool TryParseUriString(string uriString)
        {
            int startIndex = 0;

            // Check for null or empty string.
            if (uriString == null || uriString.Length == 0)
            {
                return false;
            }
            uriString = uriString.Trim();

            // Check for presence of ':'. Colon always should be present in URI.
            if (uriString.IndexOf(':') == -1)
            {
                return false;
            }

            string uriStringLower = uriString.ToLower();

            // If this is a urn parse and return
            if (uriStringLower.IndexOf("urn:", startIndex) == 0)
            {
                return ValidateUrn(uriString);
            }

            // If the uri is a relative path parse and return
            if (uriString[0] == '/')
            {
                return ValidateRelativePath(uriString);
            }

            // Validate Scheme
            int endIndex = uriString.IndexOf(':');
            _scheme = uriString.Substring(0, endIndex);
            if (!IsAlpha(_scheme[0]))
            {
                return false;
            }

            for (int i = 1; i < _scheme.Length; ++i)
            {
                if (!(IsAlphaNumeric(_scheme[i]) || _scheme[i] == '+' || _scheme[i] == '-' || _scheme[i] == '.'))
                {
                    return false;
                }
            }

            // Get past the colon
            startIndex = endIndex + 1;
            if (startIndex >= uriString.Length)
            {
                return false;
            }

            // Get host, port and absolute path
            bool bRooted = ParseSchemeSpecificPart(uriString, startIndex);

            if ((_scheme == "file" || _scheme == "mailto") && _host.Length == 0)
            {
                _hostNameType = UriHostNameType.Basic;
            }
            else if (_host.Length == 0)
            {
                _hostNameType = UriHostNameType.Unknown;
            }
            else if (_host[0] == '[')
            {
                if (!IsIPv6(_host))
                {
                    return false;
                }

                _hostNameType = UriHostNameType.IPv6;
            }
            else if (IsIPv4(_host))
            {
                _hostNameType = UriHostNameType.IPv4;
            }
            else
            {
                _hostNameType = UriHostNameType.Dns;
            }

            if (_host != null)
            {
                if (_host == "localhost" ||
                    _host == "loopback" ||
                    (_scheme == "file" || _scheme == "mailto") && _host.Length == 0)
                {
                    _Flags |= Flags.LoopbackHost;
                }
            }

            _absoluteUri = _scheme + ":" +
                (bRooted ? "//" : string.Empty) +
                _host +
                ((DefaultPort(_scheme) == _port) ? string.Empty : ":" + _port.ToString()) +
                (_scheme == "file" && _AbsolutePath.Length >= 2 && IsAlpha(_AbsolutePath[0]) && _AbsolutePath[1] == ':' ? "/" : string.Empty) +
                _AbsolutePath;

            _isAbsoluteUri = true;
            _isUnc = _scheme == "file" && _host.Length > 0;

            // got here, so it must be OK
            return true;
        }

        /// <summary>
        /// Parse Scheme-specific part of uri for host, port and absolute path
        /// Briefed syntax abstracted from .NET FX:
        /// Group 1 - http, https, ftp, file, gopher, nntp, telnet, ldap, net.tcp and net.pipe
        ///     Must be rooted. The 1st segment is authority. Empty path should be replace as '/'
        ///     
        /// Group 2 - file
        ///     Reminder: Treat all '\' as '/'
        ///     If it starts with only one '/', host should be empty
        ///     Otherwise, all leading '/' should be ignored before searching for 1st segment. The 1st segment is host
        /// 
        /// Group 3 - news and uuid
        ///     Authority always be empty. Everything goes to path.
        ///     
        /// Group 4 - mailto and all other shemes
        ///     The 1st segment is authority iff it was not rooted.
        ///         
        /// Group 5 - all other schemes
        ///     The 1st segment is authority iff it was rooted. Empty path should be replace as '/'
        /// </summary>
        /// <param name="sUri">Scheme-specific part of uri</param>
        /// <param name="iStart"></param>
        protected bool ParseSchemeSpecificPart(
            string sUri,
            int iStart)
        {
            bool bRooted = sUri.Length >= iStart + 2 && sUri.Substring(iStart, 2) == "//";
            bool bAbsoluteUriRooted;

            string sAuthority;
            switch (_scheme)
            {
                case UriSchemeHttp:
                case UriSchemeHttps:
                case UriSchemeWs:
                case UriSchemeWss:
                case "ftp":
                case "gopher":
                case "nntp":
                case "telnet":
                case "ldap":
                case "net.tcp":
                case "net.pipe":
                    if (!bRooted)
                    {
                        throw new ArgumentException();
                    }

                    bAbsoluteUriRooted = bRooted;
                    Split(sUri, iStart + 2, out sAuthority, out _AbsolutePath, true);
                    break;

                case "file":
                    if (!bRooted)
                    {
                        throw new ArgumentException();
                    }

                    sUri = sUri.Substring(iStart + 2);
                    if (sUri.Length > 0)
                    {
                        var array = sUri.ToCharArray();
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] == '\\')
                            {
                                array[i] = '/';
                            }
                        }
                        sUri = new string(array);
                    }

                    string sTrimmed = sUri.TrimStart('/');

                    if (sTrimmed.Length >= 2 && IsAlpha(sTrimmed[0]) && sTrimmed[1] == ':')
                    {
                        //Windows style path
                        if (sTrimmed.Length < 3 || sTrimmed[2] != '/')
                        {
                            throw new ArgumentException();
                        }

                        sAuthority = string.Empty;
                        _AbsolutePath = sTrimmed;
                    }
                    else
                    {
                        //Unix style path
                        if (sUri.Length - sTrimmed.Length == 1 || sTrimmed.Length == 0)
                        {
                            sAuthority = string.Empty;
                            _AbsolutePath = sUri.Length > 0 ? sUri : "/";
                        }
                        else
                        {
                            Split(sTrimmed, 0, out sAuthority, out _AbsolutePath, true);
                        }
                    }

                    bAbsoluteUriRooted = bRooted;
                    break;

                case "news":
                case "uuid":
                    sAuthority = string.Empty;
                    _AbsolutePath = sUri.Substring(iStart);
                    bAbsoluteUriRooted = false;
                    break;

                case "mailto":
                    if (bRooted)
                    {
                        sAuthority = string.Empty;
                        _AbsolutePath = sUri.Substring(iStart);
                    }
                    else
                    {
                        Split(sUri, iStart, out sAuthority, out _AbsolutePath, false);
                    }
                    bAbsoluteUriRooted = false;
                    break;

                default:
                    if (bRooted)
                    {
                        Split(sUri, iStart + 2, out sAuthority, out _AbsolutePath, true);
                    }
                    else
                    {
                        sAuthority = string.Empty;
                        _AbsolutePath = sUri.Substring(iStart);
                    }
                    bAbsoluteUriRooted = bRooted;
                    break;
            }

            int iPortSplitter = sAuthority.LastIndexOf(':');
            if (iPortSplitter < 0 || sAuthority.LastIndexOf(']') > iPortSplitter)
            {
                _host = sAuthority;
                _port = DefaultPort(_scheme);
            }
            else
            {
                _host = sAuthority.Substring(0, iPortSplitter);
                _port = Convert.ToInt32(sAuthority.Substring(iPortSplitter + 1));
            }

            return bAbsoluteUriRooted;
        }

        protected void Split(string sUri, int iStart, out string sAuthority, out string sPath, bool bReplaceEmptyPath)
        {
            int iSplitter = sUri.IndexOf('/', iStart);
            if (iSplitter < 0)
            {
                sAuthority = sUri.Substring(iStart);
                sPath = string.Empty;
            }
            else
            {
                sAuthority = sUri.Substring(iStart, iSplitter - iStart);
                sPath = sUri.Substring(iSplitter);
            }

            if (bReplaceEmptyPath && sPath.Length == 0)
            {
                sPath = "/";
            }
        }

        /// <summary>
        /// Returns if host name is IP adress 4 bytes. Like 192.1.1.1
        /// </summary>
        /// <param name="host">string with host name</param>
        /// <returns>True if name is string with IPv4 address</returns>
        protected bool IsIPv4(string host)
        {
            int dots = 0;
            int number = 0;
            bool haveNumber = false;
            int length = host.Length;

            for (int i = 0; i < length; i++)
            {
                char ch = host[i];

                if (ch <= '9' && ch >= '0')
                {
                    haveNumber = true;
                    number = number * 10 + (host[i] - '0');
                    if (number > 255)
                    {
                        return false;
                    }
                }
                else if (ch == '.')
                {
                    if (!haveNumber)
                    {
                        return false;
                    }

                    ++dots;
                    haveNumber = false;
                    number = 0;
                }
                else
                {
                    return false;
                }
            }

            return (dots == 3) && haveNumber;
        }

        private bool IsIPv6(string host)
        {
            return host[0] == '[' && host[host.Length - 1] == ']';
        }

        /// <summary>
        /// Parses urn string into Uri variables.
        /// Parsing is restricted to basic urn:NamespaceID, urn:uuid formats only.
        /// </summary>
        /// <param name="uri">A Uri.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="uri"/> is null.
        /// </exception>
        /// <exception cref="Exception">
        /// See the constructor description.
        /// </exception>
        private bool ValidateUrn(string uri)
        {
            bool invalidUrn = false;

            // If this is a urn:uuid validate the uuid
            if (uri.ToLower().IndexOf("urn:uuid:", 0) == 0)
            {
                char[] tempUUID = uri.Substring(9).ToLower().ToCharArray();
                int length = tempUUID.Length;
                int uuidSegmentCount = 0;
                int[] delimiterIndexes = { 8, 13, 18, 23 };
                for (int i = 0; i < length; ++i)
                {
                    // Make sure these are valid hex numbers numbers
                    if (!IsHex(tempUUID[i]) && tempUUID[i] != '-')
                    {
                        invalidUrn = true;
                        break;
                    }
                    else
                    {
                        // Check each segment length
                        if (tempUUID[i] == '-')
                        {
                            if (uuidSegmentCount > 3)
                            {
                                invalidUrn = true;
                                break;
                            }

                            if (i != delimiterIndexes[uuidSegmentCount])
                            {
                                invalidUrn = true;
                                break;
                            }

                            ++uuidSegmentCount;
                        }
                    }
                }

                _AbsolutePath = uri.Substring(4);
            }

            // Else validate against RFC2141
            else
            {
                string lowerUrn = uri.Substring(4).ToLower();
                char[] tempUrn = lowerUrn.ToCharArray();

                // Validate the NamespaceID (NID)
                int index = lowerUrn.IndexOf(':');
                if (index == -1)
                {
                    throw new ArgumentException();
                }

                int i;
                for (i = 0; i < index; ++i)
                {
                    // Make sure these are valid hex numbers numbers
                    if (!IsAlphaNumeric(tempUrn[i]) && tempUrn[i] != '-')
                    {
                        invalidUrn = true;
                        break;
                    }
                }

                // Validate the Namespace String
                tempUrn = lowerUrn.Substring(index + 1).ToCharArray();
                int urnLength = tempUrn.Length;
                if (!invalidUrn && urnLength != 0)
                {
                    string otherChars = "()+,-.:=@;$_!*'";
                    for (i = 0; i < urnLength; ++i)
                    {
                        if (!IsAlphaNumeric(tempUrn[i]) && !IsHex(tempUrn[i]) && tempUrn[i] != '%' && otherChars.IndexOf(tempUrn[i]) == -1)
                        {
                            invalidUrn = true;
                            break;
                        }
                    }

                    _AbsolutePath = uri.Substring(4);
                }
            }

            if (invalidUrn)
            {
                return false;
            }

            // Set Uri properties
            _host = "";
            _isAbsoluteUri = true;
            _isUnc = false;
            _hostNameType = UriHostNameType.Unknown;
            _port = UnknownPort;
            _scheme = "urn";
            _absoluteUri = uri;

            return true;
        }

        /// <summary>
        /// Parses relative Uri into variables.
        /// </summary>
        /// <param name="uri">A Uri.</param>
        private bool ValidateRelativePath(string uri)
        {
            // Check for null
            if (uri == null || uri.Length == 0)
            {
                return false;
            }

            // Check for "//"
            if (uri[1] == '/')
            {
                return false;
            }

            // Check for alphnumeric and special characters
            for (int i = 1; i < uri.Length; ++i)
            {
                if (!IsAlphaNumeric(uri[i]) && ("()+,-.:=@;$_!*'").IndexOf(uri[i]) == -1)
                {
                    return false;
                }
            }

            _AbsolutePath = uri.Substring(1);
            _host = "";
            _isAbsoluteUri = false;
            _isUnc = false;
            _hostNameType = UriHostNameType.Unknown;
            _port = UnknownPort;

            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object o)
        {
            return this == (Uri)o;
        }

        /// <inheritdoc/>
        public static bool operator ==(
            Uri lhs,
            Uri rhs)
        {
            object l = lhs, r = rhs;

            if (l == null)
            {
                return (r == null);
            }
            else if (r == null)
            {
                return false;
            }
            else
            {
                if (lhs._isAbsoluteUri && rhs._isAbsoluteUri)
                {
                    return lhs._AbsolutePath.ToLower() == rhs._AbsolutePath.ToLower();
                }
                else
                {
                    return lhs._OriginalUriString.ToLower() == rhs._OriginalUriString.ToLower();
                }
            }
        }

        /// <inheritdoc/>
        public static bool operator !=(
            Uri lhs,
            Uri rhs)
        {
            object l = lhs, r = rhs;

            if (l == null)
            {
                return (r != null);
            }
            else if (r == null)
            {
                return true;
            }
            else
            {
                if (lhs._isAbsoluteUri && rhs._isAbsoluteUri)
                {
                    return lhs._AbsolutePath.ToLower() != rhs._AbsolutePath.ToLower();
                }
                else
                {
                    return lhs._OriginalUriString.ToLower() != rhs._OriginalUriString.ToLower();
                }
            }
        }

        /// <summary>
        /// Checks to see if the character value is an alpha character.
        /// </summary>
        /// <param name="testChar">The character to evaluate.</param>
        /// <returns><itemref>true</itemref> if the character is Alpha;
        /// otherwise, <itemref>false</itemref>.</returns>
        private bool IsAlpha(char testChar)
        {
            return (testChar >= 'A' && testChar <= 'Z') || (testChar >= 'a' && testChar <= 'z');
        }

        /// <summary>
        /// Checks to see if the character value is an alpha or numeric.
        /// </summary>
        /// <param name="testChar">The character to evaluate.</param>
        /// <returns><itemref>true</itemref> if the character is Alpha or
        /// numeric; otherwise, <itemref>false</itemref>.</returns>
        private bool IsAlphaNumeric(char testChar)
        {
            return (testChar >= 'A' && testChar <= 'Z') || (testChar >= 'a' && testChar <= 'z') || (testChar >= '0' && testChar <= '9');
        }

        /// <summary>
        /// Checks to see if the character value is Hex.
        /// </summary>
        /// <param name="testChar">The character to evaluate.</param>
        /// <returns><itemref>true</itemref> if the character is a valid Hex
        /// character; otherwise, <itemref>false</itemref>.</returns>
        private bool IsHex(char testChar)
        {
            return (testChar >= 'A' && testChar <= 'F') || (testChar >= 'a' && testChar <= 'f') || (testChar >= '0' && testChar <= '9');
        }

        /// <summary>
        /// Indicates whether the string is well-formed by attempting to
        /// construct a URI with the string.
        /// </summary>
        /// <param name="uriString">A URI.</param>
        /// <param name="uriKind">The type of the URI in
        /// <paramref name="uriString"/>.</param>
        /// <returns>
        /// <itemref>true</itemref> if the string was well-formed in accordance
        /// with RFC 2396 and RFC 2732; otherwise <itemref>false</itemref>.
        /// </returns>
        public static bool IsWellFormedUriString(
            string uriString,
            UriKind uriKind)
        {
            try
            {   // If absolute Uri was passed - create Uri object.
                switch (uriKind)
                {
                    case UriKind.Absolute:
                        {
                            Uri testUri = new Uri(uriString);

                            if (testUri.IsAbsoluteUri)
                            {
                                return true;
                            }

                            return false;
                        }

                    case UriKind.Relative:
                        {
                            Uri testUri = new Uri(uriString, UriKind.Relative);
                            if (!testUri.IsAbsoluteUri)
                            {
                                return true;
                            }

                            return false;
                        }

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
