// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Contains regression tests for the built-in French frontend.
    /// </summary>
    [TestClass]
    public class FrenchTtsFrontendTests
    {
        /// <summary>
        /// Verifies the French language definition and reusable construction path.
        /// </summary>
        [TestMethod]
        public void FrenchLanguageIsAvailable()
        {
            FrenchTtsLanguage language = FrenchTtsLanguage.Instance;
            TtsSynthesizer synthesizer = new TtsSynthesizer(language, new VoiceOptions());

            Assert.IsNotNull(language, "Shared French language");
            Assert.AreEqual("French", language.Name, "Language name");
            Assert.AreEqual(96, language.MaximumTextLength, "Maximum text length");
            Assert.AreEqual(language, synthesizer.Language, "Selected language");
            Assert.IsTrue(synthesizer.Speak("Bonjour le monde.").Length > 1000, "French PCM");
        }

        /// <summary>
        /// Verifies CLDR-derived integer expansion matches equivalent written French.
        /// </summary>
        [TestMethod]
        public void FrenchNumbersMatchWrittenCardinals()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();

            AssertPcmEqual(synthesizer.Speak("21 chats"), synthesizer.Speak("vingt-et-un chats"), "21");
            AssertPcmEqual(synthesizer.Speak("71 chats"), synthesizer.Speak("soixante-et-onze chats"), "71");
            AssertPcmEqual(synthesizer.Speak("80 chats"), synthesizer.Speak("quatre-vingts chats"), "80");
            AssertPcmEqual(synthesizer.Speak("81 chats"), synthesizer.Speak("quatre-vingt-un chats"), "81");
            AssertPcmEqual(synthesizer.Speak("100 chats"), synthesizer.Speak("cent chats"), "100");
            AssertPcmEqual(synthesizer.Speak("200 chats"), synthesizer.Speak("deux-cents chats"), "200");
            AssertPcmEqual(synthesizer.Speak("1000 chats"), synthesizer.Speak("mille chats"), "1000");
        }

        /// <summary>
        /// Verifies grouped silent finals and lateral ill words do not add extra phonemes.
        /// </summary>
        [TestMethod]
        public void FrenchSilentFinalGroupsAndLateralIllArePronouncedCorrectly()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();

            Assert.AreEqual(
                synthesizer.Speak("vin").Length,
                synthesizer.Speak("vingt").Length,
                "Silent gt in vingt");
            Assert.AreEqual(
                synthesizer.Speak("vin").Length,
                synthesizer.Speak("vingts").Length,
                "Silent gts in vingts");
            Assert.AreEqual(
                synthesizer.Speak("sans").Length,
                synthesizer.Speak("cents").Length,
                "Silent ts in cents");
            Assert.AreEqual(
                synthesizer.Speak("mil").Length,
                synthesizer.Speak("mille").Length,
                "Lateral ill in mille");
            Assert.AreEqual(
                synthesizer.Speak("vil").Length,
                synthesizer.Speak("ville").Length,
                "Lateral ill in ville");
            Assert.AreEqual(
                synthesizer.Speak("tranquile").Length,
                synthesizer.Speak("tranquille").Length,
                "Lateral ill in tranquille");
        }

        /// <summary>
        /// Verifies accented letters and important French grapheme groups produce speech.
        /// </summary>
        [TestMethod]
        public void FrenchAccentsAndGraphemesProduceSpeech()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();
            byte[] pcm = synthesizer.Speak(
                "Ça coûte très cher: cœur, fille, montagne, chat, eau, œuf, oui et huit.");

            Assert.IsTrue(pcm.Length > 5000, "Accented grapheme PCM");
        }

        /// <summary>
        /// Verifies the four French nasal-vowel spelling groups remain acoustically distinct.
        /// </summary>
        [TestMethod]
        public void FrenchNasalVowelsAreDistinct()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();
            int an = ComputeHash(synthesizer.Speak("sans"));
            int inValue = ComputeHash(synthesizer.Speak("pain"));
            int on = ComputeHash(synthesizer.Speak("bon"));
            int un = ComputeHash(synthesizer.Speak("un"));

            Assert.IsTrue(an != inValue, "AN and IN");
            Assert.IsTrue(an != on, "AN and ON");
            Assert.IsTrue(inValue != un, "IN and UN");
            Assert.IsTrue(on != un, "ON and UN");
        }

        /// <summary>
        /// Verifies reused French frontend and renderer state reset between utterances.
        /// </summary>
        [TestMethod]
        public void FrenchSynthesisIsDeterministicAfterReuse()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();
            byte[] first = synthesizer.Speak("Bonjour, êtes-vous prêt?");
            synthesizer.Speak("Une phrase différente avec plusieurs phonèmes.");
            byte[] second = synthesizer.Speak("Bonjour, êtes-vous prêt?");

            AssertPcmEqual(first, second, "Reused French workspace");
        }

        /// <summary>
        /// Verifies buffered and streamed French synthesis use identical PCM rendering.
        /// </summary>
        [TestMethod]
        public void FrenchStreamingMatchesBufferedOutput()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();
            byte[] expected = synthesizer.Speak("Le français fonctionne en diffusion continue.");
            ComparingSink sink = new ComparingSink(expected);
            int count = synthesizer.Speak("Le français fonctionne en diffusion continue.", sink);

            Assert.AreEqual(expected.Length, count, "Streaming byte count");
            Assert.AreEqual(expected.Length, sink.Position, "Sink byte count");
            Assert.IsTrue(sink.Matches, "French streamed PCM");
        }

        /// <summary>
        /// Verifies French accentual groups use distinct continuation and terminal timing.
        /// </summary>
        [TestMethod]
        public void FrenchProsodyUsesAccentualGroups()
        {
            TtsSynthesizer synthesizer = CreateSynthesizer();
            byte[] plain = synthesizer.Speak("Demain nous partirons.");
            byte[] grouped = synthesizer.Speak("Demain, nous partirons.");
            byte[] question = synthesizer.Speak("Demain nous partirons?");

            Assert.IsTrue(grouped.Length > plain.Length, "Comma group pause");
            Assert.IsTrue(question.Length > plain.Length, "Question boundary");
            Assert.IsTrue(ComputeHash(question) != ComputeHash(plain), "Question contour");
        }

        /// <summary>
        /// Verifies French number expansion can be split within the fixed phoneme capacity.
        /// </summary>
        [TestMethod]
        public void FrenchSegmenterSplitsExpandedNumbers()
        {
            FrenchTtsLanguage language = FrenchTtsLanguage.Instance;
            TtsSegmenter segmenter = new TtsSegmenter(language);
            TtsSynthesizer synthesizer = new TtsSynthesizer(language, new VoiceOptions());
            string[] segments = segmenter.Split(new string('8', language.MaximumTextLength));

            Assert.IsTrue(segments.Length > 1, "Expanded French segment count");
            for (int i = 0; i < segments.Length; i++)
            {
                Assert.IsTrue(synthesizer.Speak(segments[i]).Length > 0, "French segment synthesis");
            }
        }

        private static TtsSynthesizer CreateSynthesizer()
        {
            return new TtsSynthesizer(FrenchTtsLanguage.Instance, new VoiceOptions());
        }

        private static int ComputeHash(byte[] data)
        {
            int hash = 17;
            for (int i = 0; i < data.Length; i++)
            {
                hash = (hash * 31) + data[i];
            }

            return hash;
        }

        private static void AssertPcmEqual(byte[] expected, byte[] actual, string message)
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " length");
            Assert.AreEqual(ComputeHash(expected), ComputeHash(actual), message + " hash");
        }
    }
}
