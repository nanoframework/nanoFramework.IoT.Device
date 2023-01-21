// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Globalization;

namespace Iot.Device.Graphics
{
    /// <summary>
    /// Factory for creating Encodings that support different cultures on different LCD Displays.
    /// </summary>
    public class LcdCharacterEncodingFactory
    {
        // Default maps for the HD44780 controller
        private static readonly DictionaryCharByte DefaultA00Map;
        private static readonly DictionaryCharByte DefaultA02Map;
        // This map is used on SPLC780 controllers, it seems. They're otherwise compatible to the HD44780.
        private static readonly DictionaryCharByte DefaultSplC780Map;
        private static readonly DictionaryCharByte DefaultCustomMap;

        static LcdCharacterEncodingFactory()
        {
            // Default map which is used for unknown ROMs
            DefaultCustomMap = new DictionaryCharByte();
            // The character map A00 contains the most used european letters, some greek math symbols plus japanese letters.
            // Compare with the HD44780 specification sheet, page 17
            DefaultA00Map = new DictionaryCharByte();

            // Now the japanese characters in the A00 rom map.
            // They use the same order than described in https://de.wikipedia.org/wiki/Japanische_Schrift#F%C3%BCnfzig-Laute-Tafel Table "Katakana", so the mapping sounds reasonable.
            // Small letters
            DefaultA00Map.Add(new CharByte('ヽ', 0b1010_0100));
            DefaultA00Map.Add(new CharByte('・', 0b1010_0101));
            DefaultA00Map.Add(new CharByte('ァ', 0b1010_0111));
            DefaultA00Map.Add(new CharByte('ィ', 0b1010_1000));
            DefaultA00Map.Add(new CharByte('ゥ', 0b1010_1001));
            DefaultA00Map.Add(new CharByte('ェ', 0b1010_1010));
            DefaultA00Map.Add(new CharByte('ォ', 0b1010_1011));
            DefaultA00Map.Add(new CharByte('ャ', 0b1010_1100));
            DefaultA00Map.Add(new CharByte('ュ', 0b1010_1101));
            DefaultA00Map.Add(new CharByte('ョ', 0b1010_1110));
            DefaultA00Map.Add(new CharByte('ヮ', 0b1010_1111)); // Not sure on this one
                                                               // Normal letters
            DefaultA00Map.Add(new CharByte('ー', 0b1011_0000));
            DefaultA00Map.Add(new CharByte('ア', 0b1011_0001));
            DefaultA00Map.Add(new CharByte('イ', 0b1011_0010));
            DefaultA00Map.Add(new CharByte('ウ', 0b1011_0011));
            DefaultA00Map.Add(new CharByte('エ', 0b1011_0100));
            DefaultA00Map.Add(new CharByte('オ', 0b1011_0101));
            DefaultA00Map.Add(new CharByte('カ', 0b1011_0110));
            DefaultA00Map.Add(new CharByte('キ', 0b1011_0111));
            DefaultA00Map.Add(new CharByte('ク', 0b1011_1000));
            DefaultA00Map.Add(new CharByte('ケ', 0b1011_1001));
            DefaultA00Map.Add(new CharByte('コ', 0b1011_1010));
            DefaultA00Map.Add(new CharByte('サ', 0b1011_1011));
            DefaultA00Map.Add(new CharByte('シ', 0b1011_1100));
            DefaultA00Map.Add(new CharByte('ス', 0b1011_1101));
            DefaultA00Map.Add(new CharByte('セ', 0b1011_1110));
            DefaultA00Map.Add(new CharByte('ソ', 0b1011_1111));
            DefaultA00Map.Add(new CharByte('タ', 0b1100_0000));
            DefaultA00Map.Add(new CharByte('チ', 0b1100_0001));
            DefaultA00Map.Add(new CharByte('ツ', 0b1100_0010));
            DefaultA00Map.Add(new CharByte('テ', 0b1100_0011));
            DefaultA00Map.Add(new CharByte('ト', 0b1100_0100));
            DefaultA00Map.Add(new CharByte('ナ', 0b1100_0101));
            DefaultA00Map.Add(new CharByte('ニ', 0b1100_0110));
            DefaultA00Map.Add(new CharByte('ヌ', 0b1100_0111));
            DefaultA00Map.Add(new CharByte('ネ', 0b1100_1000));
            DefaultA00Map.Add(new CharByte('ノ', 0b1100_1001));
            DefaultA00Map.Add(new CharByte('ハ', 0b1100_1010));
            DefaultA00Map.Add(new CharByte('ヒ', 0b1100_1011));
            DefaultA00Map.Add(new CharByte('フ', 0b1100_1100));
            DefaultA00Map.Add(new CharByte('ヘ', 0b1100_1101));
            DefaultA00Map.Add(new CharByte('ホ', 0b1100_1110));
            DefaultA00Map.Add(new CharByte('マ', 0b1100_1111));
            DefaultA00Map.Add(new CharByte('ミ', 0b1101_0000));
            DefaultA00Map.Add(new CharByte('ム', 0b1101_0001));
            DefaultA00Map.Add(new CharByte('メ', 0b1101_0010));
            DefaultA00Map.Add(new CharByte('モ', 0b1101_0011));
            DefaultA00Map.Add(new CharByte('ヤ', 0b1101_0100));
            DefaultA00Map.Add(new CharByte('ユ', 0b1101_0101));
            DefaultA00Map.Add(new CharByte('ヨ', 0b1101_0110));
            DefaultA00Map.Add(new CharByte('ラ', 0b1101_0111));
            DefaultA00Map.Add(new CharByte('リ', 0b1101_1000));
            DefaultA00Map.Add(new CharByte('ル', 0b1101_1001));
            DefaultA00Map.Add(new CharByte('レ', 0b1101_1010));
            DefaultA00Map.Add(new CharByte('ロ', 0b1101_1011));
            DefaultA00Map.Add(new CharByte('ワ', 0b1101_1100));
            DefaultA00Map.Add(new CharByte('ン', 0b1101_1101)); // Characters ヰ and ヱ seem not to exist, they are not used in current japanese and can be replaced by イ and エ
            DefaultA00Map.Add(new CharByte('ヰ', 0b1011_0010));
            DefaultA00Map.Add(new CharByte('ヱ', 0b1011_0100));
            DefaultA00Map.Add(new CharByte('ヲ', 0b1010_0110)); // This one is out of sequence
            DefaultA00Map.Add(new CharByte('゛', 0b1101_1110));
            DefaultA00Map.Add(new CharByte('゜', 0b1101_1111));
            // break in character type
            DefaultA00Map.Add(new CharByte('¥', 92));
            DefaultA00Map.Add(new CharByte('{', 123));
            DefaultA00Map.Add(new CharByte('|', 124));
            DefaultA00Map.Add(new CharByte('}', 125));
            DefaultA00Map.Add(new CharByte('\u2192', 126)); // right arrow
            DefaultA00Map.Add(new CharByte('\u2190', 127)); // left arrow
                                                            // Note: Several letters may point to the same character code
            DefaultA00Map.Add(new CharByte('–', 0b1011_0000));
            DefaultA00Map.Add(new CharByte('α', 224));
            DefaultA00Map.Add(new CharByte('ä', 225));
            DefaultA00Map.Add(new CharByte('β', 226));
            DefaultA00Map.Add(new CharByte('ε', 227));
            DefaultA00Map.Add(new CharByte('μ', 228));
            DefaultA00Map.Add(new CharByte('δ', 229));
            DefaultA00Map.Add(new CharByte('ρ', 230));
            // Character 231 looks like a small q, but what should it be?
            DefaultA00Map.Add(new CharByte('√', 232));
            // What is the match for chars 233, 234 and 235?
            DefaultA00Map.Add(new CharByte('¢', 236));
            DefaultA00Map.Add(new CharByte('ñ', 238));
            DefaultA00Map.Add(new CharByte('ö', 239));
            // 240 and 241 look like p and q again. What are they?
            DefaultA00Map.Add(new CharByte('θ', 242));
            DefaultA00Map.Add(new CharByte('∞', 243));
            DefaultA00Map.Add(new CharByte('Ω', 244));
            DefaultA00Map.Add(new CharByte('Ω', 244));
            DefaultA00Map.Add(new CharByte('ü', 245));
            DefaultA00Map.Add(new CharByte('∑', 246));
            DefaultA00Map.Add(new CharByte('π', 247));
            // Some unrecognized characters here as well
            DefaultA00Map.Add(new CharByte('÷', 253));
            DefaultA00Map.Add(new CharByte('×', (byte)'x'));
            DefaultA00Map.Add(new CharByte('█', 255));
            DefaultA00Map.Add(new CharByte('°', 0b1101_1111));

            // Character map A02 contains about all characters used in western european languages, a few greek math symbols and some symbols.
            // Compare with the HD44780 specification sheet, page 18.
            // Note especially that this character map really uses the 8 pixel height of the characters, while the A00 map leaves the lowest
            // pixel row usually empty for the cursor. The added space at the top of the character helps by better supporting diacritical symbols.
            DefaultA02Map = new DictionaryCharByte();

                // This map contains wide arrow characters. They could be helpful for menus, but not sure where to map them.
            DefaultA02Map.Add(new CharByte('{', 123));
            DefaultA02Map.Add(new CharByte('|', 124));
            DefaultA02Map.Add(new CharByte('}', 125));
            DefaultA02Map.Add(new CharByte('~', 126));
            DefaultA02Map.Add(new CharByte('→', 0b0001_1010)); // right arrow
            DefaultA02Map.Add(new CharByte('←', 0b0001_1011)); // left arrow
            DefaultA02Map.Add(new CharByte('↑', 0b0001_1000));
            DefaultA02Map.Add(new CharByte('↓', 0b0001_1001));
            DefaultA02Map.Add(new CharByte('↲', 0b0001_0111));
            DefaultA02Map.Add(new CharByte('≤', 0b0001_1100));
            DefaultA02Map.Add(new CharByte('≥', 0b0001_1101));
            DefaultA02Map.Add(new CharByte('°', 0b1011_0000));
            // Cyrillic script, capital letters (russian, slawic languages)
            DefaultA02Map.Add(new CharByte('А', (byte)'A'));
            DefaultA02Map.Add(new CharByte('Б', 0b1000_0000));
            DefaultA02Map.Add(new CharByte('В', (byte)'B'));
            DefaultA02Map.Add(new CharByte('Г', 0b1001_0010));
            DefaultA02Map.Add(new CharByte('Д', 0b1000_0001));
            DefaultA02Map.Add(new CharByte('Е', (byte)'E'));
            DefaultA02Map.Add(new CharByte('Ж', 0b1000_0010));
            DefaultA02Map.Add(new CharByte('З', 0b1000_0011));
            DefaultA02Map.Add(new CharByte('И', 0b1000_0100));
            DefaultA02Map.Add(new CharByte('Й', 0b1000_0101));
            DefaultA02Map.Add(new CharByte('К', (byte)'K'));
            DefaultA02Map.Add(new CharByte('Л', 0b1000_0110));
            DefaultA02Map.Add(new CharByte('М', (byte)'M'));
            DefaultA02Map.Add(new CharByte('Н', (byte)'H'));
            DefaultA02Map.Add(new CharByte('О', (byte)'O'));
            DefaultA02Map.Add(new CharByte('П', 0b1000_0111));
            DefaultA02Map.Add(new CharByte('Р', (byte)'P'));
            DefaultA02Map.Add(new CharByte('С', (byte)'C'));
            DefaultA02Map.Add(new CharByte('Т', (byte)'T'));
            DefaultA02Map.Add(new CharByte('У', 0b1000_1000));
            DefaultA02Map.Add(new CharByte('Ф', 0b1111_1000));
            DefaultA02Map.Add(new CharByte('Х', (byte)'X'));
            DefaultA02Map.Add(new CharByte('Ц', 0b1000_1001));
            DefaultA02Map.Add(new CharByte('Ч', 0b1000_1010));
            DefaultA02Map.Add(new CharByte('Ш', 0b1000_1011));
            DefaultA02Map.Add(new CharByte('Щ', 0b1000_1100));
            DefaultA02Map.Add(new CharByte('Ъ', 0b1000_1101));
            DefaultA02Map.Add(new CharByte('Ы', 0b1000_1110));
            DefaultA02Map.Add(new CharByte('Ь', (byte)'b'));
            DefaultA02Map.Add(new CharByte('Э', 0b1000_1111));
            DefaultA02Map.Add(new CharByte('Ю', 0b1010_1100));
            DefaultA02Map.Add(new CharByte('Я', 0b1010_1101));
            DefaultA02Map.Add(new CharByte('μ', 0b1011_0101));
            DefaultA02Map.Add(new CharByte('¡', 0b1010_0001));
            DefaultA02Map.Add(new CharByte('¿', 0b1011_1111));
            // Cyrillic script, special letters
            DefaultA02Map.Add(new CharByte('Ё', 0b1100_1011));
            // Not available in ROM: ЂЃЄ
            DefaultA02Map.Add(new CharByte('Ѕ', (byte)'S'));
            DefaultA02Map.Add(new CharByte('І', (byte)'I'));
            DefaultA02Map.Add(new CharByte('Ї', 0b1100_1111));
            DefaultA02Map.Add(new CharByte('Ј', (byte)'J'));
            // Not available in ROM: ЉЊЋЌЏ
            DefaultA02Map.Add(new CharByte('Ў', 0b1101_1101));
            DefaultA02Map.Add(new CharByte('–', 0b0010_1101));
            DefaultA02Map.Add(new CharByte('α', 0b1001_0000));
            DefaultA02Map.Add(new CharByte('ε', 0b1001_1110));
            DefaultA02Map.Add(new CharByte('δ', 0b1001_1011));
            DefaultA02Map.Add(new CharByte('σ', 0b1001_0101));
            // 240 and 241 look like p and q again. What are they?
            DefaultA02Map.Add(new CharByte('θ', 0b1001_1001));
            DefaultA02Map.Add(new CharByte('Ω', 0b1001_1010));
            DefaultA02Map.Add(new CharByte('Ω', 0b1001_1010));
            DefaultA02Map.Add(new CharByte('∑', 0b1001_0100));
            DefaultA02Map.Add(new CharByte('π', 0b1001_0011));
            // West european diacritics (german, spanish, french, scandinavian languages)
            DefaultA02Map.Add(new CharByte('À', 0b1100_0000));
            DefaultA02Map.Add(new CharByte('Á', 0b1100_0001));
            DefaultA02Map.Add(new CharByte('Â', 0b1100_0010));
            DefaultA02Map.Add(new CharByte('Ã', 0b1100_0011));
            DefaultA02Map.Add(new CharByte('Å', 0b1100_0100));
            DefaultA02Map.Add(new CharByte('Æ', 0b1100_0101));
            DefaultA02Map.Add(new CharByte('Ç', 0b1100_0111));
            DefaultA02Map.Add(new CharByte('È', 0b1100_1000));
            DefaultA02Map.Add(new CharByte('É', 0b1100_1001));
            DefaultA02Map.Add(new CharByte('Ê', 0b1100_1010));
            DefaultA02Map.Add(new CharByte('Ë', 0b1100_1011));
            DefaultA02Map.Add(new CharByte('Ì', 0b1100_1100));
            DefaultA02Map.Add(new CharByte('Í', 0b1100_1101));
            DefaultA02Map.Add(new CharByte('Î', 0b1100_1110));
            DefaultA02Map.Add(new CharByte('Ï', 0b1100_1111));
            DefaultA02Map.Add(new CharByte('Đ', 0b1101_0000));
            DefaultA02Map.Add(new CharByte('Ñ', 0b1101_0001));
            DefaultA02Map.Add(new CharByte('Ò', 0b1101_0010));
            DefaultA02Map.Add(new CharByte('Ó', 0b1101_0011));
            DefaultA02Map.Add(new CharByte('Ô', 0b1101_0100));
            DefaultA02Map.Add(new CharByte('Õ', 0b1101_0101));
            DefaultA02Map.Add(new CharByte('Ö', 0b1101_0110));
            DefaultA02Map.Add(new CharByte('×', 0b1101_0111));
            DefaultA02Map.Add(new CharByte('Ø', 0b1101_1000));
            DefaultA02Map.Add(new CharByte('Ù', 0b1101_1001));
            DefaultA02Map.Add(new CharByte('Ú', 0b1101_1010));
            DefaultA02Map.Add(new CharByte('Û', 0b1101_1011));
            DefaultA02Map.Add(new CharByte('Ü', 0b1101_1100));
            DefaultA02Map.Add(new CharByte('Ý', 0b1101_1101));
            DefaultA02Map.Add(new CharByte('Þ', 0b1101_1110));
            DefaultA02Map.Add(new CharByte('ß', 0b1101_1111));
            // break in characters
            DefaultA02Map.Add(new CharByte('à', 0b1110_0000));
            DefaultA02Map.Add(new CharByte('á', 0b1110_0001));
            DefaultA02Map.Add(new CharByte('â', 0b1110_0010));
            DefaultA02Map.Add(new CharByte('ã', 0b1110_0011));
            DefaultA02Map.Add(new CharByte('å', 0b1110_0100));
            DefaultA02Map.Add(new CharByte('æ', 0b1110_0101));
            DefaultA02Map.Add(new CharByte('ç', 0b1110_0111));
            DefaultA02Map.Add(new CharByte('è', 0b1110_1000));
            DefaultA02Map.Add(new CharByte('é', 0b1110_1001));
            DefaultA02Map.Add(new CharByte('ê', 0b1110_1010));
            DefaultA02Map.Add(new CharByte('ë', 0b1110_1011));
            DefaultA02Map.Add(new CharByte('ì', 0b1110_1100));
            DefaultA02Map.Add(new CharByte('í', 0b1110_1101));
            DefaultA02Map.Add(new CharByte('î', 0b1110_1110));
            DefaultA02Map.Add(new CharByte('ï', 0b1110_1111));
            // break in characters
            DefaultA02Map.Add(new CharByte('đ', 0b1111_0000));
            DefaultA02Map.Add(new CharByte('ð', 0b1111_0000));
            DefaultA02Map.Add(new CharByte('ñ', 0b1111_0001));
            DefaultA02Map.Add(new CharByte('ò', 0b1111_0010));
            DefaultA02Map.Add(new CharByte('ó', 0b1111_0011));
            DefaultA02Map.Add(new CharByte('ô', 0b1111_0100));
            DefaultA02Map.Add(new CharByte('ö', 0b1111_0101));
            DefaultA02Map.Add(new CharByte('÷', 0b1111_0111));
            DefaultA02Map.Add(new CharByte('ø', 0b1111_1000));
            DefaultA02Map.Add(new CharByte('ù', 0b1111_1001));
            DefaultA02Map.Add(new CharByte('ú', 0b1111_1010));
            DefaultA02Map.Add(new CharByte('û', 0b1111_1011));
            DefaultA02Map.Add(new CharByte('ü', 0b1111_1100));
            DefaultA02Map.Add(new CharByte('ý', 0b1111_1101));
            DefaultA02Map.Add(new CharByte('þ', 0b1111_1110));
            DefaultA02Map.Add(new CharByte('ÿ', 0b1111_1111));


            // This map for instance can be found here: https://www.microchip.com/forums/m977852.aspx
            DefaultSplC780Map = new DictionaryCharByte();

            // Map for the SplC780
            DefaultSplC780Map.Add(new CharByte('{', 123));
            DefaultSplC780Map.Add(new CharByte('|', 124));
            DefaultSplC780Map.Add(new CharByte('}', 125));
            DefaultSplC780Map.Add(new CharByte('~', 0126));
            DefaultSplC780Map.Add(new CharByte('Ç', 0128));
            DefaultSplC780Map.Add(new CharByte('ü', 0129));
            DefaultSplC780Map.Add(new CharByte('é', 0130));
            DefaultSplC780Map.Add(new CharByte('â', 0131));
            DefaultSplC780Map.Add(new CharByte('å', 0131));
            DefaultSplC780Map.Add(new CharByte('ä', 0132));
            DefaultSplC780Map.Add(new CharByte('à', 0133));
            DefaultSplC780Map.Add(new CharByte('ả', 0134));
            DefaultSplC780Map.Add(new CharByte('ç', 0135));
            DefaultSplC780Map.Add(new CharByte('ê', 0136));
            DefaultSplC780Map.Add(new CharByte('ë', 0137));
            DefaultSplC780Map.Add(new CharByte('è', 0138));
            DefaultSplC780Map.Add(new CharByte('ï', 0139));
            DefaultSplC780Map.Add(new CharByte('î', 0140));
            DefaultSplC780Map.Add(new CharByte('ì', 0141));
            DefaultSplC780Map.Add(new CharByte('Ä', 0142));
            DefaultSplC780Map.Add(new CharByte('Å', 0143));
            DefaultSplC780Map.Add(new CharByte('φ', 0xCD));
            // Complete the set of capital greek letters for those that look like latin letters (note that these are not identity assignments)
            DefaultSplC780Map.Add(new CharByte('Α', (byte)'A'));
            DefaultSplC780Map.Add(new CharByte('Β', (byte)'B'));
            DefaultSplC780Map.Add(new CharByte('Ε', (byte)'E'));
            DefaultSplC780Map.Add(new CharByte('Ζ', (byte)'Z'));
            DefaultSplC780Map.Add(new CharByte('Η', (byte)'H'));
            DefaultSplC780Map.Add(new CharByte('Ι', (byte)'I'));
            DefaultSplC780Map.Add(new CharByte('Κ', (byte)'K'));
            DefaultSplC780Map.Add(new CharByte('Μ', (byte)'M'));
            DefaultSplC780Map.Add(new CharByte('Ν', (byte)'N'));
            DefaultSplC780Map.Add(new CharByte('Ο', (byte)'O'));
            DefaultSplC780Map.Add(new CharByte('Ρ', (byte)'P'));
            DefaultSplC780Map.Add(new CharByte('Τ', (byte)'T'));
            DefaultSplC780Map.Add(new CharByte('Υ', (byte)'Y'));
            DefaultSplC780Map.Add(new CharByte('Χ', (byte)'X'));

            void AddToAllMaps(char c, char c1)
            {
                DefaultCustomMap.Add(new CharByte(c, (byte)c1));
                DefaultA00Map.Add(new CharByte(c, (byte)c1));
                DefaultA02Map.Add(new CharByte(c, (byte)c1));
                DefaultSplC780Map.Add(new CharByte(c, (byte)c1));
            }

            // Inserts ASCII characters ' ' to 'z', which are common to most known character sets
            for (char c = ' '; c <= 'z'; c++)
            {
                AddToAllMaps(c, c);
            }

            AddToAllMaps('’', '\'');
            AddToAllMaps('‚', '\'');
            AddToAllMaps('‘', '\'');
            AddToAllMaps('„', '\"');
            AddToAllMaps('“', '\"');

            // DefaultA00Map.Remove('\\')); // Instead of the backspace, the Yen letter is in the map, but we can use char 164 instead
            DefaultA00Map.Add(new CharByte('\\', 164));

            string cyrillicLettersSmall = "абвгдежзийклмнопрстуфхцчшщъыьэюяёђѓєѕіїјљњћќўџ";
            string cyrillicLettersCapital = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯЁЂЃЄЅІЇЈЉЊЋЌЎЏ";

            // Map the small cycrillic letters to their capital equivalents
            for (int i = 0; i < cyrillicLettersSmall.Length; i++)
            {
                char small = cyrillicLettersSmall[i];
                char capital = cyrillicLettersCapital[i];
                byte dataPoint;
                if (DefaultA02Map.TryGetValue(capital, out dataPoint))
                {
                    DefaultA02Map.Add(small, dataPoint);
                }
            }

            // A bit easier like this...
            string toAdd = "ÉæÆôöòûùÿÖÜñÑ  ¿"
                           + "áíóú₡£¥₧ ¡ÃãÕõØø"
                           + " ¨° ´½¼×÷≤≥«»≠√ "
                           + "              ®©" // Not much useful in this row
                           + "™†§¶Γ ΘΛΞΠΣ ΦΨΩα"
                           + "βγδεζηθικλμνξπρσ"
                           + "τυχψω"; // greek letter small phi may apparently be represented by its capital letter

            byte startIndex = 144;
            foreach (char c in toAdd)
            {
                if (c != ' ')
                {
                    DefaultSplC780Map.Add(c, startIndex);
                }

                startIndex++;
            }
        }

        /// <summary>
        /// Creates the character mapping optimized for the given culture.
        /// Checks whether the characters required for a given culture are available in the installed character map and tries
        /// to add them as user-defined characters, if possible.
        /// </summary>
        /// <param name="culture">Culture for which support is required</param>
        /// <param name="romName">ROM type of attached chip. Supported values: "A00", "A02", "SplC780"</param>
        /// <param name="unknownLetter">Letter that is printed when an unknown character is encountered. This letter must be part of the
        /// default rom set</param>
        /// <param name="maxNumberOfCustomCharacters">Maximum number of custom characters supported on the hardware. Should be 8 for Hd44780-controlled displays.</param>
        /// <returns>The newly created encoding. Whether the encoding can be loaded to a certain display will be decided later.</returns>
        /// <exception cref="ArgumentException">The character specified as unknownLetter must be part of the mapping.</exception>
        public LcdCharacterEncoding Create(string culture, string romName, char unknownLetter, int maxNumberOfCustomCharacters)
        {
            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            // Need to copy the static map, we must not update that
            DictionaryCharByte newMap = romName switch
            {
                "A00" => DefaultA00Map,
                "A02" => DefaultA02Map,
                "SplC780" => DefaultSplC780Map,
                _ => DefaultCustomMap,
            };

            ArrayList extraCharacters = new();
            bool supported = AssignLettersForCurrentCulture(newMap, culture, romName, extraCharacters, maxNumberOfCustomCharacters);

            if (!newMap.ContainsKey(unknownLetter))
            {
                throw new ArgumentException("The replacement letter is not part of the mapping", nameof(unknownLetter));
            }

            var encoding = new LcdCharacterEncoding(culture, romName, newMap, unknownLetter, extraCharacters);
            encoding.AllCharactersSupported = !supported;
            return encoding;
        }

        /// <summary>
        /// Tries to generate letters important in that culture but missing from the current rom set
        /// </summary>
        private bool AssignLettersForCurrentCulture(DictionaryCharByte characterMapping, string culture, string romName, ArrayList extraCharacters, int maxNumberOfCustomCharacters)
        {
            string specialLetters = SpecialLettersForCulture(culture, characterMapping); // Special letters this language group uses, in order of importance

            byte charPos = 0;
            foreach (char c in specialLetters)
            {
                if (!characterMapping.ContainsKey(c))
                {
                    // This letter doesn't exist, insert it
                    if (charPos < maxNumberOfCustomCharacters)
                    {
                        var pixelMap = CreateLetter(c, romName);
                        if (pixelMap is object)
                        {
                            extraCharacters.Add(pixelMap);
                            characterMapping.Add(c, charPos);
                            charPos++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the set of special characters required for a given culture/language.
        /// This may include diacritics (ä, ö, ø), currency signs (€) or similar chars.
        /// If any of the returned characters are not found in the ROM map, <see cref="CreateLetter"/> is called to obtain the pixel representation of the given character.
        /// A maximum of 8 extra characters can be added to the ones in the ROM.
        /// </summary>
        /// <param name="culture">Culture to support</param>
        /// <param name="characterMapping">The character map, pre-loaded with the characters from the character ROM. This may be extended by explicitly adding direct mappings
        /// where an alternative is allowed (i.e. mapping capital diacritics to normal capital letters É -> E, when there's not enough room to put É into character RAM.</param>
        /// <returns>A string with the set of special characters for a language, i.e. "äöüß€ÄÖÜ" for German</returns>
        protected virtual string SpecialLettersForCulture(string culture, DictionaryCharByte characterMapping)
        {
            string mainCultureName = culture;
            int idx = mainCultureName.IndexOf('-');
            if (idx > 0)
            {
                mainCultureName = mainCultureName.Substring(0, idx);
            }

            string specialLetters;
            switch (mainCultureName)
            {
                case "en": // English doesn't use any diacritics, so insert some chars that might be helpful anyway
                    specialLetters = "€£";
                    break;
                case "ja": // Japanese
                           // About all the letters. They're there if we use rom map A00, otherwise this will later fail
                    specialLetters = "イロハニホヘトチリヌルヲワカヨタレソツネナラムウヰノオクヤマケフコエテアサキユメミシヱヒモセス";
                    break;
                case "de":
                    specialLetters = "äöüß€ÄÖÜ£ë";
                    break;
                case "fr":
                    specialLetters = "èéêà€çôù";
                    // If the character map doesn't already contain them, we map these diacritical capital letters to their non-diacritical variants.
                    // That's common in french.
                    characterMapping.TryAdd('É', (byte)'E');
                    characterMapping.TryAdd('È', (byte)'E');
                    characterMapping.TryAdd('Ê', (byte)'E');
                    characterMapping.TryAdd('À', (byte)'A');
                    characterMapping.TryAdd('Ç', (byte)'C');
                    break;
                case "no":
                case "da":
                    specialLetters = "æå€øÆÅØ";
                    break;
                case "sv":
                    specialLetters = "åÅöÖüÜ";
                    break;
                case "es":
                    specialLetters = "ñ€¿¡";
                    break;
                case "uk":
                case "ru":
                    specialLetters = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ"; // cyryllic script used for russian (only works on ROM A02, capital letters only)
                    break;
                default:
                    specialLetters = "€£";
                    break;
            }

            return specialLetters;
        }

        /// <summary>
        /// Creates the given letter for the given ROM type.
        /// Overwrite this only if an alternate ROM is used.
        /// </summary>
        protected virtual byte[]? CreateLetter(char character, string romName) => romName switch
        {
            "A00" => CreateLetterA00(character),
            "A02" => CreateLetterA02(character),
            // The font looks identical, so we can use the same lookup table
            "SplC780" => CreateLetterA00(character),
            _ => null,
        };

        /// <summary>
        /// Creates the given letter from a pixel map for Rom Type A00 (7-pixel high letters, bottom row empty)
        /// </summary>
        /// <param name="character">Character to create</param>
        /// <returns>An 8-Byte array of the pixel map for the created letter.</returns>
        /// <remarks>
        /// Currently requires the characters to be hardcoded here. Would be nice if we could generate the pixel maps from an existing font, such as Consolas
        /// </remarks>
        protected virtual byte[]? CreateLetterA00(char character) => character switch
        {
            '€' => CreateCustomCharacter(
                    0b_00111,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_00111,
                    0b_00000),
            '£' => CreateCustomCharacter(
                    0b_00110,
                    0b_01001,
                    0b_01000,
                    0b_11111,
                    0b_01000,
                    0b_01000,
                    0b_11111,
                    0b_00000),
            'Ä' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            'Ö' => CreateCustomCharacter(
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'Ü' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ß' => CreateCustomCharacter(
                    0b_00000,
                    0b_00110,
                    0b_01001,
                    0b_01110,
                    0b_01001,
                    0b_01001,
                    0b_10110,
                    0b_00000),
            'Å' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            // Same as above, cannot really distinguish them in the 7-pixel font (but they would not normally be used by the same languages)
            'Â' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_00100,
                    0b_01010,
                    0b_11111,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            'æ' => CreateCustomCharacter(
                    0b_00000,
                    0b_00000,
                    0b_11010,
                    0b_00101,
                    0b_01111,
                    0b_10100,
                    0b_01011,
                    0b_00000),
            'Æ' => CreateCustomCharacter(
                    0b_00111,
                    0b_00100,
                    0b_01100,
                    0b_10111,
                    0b_11100,
                    0b_10100,
                    0b_10111,
                    0b_00000),
            'ø' => CreateCustomCharacter(
                    0b_00000,
                    0b_00000,
                    0b_01110,
                    0b_10011,
                    0b_10101,
                    0b_11001,
                    0b_01110,
                    0b_00000),
            'Ø' => CreateCustomCharacter(
                    0b_01110,
                    0b_10011,
                    0b_10011,
                    0b_10101,
                    0b_10101,
                    0b_11001,
                    0b_01110,
                    0b_00000),
            'à' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000),
            'á' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000),
            'â' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_00001,
                    0b_01111,
                    0b_10001,
                    0b_01111,
                    0b_00000),
            'ä' => CreateCustomCharacter(
                     0b_01010,
                     0b_00000,
                     0b_01110,
                     0b_00001,
                     0b_01111,
                     0b_10001,
                     0b_01111,
                     0b_00000),
            'å' => CreateCustomCharacter(
                     0b_00100,
                     0b_01010,
                     0b_01110,
                     0b_00001,
                     0b_01111,
                     0b_10001,
                     0b_01111,
                     0b_00000),
            'ç' => CreateCustomCharacter(
                     0b_00000,
                     0b_00000,
                     0b_01110,
                     0b_10000,
                     0b_10000,
                     0b_01111,
                     0b_00010,
                     0b_00110),
            'é' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'è' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'ê' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'ë' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01110,
                    0b_10001,
                    0b_11111,
                    0b_10000,
                    0b_01111,
                    0b_00000),
            'ï' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'ì' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'í' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'î' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_01110,
                    0b_00000),
            'ñ' => CreateCustomCharacter(
                    0b_01010,
                    0b_00101,
                    0b_10000,
                    0b_11110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_00000),
            'ö' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ô' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ò' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ó' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_01110,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            'ú' => CreateCustomCharacter(
                    0b_00100,
                    0b_01000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            'ù' => CreateCustomCharacter(
                    0b_00100,
                    0b_00010,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            'û' => CreateCustomCharacter(
                    0b_00100,
                    0b_01010,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            'ü' => CreateCustomCharacter(
                    0b_01010,
                    0b_00000,
                    0b_10001,
                    0b_10001,
                    0b_10001,
                    0b_10011,
                    0b_01101,
                    0b_00000),
            '¡' => CreateCustomCharacter(
                    0b_00100,
                    0b_00000,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00100,
                    0b_00000),
            '¿' => CreateCustomCharacter(
                    0b_00100,
                    0b_00000,
                    0b_00100,
                    0b_01000,
                    0b_10000,
                    0b_10001,
                    0b_01110,
                    0b_00000),
            _ => throw new NotSupportedException("Character not supported"),
        };

        /// <summary>
        /// Creates the given letter from a pixel map for Rom Type A02 (7 or 8 Pixel high letters, bottom row filled)
        /// </summary>
        /// <param name="character">Character to create</param>
        /// <returns>An 8-Byte array of the pixel map for the created letter.</returns>
        /// <remarks>
        /// Currently requires the characters to be hardcoded here. Would be nice if we could generate the pixel maps from an existing font, such as Consolas
        /// </remarks>
        // TODO: Create letters for A02 map, but that one is a lot better equipped for european languages, so nothing to do for the currently supported languages
        protected virtual byte[]? CreateLetterA02(char character) => throw new NotSupportedException("Character not supported");

        /// <summary>
        /// Combines a set of bytes into a pixel map
        /// </summary>
        /// <example>
        /// Use as follows to create letter 'ü':
        /// <code>
        /// CreateCustomCharacter(
        ///            0b_01010,
        ///            0b_00000,
        ///            0b_10001,
        ///            0b_10001,
        ///            0b_10001,
        ///            0b_10011,
        ///            0b_01101,
        ///            0b_00000)
        /// </code>
        /// </example>
        protected byte[] CreateCustomCharacter(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7) =>
            new byte[] { byte0, byte1, byte2, byte3, byte4, byte5, byte6, byte7 };

        /// <summary>
        /// Convert a 8 bytes array with 5 lower bit character representation into a
        /// 5 bytes array with all bit character representation vertically ordered.
        /// </summary>
        /// <param name="font8">A span of bytes, must be 8 bytes length</param>
        /// <returns>A 5 bytes array containing the character</returns>
        public static byte[] ConvertFont8to5bytes(SpanByte font8)
        {
            if (font8.Length != 8)
            {
                throw new ArgumentException("Font size must be 8 bytes");
            }

            byte[] font5 = new byte[5];
            for (int i = 0; i < 5; i++)
            {
                byte font = 0x00;
                for (int j = 0; j < 8; j++)
                {
                    font |= (byte)(((font8[j] >> (4 - i)) & 0x01) << j);
                }

                font5[i] = font;
            }

            return font5;
        }

        /// <summary>
        /// Convert a 5 bytes array with 8 bits vertically encoded character representation into a
        /// 8 bytes array with the lower 5 bits.
        /// </summary>
        /// <param name="font5">A span of bytes, must be 5 bytes length</param>
        /// <returns>A 8 bytes array containing the character</returns>
        public static byte[] ConvertFont5to8bytes(SpanByte font5)
        {
            if (font5.Length != 5)
            {
                throw new ArgumentException("Font size must be 5 bytes");
            }

            byte[] font8 = new byte[8];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    font8[7 - j] = (byte)(font8[7 - j] << 1 | ((font5[i] >> (7 - j)) & 1));
                }
            }

            return font8;
        }
    }
}
