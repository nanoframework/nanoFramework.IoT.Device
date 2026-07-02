// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace nanoFramework.HomeAssistant
{
    internal static class MiniJson
    {
        /// <summary>
        /// Appends a JSON string property to the object being built.
        /// </summary>
        /// <param name="json">The JSON builder buffer.</param>
        /// <param name="first">Tracks whether this is the first property.</param>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        public static void AppendStringProperty(StringBuilder json, ref bool first, string name, string value)
        {
            AppendStringProperty(json, ref first, name, value, false);
        }

        /// <summary>
        /// Appends a JSON property as quoted string or raw token.
        /// </summary>
        /// <param name="json">The JSON builder buffer.</param>
        /// <param name="first">Tracks whether this is the first property.</param>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        /// <param name="raw">When true, writes value without JSON string quoting.</param>
        public static void AppendStringProperty(StringBuilder json, ref bool first, string name, string value, bool raw)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (!first)
            {
                json.Append(',');
            }

            json.Append('"');
            json.Append(name);
            json.Append('"');
            json.Append(':');

            if (raw)
            {
                json.Append(value);
            }
            else
            {
                json.Append('"');
                json.Append(Escape(value));
                json.Append('"');
            }

            first = false;
        }

        /// <summary>
        /// Appends a raw JSON property (unquoted value) to the object being built.
        /// </summary>
        /// <param name="json">The JSON builder buffer.</param>
        /// <param name="first">Tracks whether this is the first property.</param>
        /// <param name="name">Property name.</param>
        /// <param name="rawValue">Raw JSON value text.</param>
        public static void AppendRawProperty(StringBuilder json, ref bool first, string name, string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return;
            }

            if (!first)
            {
                json.Append(',');
            }

            json.Append('"');
            json.Append(name);
            json.Append('"');
            json.Append(':');
            json.Append(rawValue);
            first = false;
        }

        /// <summary>
        /// Appends a JSON array of strings property to the object being built.
        /// </summary>
        /// <param name="json">The JSON builder buffer.</param>
        /// <param name="first">Tracks whether this is the first property.</param>
        /// <param name="name">Property name.</param>
        /// <param name="values">String values for the JSON array.</param>
        public static void AppendStringArrayProperty(StringBuilder json, ref bool first, string name, string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return;
            }

            if (!first)
            {
                json.Append(',');
            }

            json.Append('"');
            json.Append(name);
            json.Append('"');
            json.Append(':');
            json.Append('[');

            bool firstItem = true;
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (!firstItem)
                {
                    json.Append(',');
                }

                json.Append('"');
                json.Append(Escape(value));
                json.Append('"');
                firstItem = false;
            }

            json.Append(']');
            first = false;
        }

        /// <summary>
        /// Escapes a string for inclusion in a JSON string literal.
        /// </summary>
        /// <param name="value">Input string value.</param>
        /// <returns>Escaped string value.</returns>
        public static string Escape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            StringBuilder output = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '\\')
                {
                    output.Append("\\\\");
                }
                else if (c == '"')
                {
                    output.Append("\\\"");
                }
                else if (c == '\b')
                {
                    output.Append("\\b");
                }
                else if (c == '\f')
                {
                    output.Append("\\f");
                }
                else if (c == '\n')
                {
                    output.Append("\\n");
                }
                else if (c == '\r')
                {
                    output.Append("\\r");
                }
                else if (c == '\t')
                {
                    output.Append("\\t");
                }
                else if (c < 0x20)
                {
                    AppendUnicodeEscape(output, c);
                }
                else
                {
                    output.Append(c);
                }
            }

            return output.ToString();
        }

        private static void AppendUnicodeEscape(StringBuilder output, char c)
        {
            output.Append("\\u");
            output.Append('0');
            output.Append('0');
            output.Append(ToHexDigit((c >> 4) & 0x0F));
            output.Append(ToHexDigit(c & 0x0F));
        }

        private static char ToHexDigit(int value)
        {
            if (value < 10)
            {
                return (char)('0' + value);
            }

            return (char)('A' + (value - 10));
        }
    }
}
