// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Verifies allocation-conscious continuity between adjoining voiced phonemes.
    /// </summary>
    [TestClass]
    public class VoicingContinuityTests
    {
        /// <summary>
        /// Verifies the existing constructor enables continuity and an explicit legacy mode remains available.
        /// </summary>
        [TestMethod]
        public void ContinuityIsDefaultAndLegacyOptionsRetainTheirControls()
        {
            VoiceOptions current = new VoiceOptions(-1, 95, 98, 140, 80, 150);
            VoiceOptions legacy = new VoiceOptions(false, -1, 95, 98, 140, 80, 150);

            Assert.IsTrue(current.ContinuousVoicing, "Default continuity");
            Assert.IsFalse(legacy.ContinuousVoicing, "Legacy mode");
            Assert.AreEqual(current.PitchShift, legacy.PitchShift, "Pitch");
            Assert.AreEqual(current.SpeedPercent, legacy.SpeedPercent, "Speed");
            Assert.AreEqual(current.FormantScalePercent, legacy.FormantScalePercent, "Formants");
            Assert.AreEqual(current.IntonationPercent, legacy.IntonationPercent, "Intonation");
            Assert.AreEqual(current.FricativeNoisePercent, legacy.FricativeNoisePercent, "Noise");
            Assert.AreEqual(current.TransitionSmoothingPercent, legacy.TransitionSmoothingPercent, "Smoothing");
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(false, speedPercent: 161); },
                "Legacy overload validates controls");
        }

        /// <summary>
        /// Verifies phase and envelopes continue exactly through repeated identical voiced phonemes.
        /// </summary>
        [TestMethod]
        public void JoinedVoicingMatchesASustainedToneAwayFromOuterFades()
        {
            TtsSynthesizer current = new TtsSynthesizer(new TestLanguage(), new VoiceOptions());
            TtsSynthesizer legacy = new TtsSynthesizer(new TestLanguage(), new VoiceOptions(false));
            byte[] joined = current.Speak("llll");
            byte[] sustained = current.Speak("c");
            byte[] original = legacy.Speak("llll");

            Assert.AreEqual(6400, joined.Length, "Joined duration");
            Assert.AreEqual(sustained.Length, joined.Length, "Sustained duration");
            Assert.AreEqual(original.Length, joined.Length, "Legacy duration");
            AssertRangeEqual(sustained, 900, joined, 900, 4100);
            long originalJoin = Power(original, 1568, 64);
            long currentJoin = Power(joined, 1568, 64);
            long steady = Power(joined, 1100, 64);
            Assert.IsTrue(currentJoin > originalJoin * 4, "No artificial voiced-join silence");
            Assert.IsTrue(currentJoin > steady / 2, "Join retains voiced energy");
            Assert.AreEqual((byte)128, joined[joined.Length - 1], "Voiced-run tail still fades");
        }

        /// <summary>
        /// Verifies legacy rendering still restarts the same isolated tone at every phoneme.
        /// </summary>
        [TestMethod]
        public void LegacyRepeatedPhonemesRemainByteIdentical()
        {
            TtsSynthesizer synthesizer = new TtsSynthesizer(new TestLanguage(), new VoiceOptions(false));
            byte[] single = synthesizer.Speak("x");
            byte[] pair = synthesizer.Speak("xx");

            Assert.AreEqual(single.Length * 2, pair.Length, "Legacy pair duration");
            AssertRangeEqual(single, 0, pair, 0, single.Length);
            AssertRangeEqual(single, 0, pair, single.Length, single.Length);
        }

        /// <summary>
        /// Verifies consonants and pauses break oscillator and formant continuity.
        /// </summary>
        [TestMethod]
        public void ContinuityDoesNotCrossConsonantsOrPauses()
        {
            TtsSynthesizer current = new TtsSynthesizer(new TestLanguage(), new VoiceOptions());
            byte[] first = current.Speak("x");
            byte[] last = current.Speak("f");
            char[] boundaries = { ' ', 'h', 'v', 'p', 'b' };
            for (int i = 0; i < boundaries.Length; i++)
            {
                byte[] separator = current.Speak(boundaries[i].ToString());
                byte[] combined = current.Speak("x" + boundaries[i] + "f");
                Assert.AreEqual(first.Length + separator.Length + last.Length, combined.Length, "Boundary duration");
                AssertRangeEqual(first, 0, combined, 0, first.Length);
                AssertRangeEqual(separator, 0, combined, first.Length, separator.Length);
                AssertRangeEqual(last, 0, combined, first.Length + separator.Length, last.Length);
            }
        }

        /// <summary>
        /// Verifies non-voiced excitation paths do not change between renderer modes.
        /// </summary>
        [TestMethod]
        public void ConsonantOnlySequencesRemainByteIdentical()
        {
            byte[] current = new TtsSynthesizer(new TestLanguage(), new VoiceOptions()).Speak("hvpb");
            byte[] legacy = new TtsSynthesizer(new TestLanguage(), new VoiceOptions(false)).Speak("hvpb");

            Assert.AreEqual(current.Length, legacy.Length, "Consonant duration");
            AssertRangeEqual(legacy, 0, current, 0, legacy.Length);
        }

        /// <summary>
        /// Verifies diphthong fallback formants remain consistent at a voiced join.
        /// </summary>
        [TestMethod]
        public void JoinedGlidesUseIndependentFormantFallbacks()
        {
            TtsSynthesizer synthesizer = new TtsSynthesizer(new TestLanguage(), new VoiceOptions());
            byte[] implicitFallbacks = synthesizer.Speak("fx");
            byte[] explicitFallbacks = synthesizer.Speak("ex");

            Assert.AreEqual(implicitFallbacks.Length, explicitFallbacks.Length, "Glide duration");
            AssertRangeEqual(implicitFallbacks, 0, explicitFallbacks, 0, implicitFallbacks.Length);
        }

        /// <summary>
        /// Verifies tiny phonemes, partial control blocks, and Nyquist joins retain bounded timing and fades.
        /// </summary>
        [TestMethod]
        public void JoinedVoicingHandlesControlAndFormantLimits()
        {
            int[] speeds = { 60, 100, 150, 160 };
            int[] smoothing = { 0, 100, 200 };
            for (int i = 0; i < speeds.Length; i++)
            {
                for (int j = 0; j < smoothing.Length; j++)
                {
                    VoiceOptions options = new VoiceOptions(
                        speedPercent: speeds[i],
                        formantScalePercent: 120,
                        transitionSmoothingPercent: smoothing[j]);
                    TtsSynthesizer synthesizer = new TtsSynthesizer(new TestLanguage(), options);
                    byte[] pcm = synthesizer.Speak("ntfn");
                    byte[] tiny = synthesizer.Speak("tttt");
                    int ordinaryDuration = ((50 * 8000) + (speeds[i] * 5)) / (speeds[i] * 10);
                    int tinyDuration = (8000 + (speeds[i] * 5)) / (speeds[i] * 10);
                    Assert.AreEqual((ordinaryDuration * 3) + tinyDuration, pcm.Length, "Partial block timing");
                    Assert.AreEqual(tinyDuration * 4, tiny.Length, "Tiny phoneme timing");
                    Assert.AreEqual((byte)128, pcm[pcm.Length - 1], "Nyquist run tail");
                    Assert.AreEqual((byte)128, tiny[tiny.Length - 1], "Tiny run tail");
                }
            }
        }

        /// <summary>
        /// Verifies the same bounded streaming buffer carries continuous voicing across writes.
        /// </summary>
        [TestMethod]
        public void JoinedStreamingReusesItsBufferAndRecoversAfterSinkFailure()
        {
            TtsSynthesizer synthesizer = new TtsSynthesizer(new TestLanguage(), new VoiceOptions());
            byte[] expected = synthesizer.Speak("llll");
            ComparingSink comparison = new ComparingSink(expected);
            synthesizer.Speak("llll", comparison);
            Assert.IsTrue(comparison.Matches, "Continuous streamed bytes");
            Assert.AreEqual(expected.Length, comparison.Position, "Continuous streamed length");

            BlockCountingSink blocks = new BlockCountingSink();
            synthesizer.Speak("llll", blocks);
            synthesizer.Speak("llll", blocks);
            Assert.IsFalse(blocks.ChangedBuffer, "One buffer is reused across blocks and renders");
            Assert.AreEqual(2048, blocks.MaximumBlockSize, "Original streaming buffer size");
            Assert.AreEqual(expected.Length * 2, blocks.ByteCount, "Reused buffer sample count");

            Assert.ThrowsException(
                typeof(InvalidOperationException),
                delegate { synthesizer.Speak("llll", new ThrowingSink()); },
                "Sink errors remain explicit");
            byte[] recovered = synthesizer.Speak("llll");
            AssertRangeEqual(expected, 0, recovered, 0, expected.Length);
        }

        /// <summary>
        /// Compares both modes' sample budgets and reports simulator or device synthesis timings.
        /// </summary>
        [TestMethod]
        public void BothLanguagesRetainSampleBudgetAndReportStreamingTimings()
        {
            ITtsLanguage[] languages = { EnglishTtsLanguage.Instance, FrenchTtsLanguage.Instance };
            string[] phrases = { "Hello. Let us play a game.", "Bonjour. On joue ensemble." };
            for (int language = 0; language < languages.Length; language++)
            {
                TtsSynthesizer current = new TtsSynthesizer(languages[language], new VoiceOptions());
                TtsSynthesizer legacy = new TtsSynthesizer(languages[language], new VoiceOptions(false));
                BlockCountingSink currentSink = new BlockCountingSink();
                BlockCountingSink legacySink = new BlockCountingSink();
                current.Speak(phrases[language], currentSink);
                legacy.Speak(phrases[language], legacySink);
                long currentTicks = 0;
                long legacyTicks = 0;
                for (int run = 0; run < 8; run++)
                {
                    if ((run & 1) == 0)
                    {
                        currentTicks += Measure(current, phrases[language], currentSink);
                        legacyTicks += Measure(legacy, phrases[language], legacySink);
                    }
                    else
                    {
                        legacyTicks += Measure(legacy, phrases[language], legacySink);
                        currentTicks += Measure(current, phrases[language], currentSink);
                    }
                }

                Assert.AreEqual(legacySink.ByteCount, currentSink.ByteCount, "Language sample budget");
                Assert.AreEqual(legacySink.WriteCount, currentSink.WriteCount, "Language block budget");
                Assert.IsFalse(currentSink.ChangedBuffer, "Language buffer reuse");
                OutputHelper.WriteLine(
                    "[tts-continuity] language=" + languages[language].Name
                    + ", runs=8, continuous-ticks=" + currentTicks.ToString()
                    + ", legacy-ticks=" + legacyTicks.ToString());
            }
        }

        private static long Measure(TtsSynthesizer synthesizer, string text, IPcmSink sink)
        {
            long started = DateTime.UtcNow.Ticks;
            synthesizer.Speak(text, sink);
            return DateTime.UtcNow.Ticks - started;
        }

        private static long Power(byte[] pcm, int offset, int count)
        {
            long power = 0;
            for (int i = offset; i < offset + count; i++)
            {
                int value = pcm[i] - 128;
                power += value * value;
            }

            return power;
        }

        private static void AssertRangeEqual(byte[] expected, int expectedOffset, byte[] actual, int actualOffset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (expected[expectedOffset + i] != actual[actualOffset + i])
                {
                    Assert.IsTrue(false, "PCM differs at sample " + i.ToString());
                }
            }
        }
    }
}
