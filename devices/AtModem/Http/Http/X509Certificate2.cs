// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace System.Security.Cryptography.X509Certificates
{
    /// <summary>
    /// Represents an X.509 certificate.
    /// </summary>
    public class X509Certificate2 : X509Certificate
    {
        // field required to be accessible by native code
        private readonly byte[] _privateKey;
        private readonly string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class.
        /// </summary>
        public X509Certificate2()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using information from a byte array.
        /// </summary>
        /// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
        public X509Certificate2(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 certificate.
        /// </summary>
        /// <param name="certificate">A string containing a X.509 certificate.</param>
        /// <remarks>
        /// This methods is exclusive of .NET nanoFramework. The equivalent .NET constructor accepts a file name as the parameter.
        /// </remarks>
        public X509Certificate2(string certificate)
            : base(certificate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 public certificate, the private key and a password used to access the private key.
        /// </summary>
        /// <param name="rawData">A string containing a X.509 certificate.</param>
        /// <param name="key">A string containing a private key in PEM or DER format.</param>
        /// <param name="password">The password required to decrypt the private key. Set to <see langword="null"/> if the <paramref name="rawData"/> or <paramref name="key"/> are not encrypted and do not require a password.</param>
        /// <remarks>
        /// This methods is exclusive of .NET nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public X509Certificate2(
            string rawData,
            string key,
            string password)
            : base(rawData)
        {
            _privateKey = Encoding.UTF8.GetBytes(key);
            _password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 public certificate, the private key and a password used to access the certificate.
        /// </summary>
        /// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
        /// <param name="key">A string containing a private key in PEM or DER format.</param>
        /// <param name="password">The password required to decrypt the private key. Set to <see langword="null"/> if the <paramref name="rawData"/> or <paramref name="key"/> are not encrypted and do not require a password.</param>
        /// <remarks>
        /// This methods is exclusive of .NET nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public X509Certificate2(
            byte[] rawData,
            string key,
            string password)
            : base(rawData)
        {
            _privateKey = Encoding.UTF8.GetBytes(key);
            _password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 public certificate, the private key and a password used to access the certificate.
        /// </summary>
        /// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
        /// <param name="key">A byte array containing a PEM private key.</param>
        /// <param name="password">The password required to decrypt the private key. <see langword="null"/> if the <paramref name="rawData"/> or <paramref name="key"/> are not encrypted.</param>
        /// <remarks>
        /// This methods is exclusive of nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public X509Certificate2(
            byte[] rawData,
            byte[] key,
            string password)
            : base(rawData)
        {
            _privateKey = key;
            _password = password;
        }

        /// <summary>
        /// Gets a value indicating whether an <see cref="X509Certificate2"/> object contains a private key.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="X509Certificate2"/> object contains a private key; otherwise, <see langword="false"/>.</value>
        public bool HasPrivateKey
        {
            get
            {
                return _privateKey != null;
            }
        }

        /// <summary>
        /// Gets the private key, null if there isn't a private key.
        /// </summary>
        /// <remarks>This will give you access directly to the raw decoded byte array of the private key.</remarks>
        public byte[] PrivateKey => _privateKey;

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <remarks>This will give you access directly to the raw decoded byte array of the public key.</remarks>
        public byte[] PublicKey => RawData;       

        /// <summary>
        /// Gets the raw data of a certificate.
        /// </summary>
        /// <value>The raw data of the certificate as a byte array.</value>
        public byte[] RawData
        {
            get
            {
                return GetRawCertData();
            }
        }
    }
}
