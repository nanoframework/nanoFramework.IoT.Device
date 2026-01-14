// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Collections;
using IoT.Device.Graphics;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Represents Bitmap Distribution Format (BDF) font, partial implementation of specifications.
    /// Specifications can be found here: https://www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5005.BDF_Spec.pdf
    /// </summary>
    public class BdfFont
    {
        /// <summary>
        /// Character width
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Character height
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// X displacement of the character
        /// </summary>
        public int XDisplacement { get; protected set; }

        /// <summary>
        /// Y Displacement of the character
        /// </summary>
        public int YDisplacement { get; protected set; }

        /// <summary>
        /// Default character
        /// </summary>
        public int DefaultChar { get; protected set; }

        /// <summary>
        /// Number of characters
        /// </summary>
        public int CharsCount { get; protected set; }

        /// <summary>
        /// GlyphMapper is mapping from the character number to the index of the character bitmap data in the buffer GlyphUshortData.
        /// </summary>
        protected Hashtable? GlyphMapper { get; set; }
        private int BytesPerGlyph { get; set; }

        /// <summary>
        /// The buffer containing all the data
        /// </summary>
        protected ushort[]? GlyphUshortData { get; set; }

        private static readonly string FontBoundingBoxString = "FONTBOUNDINGBOX ";
        private static readonly string CharSetString = "CHARSET_REGISTRY ";
        private static readonly string IsoCharsetString = "\"ISO10646\"";
        private static readonly string DefaultCharString = "DEFAULT_CHAR ";
        private static readonly string CharsString = "CHARS ";
        private static readonly string StartCharString = "STARTCHAR ";
        private static readonly string EncodingString = "ENCODING ";
        // Those next ones are comments as not implemented but for further usage
        // private static readonly string SWidthString = "SWIDTH";
        // private static readonly string DWidthString = "DWIDTH";
        // private static readonly string VVectorString = "VVECTOR";
        private static readonly string BbxString = "BBX ";
        private static readonly string EndCharString = "ENDCHAR";
        private static readonly string BitmapString = "BITMAP";

        /// <summary>
        /// Loads BdfFont from a specified path
        /// </summary>
        /// <param name="fontFilePath">Path of the file representing the font</param>
        /// <returns>BdfFont instance</returns>
        public static BdfFont Load(string fontFilePath)
        {
            using FileStream fontStream = new FileStream(fontFilePath, FileMode.Open);
            using StreamReader sr = new StreamReader(fontStream);
            BdfFont font = new BdfFont();
            while (!sr.EndOfStream)
            {
                Span<char> span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                if (span.StartsWith(FontBoundingBoxString))
                {
                    span = span.Slice(FontBoundingBoxString.Length); // Original: .Trim();
                    font.Width = ReadNextDecimalNumber(ref span);
                    font.Height = ReadNextDecimalNumber(ref span);
                    font.XDisplacement = ReadNextDecimalNumber(ref span);
                    font.YDisplacement = ReadNextDecimalNumber(ref span);
                    font.BytesPerGlyph = (int)Math.Ceiling(((double)font.Width) / 8);
                }
                else if (span.StartsWith(CharSetString))
                {
                    span = span.Slice(CharSetString.Length); // Original: .Trim();
                    if (span.CompareTo(IsoCharsetString) != 0)
                    {
                        throw new NotSupportedException("We only support ISO10646 for now.");
                    }
                }
                else if (span.StartsWith(DefaultCharString))
                {
                    span = span.Slice(DefaultCharString.Length); // Original: .Trim();
                    font.DefaultChar = ReadNextDecimalNumber(ref span);
                }
                else if (span.StartsWith(CharsString))
                {
                    span = span.Slice(CharsString.Length); // Original: .Trim();
                    font.CharsCount = ReadNextDecimalNumber(ref span);

                    if (font.Width == 0 || font.Height == 0 || font.CharsCount <= 0)
                    {
                        throw new Exception("The font data is not well formed.");
                    }

                    font.ReadGlyphsData(sr);
                }
            }

            return font;
        }

        private static int ReadNextDecimalNumber(ref Span<char> span)
        {
            // Original code: span = span.Trim();
            // Note: we won't do it

            int sign = 1;
            if (span.Length > 0 && span[0] == '-')
            {
                sign = -1;
                span = span.Slice(1);
            }

            int number = 0;
            while (span.Length > 0 && ((uint)(span[0] - '0')) <= 9)
            {
                number = number * 10 + (span[0] - '0');
                span = span.Slice(1);
            }

            return number * sign;
        }

        private static int ReadNextHexaDecimalNumber(ref Span<char> span)
        {
            // Original code: span = span.Trim();
            // Note: we won't do it

            int number = 0;
            while (span.Length > 0)
            {
                if ((uint)(span[0] - '0') <= 9)
                {
                    number = number * 16 + (span[0] - '0');
                    span = span.Slice(1);
                    continue;
                }
                else if ((uint)((char)((char)(span[0]) - 'a')).ToLower() <= ((uint)('f' - 'a')))
                {
                    number = number * 16 + ((char)((char)(span[0]) - 'a')).ToLower() + 10;
                    span = span.Slice(1);
                    continue;
                }
                else
                {
                    break;
                }
            }

            return number;
        }

        /// <summary>
        /// Get character data or data for default character
        /// </summary>
        /// <param name="character">Character whose data needs to be retrieved</param>
        /// <param name="charData">Character data</param>
        public void GetCharData(char character, out SpanUshort charData)
        {
            if (GlyphMapper is object &&
               (GlyphMapper.TryGetValue((int)character, out int index) ||
                GlyphMapper.TryGetValue((int)DefaultChar, out index)))
            {
                // Original code: charData = GlyphUshortData.AsSpan().Slice(index, Height);
                charData = (new SpanUshort(GlyphUshortData)).Slice(index, Height);
            }
            else
            {
                throw new Exception("Couldn't get the glyph data");
            }
        }

        /// <summary>
        /// Characters supported by this font
        /// </summary>
        /// <value>Array of supported characters</value>
        public int[] SupportedChars
        {
            get
            {
                if (GlyphMapper is null)
                {
                    throw new Exception($"{nameof(GlyphMapper)} is null");
                }

                var collection = GlyphMapper.Keys;
                int[] values = new int[collection.Count];
                collection.CopyTo(values, 0);
                return values;
            }
        }

        /// <summary>
        /// Get character data
        /// </summary>
        /// <param name="charOrdinal">Character ordinal</param>
        /// <param name="data">Buffer to be sliced and filled with character data</param>
        /// <param name="useDefaultChar">Use default character if not found</param>
        /// <returns>True if data could be retrieved</returns>
        public bool GetCharData(int charOrdinal, ref SpanInt data, bool useDefaultChar = true)
        {
            if (data.Length < Height || GlyphMapper is null)
            {
                return false;
            }

            if (!GlyphMapper.TryGetValue(charOrdinal, out int index))
            {
                if (useDefaultChar)
                {
                    if (!GlyphMapper.TryGetValue(DefaultChar, out index))
                    {
                        return false;
                    }
                }
            }

            if (GlyphUshortData is null)
            {
                return false;
            }

            for (int i = 0; i < Height; i++)
            {
                data[i] = GlyphUshortData[index + i];
            }

            return true;
        }

        private void ReadGlyphsData(StreamReader sr)
        {
            if (BytesPerGlyph <= 2)
            {
                GlyphUshortData = new ushort[CharsCount * Height];
            }
            else
            {
                throw new NotSupportedException("Fonts with width more than 16 pixels is not supported.");
            }

            GlyphMapper = new Hashtable(); //Dictionary<int, int>();
            int index = 0;
            for (int i = 0; i < CharsCount; i++)
            {
                Span<char> span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                if (!span.StartsWith(StartCharString))
                {
                    throw new Exception(
                        "The font data is not well formed. expected STARTCHAR tag in the beginning of glyoh data.");
                }

                span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                if (!span.StartsWith(EncodingString))
                {
                    throw new Exception("The font data is not well formed. expected ENCODING tag.");
                }

                span = span.Slice(EncodingString.Length); // Original: .Trim();
                int charNumber = ReadNextDecimalNumber(ref span);
                GlyphMapper.Add(charNumber, index);

                do
                {
                    span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                }
                while (!span.StartsWith(BbxString));

                span = span.Slice(BbxString.Length); // Original: .Trim();
                if (ReadNextDecimalNumber(ref span) != Width ||
                    ReadNextDecimalNumber(ref span) != Height ||
                    ReadNextDecimalNumber(ref span) != XDisplacement ||
                    ReadNextDecimalNumber(ref span) != YDisplacement)
                {
                    throw new NotSupportedException(
                        "We don't support fonts have BBX values different than FONTBOUNDINGBOX values.");
                }

                span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                if (span.CompareTo(BitmapString) != 0)
                {
                    throw new Exception("The font data is not well formed. expected BITMAP tag.");
                }

                span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                int heightData = 0;
                while (heightData < Height)
                {
                    if (span.Length > 0)
                    {
                        int data = ReadNextHexaDecimalNumber(ref span);
                        GlyphUshortData[index] = (ushort)data;

                        heightData++;
                        index++;
                    }
                    else
                    {
                        span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                    }
                }

                span = new Span<char>(sr.ReadLine().Trim().ToCharArray());
                if (!span.StartsWith(EndCharString))
                {
                    throw new Exception(
                        "The font data is not well formed. expected ENDCHAR tag in the beginning of glyph data.");
                }
            }
        }
    }
}
