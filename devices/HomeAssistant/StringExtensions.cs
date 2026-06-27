// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Minimal string helpers for nanoFramework profiles that miss some APIs.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces all occurrences of oldValue with newValue.
        /// </summary>
        /// <returns>A new string with all matches replaced, or the original string when no matches exist.</returns>
        public static string Replace(this string source, string oldValue, string newValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (oldValue == null)
            {
                throw new ArgumentNullException(nameof(oldValue));
            }

            if (oldValue.Length == 0)
            {
                throw new ArgumentException("oldValue cannot be empty.", nameof(oldValue));
            }

            if (newValue == null)
            {
                newValue = string.Empty;
            }

            int matchIndex = source.IndexOf(oldValue);
            if (matchIndex < 0)
            {
                return source;
            }

            StringBuilder builder = new StringBuilder(source.Length);
            int lastIndex = 0;

            while (matchIndex >= 0)
            {
                if (matchIndex > lastIndex)
                {
                    builder.Append(source.Substring(lastIndex, matchIndex - lastIndex));
                }

                builder.Append(newValue);
                lastIndex = matchIndex + oldValue.Length;
                matchIndex = source.IndexOf(oldValue, lastIndex);
            }

            if (lastIndex < source.Length)
            {
                builder.Append(source.Substring(lastIndex));
            }

            return builder.ToString();
        }
    }
}
