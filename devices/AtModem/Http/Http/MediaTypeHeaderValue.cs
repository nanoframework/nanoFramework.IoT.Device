//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Http.Headers
{
    /// <summary>
    /// Represents a media type used in a Content-Type header as defined in the RFC 2616.
    /// </summary>
    /// <remarks>
    /// The MediaTypeHeaderValue class provides support for the media type used in a Content-Type header as defined in RFC 2616 by the IETF.
    /// An example of a media-type would be "text/plain; charset=iso-8859-5".
    /// </remarks>
    public class MediaTypeHeaderValue
    {
        private const string _CharSetLabel = "charset=";
        private const int _CharSetLabelLenght = 8;

        /// <summary>
        /// Gets or sets the character set.
        /// </summary>
        /// <value>The character set.</value>
        public string CharSet { get; set; }

        /// <summary>
        /// Gets or sets the media-type header value.
        /// </summary>
        /// <value>The media-type header value.</value>
        /// <remarks>
        /// The media-type is used in the Content-Type and Accept header fields in order to provide open and extensible data typing and type negotiation.
        /// </remarks>
        public string MediaType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeHeaderValue"/> class.
        /// </summary>
        /// <param name="mediaType">The source represented as a string to initialize the new instance.</param>
        /// <exception cref="ArgumentException">If <paramref name="mediaType"/> parameter is null or empty</exception>
        /// <exception cref="FormatException">If <paramref name="mediaType"/> parameter contains invalid value for <see cref="MediaType"/>.</exception>
		public MediaTypeHeaderValue(string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                throw new ArgumentException();
            }

            // sanity check for invalid content in media type
            if (mediaType.Contains(_CharSetLabel)
                || mediaType.Contains(";")
                || !mediaType.Contains("/"))
            {
                throw new FormatException();
            }

            MediaType = mediaType;
        }

        /// <summary>
        /// Converts a string to an <see cref="MediaTypeHeaderValue"/> instance.
        /// </summary>
        /// <param name="input">A string that represents media type header value information.</param>
        /// <returns>A MediaTypeHeaderValue instance.</returns>
        /// <exception cref="FormatException">input is not valid media type header value information.</exception>
        /// <exception cref="ArgumentNullException">input is a null reference.</exception>
        public static MediaTypeHeaderValue Parse(string input)
        {
            if (input is null)
            {
                throw new ArgumentNullException();
            }

            MediaTypeHeaderValue value = null;

            // format should be similar to:
            // "text/plain"
            // "text/plain; charset=utf-8"

            var values = input.Split(';');

            // remove leading and trailing spaces
            var tempValue = values[0].Trim(' ');

            if (string.IsNullOrEmpty(tempValue))
            {
                throw new FormatException();
            }

            // assume it's MediaType
            value = new MediaTypeHeaderValue(tempValue);

            if (values.Length > 1 && values.Length <= 2)
            {
                // remove leading and trailing spaces
                tempValue = values[1].Trim(' ');

                // assume 2nd part, if present is charset
                int indexOfCharSet = tempValue.IndexOf(_CharSetLabel);

                // check for misplaced start of charset
                if (indexOfCharSet != 0)
                {
                    throw new FormatException();
                }

                value.CharSet = tempValue.Substring(indexOfCharSet + _CharSetLabelLenght);
            }

            if (values.Length > 2)
            {
                throw new FormatException();
            }

            return value;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string headerValue = MediaType;

            if (CharSet is not null)
            {
                headerValue += $"; {_CharSetLabel}{CharSet}";
            }

            return headerValue;
        }
    }
}
