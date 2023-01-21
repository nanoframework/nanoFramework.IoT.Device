// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Drawing
{
    /// <summary>
    /// Represents an ARGB (alpha, red, green, blue) color.
    /// </summary>
    public struct Color
    {
        private readonly uint _color;

        internal Color(uint color)
        {
            _color = color;
        }

        #region Known colors

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF66CDAA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumAquamarine { get => new Color(0xFF66CDAA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF0000CD.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumBlue { get => new Color(0xFF0000CD); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFBA55D3.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumOrchid { get => new Color(0xFFBA55D3); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF9370DB.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumPurple { get => new Color(0xFF9370DB); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF3CB371.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumSeaGreen { get => new Color(0xFF3CB371); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF7B68EE.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumSlateBlue { get => new Color(0xFF7B68EE); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00FA9A.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumSpringGreen { get => new Color(0xFF00FA9A); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF800000.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Maroon { get => new Color(0xFF800000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF48D1CC.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumTurquoise { get => new Color(0xFF48D1CC); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF191970.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MidnightBlue { get => new Color(0xFF191970); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF5FFFA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MintCream { get => new Color(0xFFF5FFFA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFE4E1.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MistyRose { get => new Color(0xFFFFE4E1); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFE4B5.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Moccasin { get => new Color(0xFFFFE4B5); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFDEAD.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color NavajoWhite { get => new Color(0xFFFFDEAD); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF000080.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Navy { get => new Color(0xFF000080); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFDF5E6.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color OldLace { get => new Color(0xFFFDF5E6); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFC71585.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color MediumVioletRed { get => new Color(0xFFC71585); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF00FF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Magenta { get => new Color(0xFFFF00FF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFAF0E6.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Linen { get => new Color(0xFFFAF0E6); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF32CD32.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LimeGreen { get => new Color(0xFF32CD32); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFF0F5.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LavenderBlush { get => new Color(0xFFFFF0F5); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF7CFC00.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LawnGreen { get => new Color(0xFF7CFC00); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFACD.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LemonChiffon { get => new Color(0xFFFFFACD); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFADD8E6.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightBlue { get => new Color(0xFFADD8E6); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF08080.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightCoral { get => new Color(0xFFF08080); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFE0FFFF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightCyan { get => new Color(0xFFE0FFFF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFAFAD2.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightGoldenrodYellow { get => new Color(0xFFFAFAD2); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFD3D3D3.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightGray { get => new Color(0xFFD3D3D3); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF90EE90.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightGreen { get => new Color(0xFF90EE90); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFB6C1.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightPink { get => new Color(0xFFFFB6C1); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFA07A.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightSalmon { get => new Color(0xFFFFA07A); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF20B2AA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightSeaGreen { get => new Color(0xFF20B2AA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF87CEFA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightSkyBlue { get => new Color(0xFF87CEFA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF778899.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightSlateGray { get => new Color(0xFF778899); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFB0C4DE.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightSteelBlue { get => new Color(0xFFB0C4DE); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFFE0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color LightYellow { get => new Color(0xFFFFFFE0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00FF00.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Lime { get => new Color(0xFF00FF00); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF808000.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Olive { get => new Color(0xFF808000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF6B8E23.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color OliveDrab { get => new Color(0xFF6B8E23); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFA500.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Orange { get => new Color(0xFFFFA500); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF4500.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color OrangeRed { get => new Color(0xFFFF4500); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFC0C0C0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Silver { get => new Color(0xFFC0C0C0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF87CEEB.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SkyBlue { get => new Color(0xFF87CEEB); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF6A5ACD.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SlateBlue { get => new Color(0xFF6A5ACD); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF708090.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SlateGray { get => new Color(0xFF708090); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFAFA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Snow { get => new Color(0xFFFFFAFA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00FF7F.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SpringGreen { get => new Color(0xFF00FF7F); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF4682B4.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SteelBlue { get => new Color(0xFF4682B4); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFD2B48C.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Tan { get => new Color(0xFFD2B48C); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF008080.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Teal { get => new Color(0xFF008080); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFD8BFD8.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Thistle { get => new Color(0xFFD8BFD8); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF6347.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Tomato { get => new Color(0xFFFF6347); }

        /// <summary>
        /// Gets a system-defined color.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Transparent { get => new Color(0x00000000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF40E0D0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Turquoise { get => new Color(0xFF40E0D0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFEE82EE.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Violet { get => new Color(0xFFEE82EE); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF5DEB3.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Wheat { get => new Color(0xFFF5DEB3); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFFFF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color White { get => new Color(0xFFFFFFFF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF5F5F5.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color WhiteSmoke { get => new Color(0xFFF5F5F5); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFA0522D.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Sienna { get => new Color(0xFFA0522D); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFE6E6FA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Lavender { get => new Color(0xFFE6E6FA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFF5EE.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SeaShell { get => new Color(0xFFFFF5EE); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF4A460.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SandyBrown { get => new Color(0xFFF4A460); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDA70D6.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Orchid { get => new Color(0xFFDA70D6); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFEEE8AA.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PaleGoldenrod { get => new Color(0xFFEEE8AA); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF98FB98.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PaleGreen { get => new Color(0xFF98FB98); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFAFEEEE.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PaleTurquoise { get => new Color(0xFFAFEEEE); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDB7093.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PaleVioletRed { get => new Color(0xFFDB7093); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFEFD5.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PapayaWhip { get => new Color(0xFFFFEFD5); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFDAB9.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PeachPuff { get => new Color(0xFFFFDAB9); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFCD853F.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Peru { get => new Color(0xFFCD853F); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFC0CB.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Pink { get => new Color(0xFFFFC0CB); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDDA0DD.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Plum { get => new Color(0xFFDDA0DD); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFB0E0E6.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color PowderBlue { get => new Color(0xFFB0E0E6); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF800080.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Purple { get => new Color(0xFF800080); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF0000.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Red { get => new Color(0xFFFF0000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFBC8F8F.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color RosyBrown { get => new Color(0xFFBC8F8F); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF4169E1.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color RoyalBlue { get => new Color(0xFF4169E1); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF8B4513.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SaddleBrown { get => new Color(0xFF8B4513); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFA8072.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Salmon { get => new Color(0xFFFA8072); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF2E8B57.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color SeaGreen { get => new Color(0xFF2E8B57); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFF00.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Yellow { get => new Color(0xFFFFFF00); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF0E68C.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Khaki { get => new Color(0xFFF0E68C); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00FFFF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Cyan { get => new Color(0xFF00FFFF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF8B008B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkMagenta { get => new Color(0xFF8B008B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFBDB76B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkKhaki { get => new Color(0xFFBDB76B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF006400.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkGreen { get => new Color(0xFF006400); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFA9A9A9.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkGray { get => new Color(0xFFA9A9A9); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFB8860B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkGoldenrod { get => new Color(0xFFB8860B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF008B8B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkCyan { get => new Color(0xFF008B8B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00008B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkBlue { get => new Color(0xFF00008B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFFF0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Ivory { get => new Color(0xFFFFFFF0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDC143C.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Crimson { get => new Color(0xFFDC143C); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFF8DC.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Cornsilk { get => new Color(0xFFFFF8DC); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF6495ED.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color CornflowerBlue { get => new Color(0xFF6495ED); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF7F50.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Coral { get => new Color(0xFFFF7F50); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFD2691E.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Chocolate { get => new Color(0xFFD2691E); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF556B2F.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkOliveGreen { get => new Color(0xFF556B2F); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF7FFF00.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Chartreuse { get => new Color(0xFF7FFF00); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDEB887.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color BurlyWood { get => new Color(0xFFDEB887); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFA52A2A.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Brown { get => new Color(0xFFA52A2A); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF8A2BE2.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color BlueViolet { get => new Color(0xFF8A2BE2); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF0000FF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Blue { get => new Color(0xFF0000FF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFEBCD.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color BlanchedAlmond { get => new Color(0xFFFFEBCD); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF000000.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Black { get => new Color(0xFF000000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFE4C4.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Bisque { get => new Color(0xFFFFE4C4); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF5F5DC.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Beige { get => new Color(0xFFF5F5DC); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF0FFFF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Azure { get => new Color(0xFFF0FFFF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF7FFFD4.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Aquamarine { get => new Color(0xFF7FFFD4); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00FFFF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Aqua { get => new Color(0xFF00FFFF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFAEBD7.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color AntiqueWhite { get => new Color(0xFFFAEBD7); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF0F8FF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color AliceBlue { get => new Color(0xFFF0F8FF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF5F9EA0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color CadetBlue { get => new Color(0xFF5F9EA0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF8C00.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkOrange { get => new Color(0xFFFF8C00); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF9ACD32.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color YellowGreen { get => new Color(0xFF9ACD32); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF8B0000.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkRed { get => new Color(0xFF8B0000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF4B0082.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Indigo { get => new Color(0xFF4B0082); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFCD5C5C.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color IndianRed { get => new Color(0xFFCD5C5C); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF9932CC.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkOrchid { get => new Color(0xFF9932CC); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF0FFF0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Honeydew { get => new Color(0xFFF0FFF0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFADFF2F.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color GreenYellow { get => new Color(0xFFADFF2F); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF008000.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Green { get => new Color(0xFF008000); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF808080.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color structure representing a system-defined color.
        /// </returns>
        public static Color Gray { get => new Color(0xFF808080); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDAA520.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Goldenrod { get => new Color(0xFFDAA520); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFD700.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Gold { get => new Color(0xFFFFD700); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFF8F8FF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color GhostWhite { get => new Color(0xFFF8F8FF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFDCDCDC.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Gainsboro { get => new Color(0xFFDCDCDC); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF00FF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Fuchsia { get => new Color(0xFFFF00FF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF228B22.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color ForestGreen { get => new Color(0xFF228B22); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF69B4.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color HotPink { get => new Color(0xFFFF69B4); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFB22222.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color Firebrick { get => new Color(0xFFB22222); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFFFAF0.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color FloralWhite { get => new Color(0xFFFFFAF0); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF1E90FF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DodgerBlue { get => new Color(0xFF1E90FF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF696969.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DimGray { get => new Color(0xFF696969); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00BFFF.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DeepSkyBlue { get => new Color(0xFF00BFFF); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFFF1493.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DeepPink { get => new Color(0xFFFF1493); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF9400D3.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkViolet { get => new Color(0xFF9400D3); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF00CED1.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkTurquoise { get => new Color(0xFF00CED1); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF2F4F4F.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkSlateGray { get => new Color(0xFF2F4F4F); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF483D8B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkSlateBlue { get => new Color(0xFF483D8B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FF8FBC8B.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkSeaGreen { get => new Color(0xFF8FBC8B); }

        /// <summary>
        /// Gets a system-defined color that has an ARGB value of #FFE9967A.
        /// </summary>
        /// <returns>
        /// A System.Drawing.Color representing a system-defined color.
        /// </returns>
        public static Color DarkSalmon { get => new Color(0xFFE9967A); }

        #endregion Known colors

        /// <summary>
        /// Gets the alpha component value of this System.Drawing.Color structure.
        /// </summary>
        public byte A { get => (byte)(_color >> 24); }

        /// <summary>
        /// Gets the blue component value of this System.Drawing.Color structure.
        /// </summary>
        public byte B { get => (byte)_color; }

        /// <summary>
        /// Gets the red component value of this System.Drawing.Color structure.
        /// </summary>
        public byte R { get => (byte)(_color >> 16); }

        /// <summary>
        /// Gets the green component value of this System.Drawing.Color structure.
        /// </summary>
        public byte G { get => (byte)(_color >> 8); }

        /// <summary>
        /// Creates a System.Drawing.Color structure from the specified 8-bit color values
        /// (red, green, and blue). The alpha value is implicitly 255 (fully opaque). Although
        /// this method allows a 32-bit value to be passed for each color component, the
        /// value of each component is limited to 8 bits.
        /// </summary>
        /// <param name="r">The red component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <param name="g">The green component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <param name="b">The blue component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <returns>The System.Drawing.Color structure that this method creates.</returns>
        public static Color FromArgb(int r, int g, int b)
        {
            return new Color((uint)((0xFF << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | (b & 0xFF)));
        }

        /// <summary>
        /// Creates a System.Drawing.Color structure from the specified 8-bit color values
        /// (red, green, and blue). The alpha value is implicitly 255 (fully opaque). Although
        /// this method allows a 32-bit value to be passed for each color component, the
        /// value of each component is limited to 8 bits.
        /// </summary>
        /// <param name="a">The alpha component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <param name="r">The red component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <param name="g">The green component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <param name="b">The blue component value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <returns>The System.Drawing.Color structure that this method creates.</returns>
        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color((uint)(((a & 0xFF) << 24) | ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | (b & 0xFF)));
        }

        /// <summary>
        /// Creates a System.Drawing.Color structure from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value.</param>
        /// <returns>The System.Drawing.Color structure that this method creates.</returns>
        public static Color FromArgb(int argb) => new Color((uint)argb);

        /// <summary>
        /// Creates a System.Drawing.Color structure from the specified System.Drawing.Color
        /// structure, but with the new specified alpha value. Although this method allows
        /// a 32-bit value to be passed for the alpha value, the value is limited to 8 bits.
        /// </summary>
        /// <param name="alpha">The alpha value for the new System.Drawing.Color. Valid values are 0 through 255.</param>
        /// <param name="baseColor">The System.Drawing.Color from which to create the new System.Drawing.Color.</param>
        /// <returns>The System.Drawing.Color that this method creates.</returns>
        public static Color FromArgb(int alpha, Color baseColor) => new Color((uint)((alpha << 24) | (0x00FFFFFF & baseColor._color)));

        /// <summary>
        /// Tests whether two specified System.Drawing.Color structures are equivalent.
        /// </summary>
        /// <param name="left">The System.Drawing.Color that is to the left of the equality operator.</param>
        /// <param name="right">The System.Drawing.Color that is to the right of the equality operator.</param>
        /// <returns>true if the two System.Drawing.Color structures are equal; otherwise, false.</returns>
        public static bool operator ==(Color left, Color right) => left._color == right._color;

        /// <summary>
        /// Tests whether two specified System.Drawing.Color structures are different.
        /// </summary>
        /// <param name="left">The System.Drawing.Color that is to the left of the inequality operator.</param>
        /// <param name="right">The System.Drawing.Color that is to the right of the inequality operator.</param>
        /// <returns>true if the two System.Drawing.Color structures are different; otherwise, false.</returns>
        public static bool operator !=(Color left, Color right) => left._color != right._color;

        /// <summary>
        /// Internal Helper function for ParseHex.
        /// </summary>
        /// <param name="intChar">Value to convert.</param>
        /// <returns>Int value.</returns>
        public static int ParseHexChar(char intChar)
        {
            if (intChar >= '0' && intChar <= '9')
            {
                return intChar - '0';
            }

            if (intChar >= 'a' && intChar <= 'f')
            {
                return intChar - 'a' + 10;
            }

            if (intChar >= 'A' && intChar <= 'F')
            {
                return intChar - 'A' + 10;
            }

            throw new FormatException($"Illegal token. {intChar} can't be converted");
        }

        /// <summary>
        /// Convert String into an Color struct.
        /// </summary>
        /// <param name="hexString">Color String. Allowed formats are #AARRGGBB #RRGGBB #ARGB #RGB.</param>
        /// <returns>Returns an Color struct otherwise throws an exception.</returns>
        /// <exception>ArgumentException or FormatException.</exception>
        public static Color ParseHex(string hexString)
        {
            int r, g, b, a = 255;

            if (hexString[0] != '#')
            {
                throw new ArgumentException("Leading # is missing.");
            }

            switch (hexString.Length)
            {
                case 9: // #AARRGGBB
                    a = ParseHexChar(hexString[1]) << 4 | ParseHexChar(hexString[2]);
                    r = ParseHexChar(hexString[3]) << 4 | ParseHexChar(hexString[4]);
                    g = ParseHexChar(hexString[5]) << 4 | ParseHexChar(hexString[6]);
                    b = ParseHexChar(hexString[7]) << 4 | ParseHexChar(hexString[8]);
                    break;

                case 7: // #RRGGBB
                    r = ParseHexChar(hexString[1]) << 4 | ParseHexChar(hexString[2]);
                    g = ParseHexChar(hexString[3]) << 4 | ParseHexChar(hexString[4]);
                    b = ParseHexChar(hexString[5]) << 4 | ParseHexChar(hexString[6]);
                    break;

                case 5: // #ARGB
                    a = ParseHexChar(hexString[1]);
                    a = a << 4 | a;
                    r = ParseHexChar(hexString[2]);
                    r = r << 4 | r;
                    g = ParseHexChar(hexString[3]);
                    g = g << 4 | g;
                    b = ParseHexChar(hexString[4]);
                    b = b << 4 | b;
                    break;

                case 4: // #RGB
                    r = ParseHexChar(hexString[1]);
                    r = r << 4 | r;
                    g = ParseHexChar(hexString[2]);
                    g = g << 4 | g;
                    b = ParseHexChar(hexString[3]);
                    b = b << 4 | b;
                    break;

                default:
                    throw new ArgumentException($"Length of {hexString.Length} not match any know format");
            }

            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to other; otherwise, false.</returns>
        public bool Equals(Color other) => _color == other._color;

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) lightness value for this System.Drawing.Color structure.
        /// </summary>
        /// <returns>The lightness of this System.Drawing.Color. The lightness ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
        public float GetBrightness()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;
            float max = r > b ? r : b;
            max = max > g ? max : g;
            return max;
        }

        /// <summary>
        /// Returns a hash code for this System.Drawing.Color structure.
        /// </summary>
        /// <returns>An integer value that specifies the hash code for this System.Drawing.Color.</returns>
        public override int GetHashCode() => (int)_color;

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) hue value, in degrees, for this System.Drawing.Color structure.
        /// </summary>
        /// <returns>The hue, in degrees, of this System.Drawing.Color. The hue is measured in degrees,
        /// ranging from 0.0 through 360.0, in HSL color space.</returns>
        public float GetHue()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;
            float max = r > b ? r : b;
            max = max > g ? max : g;
            float min = r > b ? b : r;
            min = min > g ? g : min;

            float h = 0.0f;
            if (max == r && g >= b)
            {
                h = (60 * (g - b)) / (max - min);
            }
            else if (max == r && g < b)
            {
                h = ((60 * (g - b)) / (max - min)) + 360;
            }
            else if (max == g)
            {
                h = ((60 * (b - r)) / (max - min)) + 120;
            }
            else if (max == b)
            {
                h = ((60 * (r - g)) / (max - min)) + 240;
            }

            return h;
        }

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) saturation value for this System.Drawing.Color structure.
        /// </summary>
        /// <returns>The saturation of this System.Drawing.Color. The saturation ranges from 0.0 through
        /// 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
        public float GetSaturation()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;
            float max = r > b ? r : b;
            max = max > g ? max : g;
            float min = r > b ? b : r;
            min = min > g ? g : min;
            return (max == 0) ? 0.0f : (1.0f - (min / max));
        }

        /// <summary>
        /// Gets the 32-bit ARGB value of this System.Drawing.Color structure.
        /// </summary>
        /// <returns>The 32-bit ARGB value of this System.Drawing.Color.</returns>
        public int ToArgb() => (int)_color;
    }
}
