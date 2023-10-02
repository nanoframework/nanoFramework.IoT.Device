//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{
    using System;

    /// <summary>
    ///  Internal support class for Validation related stuff.
    /// </summary>
    internal class ValidationHelper
    {

        public static string[] EmptyArray = new string[0];

        public static string[] MakeEmptyArrayNull(string[] stringArray)
        {
            if (stringArray == null || stringArray.Length == 0)
            {
                return null;
            }
            else
            {
                return stringArray;
            }
        }

        public static string MakeStringNull(string stringValue)
        {
            if (stringValue == null || stringValue.Length == 0)
            {
                return null;
            }
            else
            {
                return stringValue;
            }
        }

        public static string MakeStringEmpty(string stringValue)
        {
            if (stringValue == null || stringValue.Length == 0)
            {
                return String.Empty;
            }
            else
            {
                return stringValue;
            }
        }

        public static bool IsBlankString(string stringValue)
        {
            if (stringValue == null || stringValue.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ValidateRange(int actual, int fromAllowed, int toAllowed)
        {
            if (actual > toAllowed || actual < fromAllowed)
            {
                throw new ArgumentOutOfRangeException("value");
            }
        }

        public static string ExceptionMessage(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            if (exception.InnerException == null)
            {
                return exception.Message;
            }

            return exception.Message + " (" + ExceptionMessage(exception.InnerException) + ")";
        }

        internal static readonly char[] InvalidParamChars =
            new char[]{
                '(',
                ')',
                '<',
                '>',
                '@',
                ',',
                ';',
                ':',
                '\\',
                '"',
                '\'',
                '/',
                '[',
                ']',
                '?',
                '=',
                '{',
                '}',
                ' ',
                '\t',
                '\r',
                '\n'};

    }
}
