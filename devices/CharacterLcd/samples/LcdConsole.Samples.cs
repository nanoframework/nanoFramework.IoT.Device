// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Iot.Device.CharacterLcd;
using Iot.Device.Graphics;

namespace Iot.Device.CharacterLcd.Samples
{
    /// <summary>
    /// Sample code for using the <see cref="LcdConsole"/> class.
    /// </summary>
    public class LcdConsoleSamples
    {
        /// <summary>
        /// Write stuff to the display.
        /// </summary>
        /// <param name="lcd">The display driver</param>
        public static void WriteTest(ICharacterLcd lcd)
        {
            LcdConsole console = new LcdConsole(lcd, "A00", false);
            console.LineFeedMode = LineWrapMode.Truncate;
            Debug.WriteLine("Nowrap test:");
            console.Write("This is a long text that should not wrap and just extend beyond the display");
            console.WriteLine("This has CRLF\r\nin it and should \r\n wrap.");
            console.Write("This goes to the last line of the display");
            console.WriteLine("This isn't printed, because it's off the screen");
            Thread.Sleep(2000);
            Debug.WriteLine("Autoscroll test:");
            console.LineFeedMode = LineWrapMode.Wrap;
            console.WriteLine();
            console.WriteLine("Now the display should move up.");
            console.WriteLine("And more up.");
            for (int i = 0; i < 20; i++)
            {
                console.WriteLine($"This is line {i + 1}/{20}, but longer than the screen");
                Thread.Sleep(10);
            }

            console.LineFeedMode = LineWrapMode.Wrap;
            console.WriteLine("Same again, this time with full wrapping.");
            for (int i = 0; i < 20; i++)
            {
                console.Write($"This is string {i + 1}/{20} longer than the screen");
                Thread.Sleep(10);
            }

            Thread.Sleep(2000);
            Debug.WriteLine("Intelligent wrapping test");
            console.LineFeedMode = LineWrapMode.WordWrap;
            console.WriteLine("Now intelligent wrapping should wrap this long sentence at word borders and ommit spaces at the start of lines.");
            Debug.WriteLine("Not wrappable test");
            Thread.Sleep(2000);
            console.WriteLine("NowThisIsOneSentenceInOneWordThatCannotBeWrapped");
            Thread.Sleep(2000);
            Debug.WriteLine("Individual line test");
            console.Clear();
            console.LineFeedMode = LineWrapMode.Truncate;
            console.ReplaceLine(0, "This is all garbage that will be replaced");
            console.ReplaceLine(0, "Running clock test");
            int left = console.Size.Width;

            // Let the current time move trought the display on line 1
            int count = 0;
            while (count++ < 100)
            {
                DateTime now = DateTime.UtcNow;
                String time = String.Format("{0}", now.ToString());
                string printTime = time;
                if (left > 0)
                {
                    printTime = new string(' ', left) + time;
                }
                else if (left < 0)
                {
                    printTime = time.Substring(-left);
                }

                console.ReplaceLine(1, printTime);
                left--;
                // Each full minute, blink the display (but continue writing the time)
                if (now.Second == 0)
                {
                    console.BlinkDisplay(3);
                }

                Thread.Sleep(500);
                // Restart when the time string has left the display
                if (left < -time.Length)
                {
                    left = console.Size.Width;
                }
            }

            Thread.Sleep(2000);
            Debug.WriteLine("Culture Info Test");
            // In nanoFramework, there is no specific culture, while you'll do CultureInfo.CreateSpecificCulture("de-CH")
            // Here, you have to use the current culture, so using a string instead
            LcdCharacterEncoding encoding = LcdConsole.CreateEncoding("de", "A00", '?', 8);
            console.LoadEncoding(encoding);
            console.Clear();
            console.ScrollUpDelay = TimeSpan.FromSeconds(1);
            console.LineFeedMode = LineWrapMode.WordWrap;
            console.WriteLine(@"Die Ratten im Gemäuer, englischer Originaltitel ""The Rats in the Walls"" " +
                "ist eine phantastische Kurzgeschichte des amerikanischen Schriftstellers H. P. Lovecraft. Das etwa " +
                "8000 Wörter umfassende Werk wurde zwischen August und September 1923 verfasst und erschien erstmals " +
                "im März 1924 im Pulp-Magazin Weird Tales. Der Titel bezieht sich auf das Rascheln von Ratten in den " +
                "Gemäuern des Familienanwesens, das der Erzähler Delapore nach 300 Jahren auf den Ruinen des Stammsitzes " +
                "seiner Vorfahren neu errichtet hat. Im Verlauf der Erzählung führen die Ratten Delapore zur Entdeckung " +
                "des grausigen Geheimnisses der Gruft seines Anwesens und der finsteren Vergangenheit seiner Familie. " +
                "Nach Lovecraft entstand die Grundidee für die Geschichte, als eines späten Abends seine Tapete zu knistern begann. " +
                "(von https://de.wikipedia.org/wiki/Die_Ratten_im_Gem%C3%A4uer, CC-BY-SA 3.0)");
            console.WriteLine("From A00 default map: ");
            console.WriteLine("Code: [{|}]^_\\");
            console.WriteLine("Greek: Ωαβεπθμ");
            console.WriteLine("Others: @ñ¢");
            console.WriteLine("Math stuff: ∑÷×∞");

            console.WriteLine("German code page");
            console.WriteLine("Umlauts: äöüßÄÜÖ");
            console.WriteLine("Äußerst ölige, überflüssige Ölfässer im Großhandel von Ützhausen.");
            console.WriteLine("Currency: ¥€£$");
            // Same here
            encoding = LcdConsole.CreateEncoding("fr", "A00", '?', 8);
            console.LoadEncoding(encoding);
            console.Clear();
            console.WriteLine("Le français est une langue indo-européenne de la famille des langues romanes. " +
                "Le français s'est formé en France. Le français est déclaré langue officielle en France en 1539. " +
                "Après avoir été sous l'Ancien Régime la langue des cours royales et princières, " +
                "des tsars de Russie aux rois d'Espagne et d'Angleterre en passant par les princes de l'Allemagne, " +
                "il demeure une langue importante de la diplomatie internationale aux côtés de l'anglais. ");

            // And here
            encoding = LcdConsole.CreateEncoding("da", "A00", '?', 8);
            console.LoadEncoding(encoding);
            console.Clear();
            console.WriteLine("Dansk er et nordgermansk sprog af den østnordiske (kontinentale) gruppe, " +
                "der tales af ca. seks millioner mennesker. Det er stærkt påvirket af plattysk. Dansk tales " +
                "også i Sydslesvig (i Flensborg ca. 20 %) samt PÅ FÆRØER OG GRØNLAND.");

            Thread.Sleep(2000);
            Debug.WriteLine("Japanese test");
            encoding = LcdConsole.CreateEncoding("ja", "A00", '?', 8);
            console.LoadEncoding(encoding);
            console.WriteLine("What about some japanese?");
            console.WriteLine("イロハニホヘト");
            console.WriteLine("チリヌルヲ");
            console.WriteLine("ワカヨタレソ");
            console.WriteLine("ツネナラム");
            console.WriteLine("ウヰノオクヤマ");
            console.WriteLine("ケフコエテ");
            console.WriteLine("アサキユメミシ");
            console.WriteLine("ヱヒモセス");
            console.Clear();
            console.Write("Test finished");
            console.Dispose();
        }
    }
}
