// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace System.Security.Cryptography.X509Certificates
{
    /// <summary>
    /// Provides methods that help you use X.509 v.3 certificates.
    /// </summary>
    /// <remarks>
    /// Supported formats: DER and PEM.
    /// </remarks>
    public class X509Certificate
    {
        private readonly byte[] _certificate;

        /// <summary>
        /// Initializes a new instance of the X509Certificate class.
        /// </summary>
        public X509Certificate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate"/> class defined from a sequence of bytes representing an X.509v3 certificate.
        /// </summary>
        /// <param name="certificate">A byte array containing data from an X.509 certificate.</param>
        /// <remarks>
        /// DER and PEM encoding are the supported formats. 
        /// </remarks>
        public X509Certificate(byte[] certificate)
        {
            _certificate = certificate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate"/> class defined from a string with the content of an X.509v3 certificate.
        /// </summary>
        /// <param name="certificate">A string containing a X.509 certificate.</param>
        /// <remarks>
        /// Supported formats: DER and PEM.
        /// This methods is exclusive of .NET nanoFramework. The equivalent .NET constructor accepts a file name as the parameter.
        /// </remarks>
        public X509Certificate(string certificate)
        {
            _certificate = Encoding.UTF8.GetBytes(certificate);
        }        

        /// <summary>
        /// Returns the raw data for the entire X.509v3 certificate as an array of bytes.
        /// </summary>
        /// <returns>A byte array containing the X.509 certificate data.</returns>
        public byte[] GetRawCertData()
        {
            return _certificate;
        }
    }
}