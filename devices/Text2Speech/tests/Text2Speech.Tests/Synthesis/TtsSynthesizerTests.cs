// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Contains regression tests for <see cref="TtsSynthesizer"/>.
    /// </summary>
    [TestClass]
    public class TtsSynthesizerTests
    {
        /// <summary>
        /// Verifies the compact output format constants.
        /// </summary>
        [TestMethod]
        public void AudioFormatConstantsAreCompactWavFormat()
        {
            Assert.AreEqual(8000, TtsSynthesizer.SampleRate, "Sample rate");
            Assert.AreEqual(8, TtsSynthesizer.BitsPerSample, "Bit depth");
            Assert.AreEqual(1, TtsSynthesizer.Channels, "Channel count");
            Assert.AreEqual(96, EnglishTtsLanguage.DefaultMaximumTextLength, "Maximum text length");
        }

        /// <summary>
        /// Verifies explicit English construction does not depend on eager static initialization.
        /// </summary>
        [TestMethod]
        public void ExplicitEnglishLanguageIsAvailable()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            TtsSegmenter segmenter = new TtsSegmenter(EnglishTtsLanguage.Instance);

            Assert.IsNotNull(synthesizer.Language, "English synthesizer language");
            Assert.IsNotNull(segmenter.Language, "English segmenter language");
            Assert.IsNotNull(EnglishTtsLanguage.Instance, "Shared English language");
        }

        /// <summary>
        /// Verifies deterministic, non-silent synthesis.
        /// </summary>
        [TestMethod]
        public void SpeakProducesDeterministicPcm()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            byte[] first = synthesizer.Speak("hello");
            byte[] second = synthesizer.Speak("hello");

            Assert.IsTrue(first.Length > 1000, "Speech must contain a useful number of samples.");
            Assert.AreEqual(first.Length, second.Length, "PCM byte count");
            Assert.AreEqual(ComputeHash(first), ComputeHash(second), "PCM hash");

            int changes = 0;
            for (int i = 1; i < first.Length; i++)
            {
                if (first[i] != first[i - 1])
                {
                    changes++;
                }
            }

            Assert.IsTrue(changes > 100, "Speech must contain varying samples.");
        }

        /// <summary>
        /// Verifies reused parser and renderer workspaces reset between different utterances.
        /// </summary>
        [TestMethod]
        public void ReusedWorkspacesResetBetweenUtterances()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            byte[] first = synthesizer.Speak("hello");
            synthesizer.Speak("A different phrase with several phonemes.");
            byte[] second = synthesizer.Speak("hello");

            Assert.AreEqual(first.Length, second.Length, "Reused workspace byte count");
            Assert.AreEqual(ComputeHash(first), ComputeHash(second), "Reused workspace hash");
        }

        /// <summary>
        /// Verifies streamed and buffered synthesis use the same renderer path.
        /// </summary>
        [TestMethod]
        public void StreamingOutputMatchesBufferedOutput()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            byte[] expected = synthesizer.Speak("Hello, I am Text2Speech.");
            ComparingSink sink = new ComparingSink(expected);
            int count = synthesizer.Speak("Hello, I am Text2Speech.", sink);

            Assert.AreEqual(expected.Length, count, "Streaming byte count");
            Assert.AreEqual(expected.Length, sink.Position, "Sink byte count");
            Assert.IsTrue(sink.Matches, "Streaming PCM must match buffered PCM.");
        }

        /// <summary>
        /// Verifies streaming uses 2 KB blocks to limit sink and filesystem write overhead.
        /// </summary>
        [TestMethod]
        public void StreamingUsesTwoKilobyteBlocks()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            byte[] expected = synthesizer.Speak("This phrase verifies larger streaming blocks.");
            BlockCountingSink sink = new BlockCountingSink();
            int count = synthesizer.Speak("This phrase verifies larger streaming blocks.", sink);
            int expectedWrites = (expected.Length + 2047) / 2048;

            Assert.AreEqual(expected.Length, count, "Streaming byte count");
            Assert.AreEqual(expected.Length, sink.ByteCount, "Sink byte count");
            Assert.AreEqual(expectedWrites, sink.WriteCount, "Sink write count");
            Assert.IsTrue(sink.MaximumBlockSize <= 2048, "Maximum streaming block size");
        }

        /// <summary>
        /// Verifies speech continues beyond the former twelve-word parser limit.
        /// </summary>
        [TestMethod]
        public void SpeakProcessesMoreThanTwelveWords()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            byte[] firstTwelveWords = synthesizer.Speak("Hello from Text 2 Speech. And this is really long so that");
            byte[] complete = synthesizer.Speak(
                "Hello from Text 2 Speech. And this is really long so that I can check things are working");

            Assert.IsTrue(complete.Length > firstTwelveWords.Length, "Speech after word twelve must be rendered.");
        }

        /// <summary>
        /// Verifies long prose is split into independently synthesizable rounds.
        /// </summary>
        [TestMethod]
        public void SegmenterSplitsLongTextAtSafeBoundaries()
        {
            const string Text =
                "This is a deliberately long passage for the shared segmenter. "
                + "It contains more than one synthesis round and should be split safely between words.";
            TtsSegmenter segmenter = new TtsSegmenter(EnglishTtsLanguage.Instance);
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            string[] segments = segmenter.Split(Text);

            Assert.IsTrue(segments.Length > 1, "Long text segment count");
            for (int i = 0; i < segments.Length; i++)
            {
                Assert.IsTrue(
                    segments[i].Length <= EnglishTtsLanguage.DefaultMaximumTextLength,
                    "Segment character count");
                Assert.IsTrue(synthesizer.Speak(segments[i]).Length > 0, "Segment synthesis");
            }
        }

        /// <summary>
        /// Verifies number expansion is split again when it exceeds the phoneme workspace.
        /// </summary>
        [TestMethod]
        public void SegmenterSplitsExpandedNumbersWithinPhonemeCapacity()
        {
            TtsSegmenter segmenter = new TtsSegmenter(EnglishTtsLanguage.Instance);
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            string[] segments = segmenter.Split(new string('8', EnglishTtsLanguage.DefaultMaximumTextLength));

            Assert.IsTrue(segments.Length > 1, "Expanded number segment count");
            for (int i = 0; i < segments.Length; i++)
            {
                Assert.IsTrue(synthesizer.Speak(segments[i]).Length > 0, "Expanded segment synthesis");
            }
        }

        /// <summary>
        /// Verifies number expansion matches equivalent written words.
        /// </summary>
        [TestMethod]
        public void NumberExpansionMatchesWrittenWords()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            AssertPcmEqual(
                synthesizer.Speak("25 cats"),
                synthesizer.Speak("twenty five cats"),
                "Number expansion");
        }

        /// <summary>
        /// Verifies punctuation and spelling rules affect the rendered speech.
        /// </summary>
        [TestMethod]
        public void FrontendRulesAffectOutput()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            byte[] statement = synthesizer.Speak("is this a question.");
            byte[] question = synthesizer.Speak("is this a question?");
            byte[] shortVowel = synthesizer.Speak("tim");
            byte[] magicE = synthesizer.Speak("time");

            Assert.IsTrue(ComputeHash(statement) != ComputeHash(question), "Question intonation");
            Assert.IsTrue(ComputeHash(shortVowel) != ComputeHash(magicE), "Magic-e rule");
        }

        /// <summary>
        /// Verifies voice controls alter pitch, timbre, and speaking duration.
        /// </summary>
        [TestMethod]
        public void VoiceOptionsChangeOutputAndDuration()
        {
            const string Text = "Smooth speech sounds better.";
            byte[] normal = CreateEnglishSynthesizer().Speak(Text);
            byte[] pitched = new TtsSynthesizer(EnglishTtsLanguage.Instance, new VoiceOptions(pitchShift: 3)).Speak(Text);
            byte[] expressive = new TtsSynthesizer(
                EnglishTtsLanguage.Instance,
                new VoiceOptions(
                    formantScalePercent: 95,
                    intonationPercent: 150,
                    fricativeNoisePercent: 75,
                    transitionSmoothingPercent: 150)).Speak(Text);
            byte[] slow = new TtsSynthesizer(EnglishTtsLanguage.Instance, new VoiceOptions(speedPercent: 75)).Speak(Text);
            byte[] fast = new TtsSynthesizer(EnglishTtsLanguage.Instance, new VoiceOptions(speedPercent: 150)).Speak(Text);

            Assert.AreEqual(normal.Length, pitched.Length, "Pitch must not change duration");
            Assert.IsTrue(ComputeHash(normal) != ComputeHash(pitched), "Pitch output");
            Assert.IsTrue(ComputeHash(normal) != ComputeHash(expressive), "Expressive output");
            Assert.IsTrue(slow.Length > normal.Length, "Slower voice duration");
            Assert.IsTrue(fast.Length < normal.Length, "Faster voice duration");
        }

        /// <summary>
        /// Verifies configured streaming output matches configured buffered output.
        /// </summary>
        [TestMethod]
        public void ConfiguredStreamingMatchesBufferedOutput()
        {
            VoiceOptions options = new VoiceOptions(-1, 95, 98, 140, 80, 150);
            TtsSynthesizer synthesizer = new TtsSynthesizer(EnglishTtsLanguage.Instance, options);
            byte[] expected = synthesizer.Speak("A smoother configured voice.");
            ComparingSink sink = new ComparingSink(expected);
            int count = synthesizer.Speak("A smoother configured voice.", sink);

            Assert.AreEqual(expected.Length, count, "Configured streaming byte count");
            Assert.AreEqual(expected.Length, sink.Position, "Configured sink byte count");
            Assert.IsTrue(sink.Matches, "Configured streaming PCM must match buffered PCM.");
        }

        /// <summary>
        /// Verifies the core tee sink forwards each block to both destinations in order.
        /// </summary>
        [TestMethod]
        public void TeePcmSinkForwardsBlocksInOrder()
        {
            SequenceState state = new SequenceState();
            SequencedSink first = new SequencedSink(state, 0);
            SequencedSink second = new SequencedSink(state, 1);
            TeePcmSink tee = new TeePcmSink(first, second);
            byte[] samples = { 1, 2, 3 };

            tee.Write(samples, 0, samples.Length);

            Assert.AreEqual(2, state.Position, "Sink call order");
            Assert.AreEqual(samples.Length, first.ByteCount, "First sink byte count");
            Assert.AreEqual(samples.Length, second.ByteCount, "Second sink byte count");
            Assert.ThrowsException(
                typeof(ArgumentNullException),
                delegate { new TeePcmSink(null, second); },
                "Null first sink");
            Assert.ThrowsException(
                typeof(ArgumentNullException),
                delegate { new TeePcmSink(first, null); },
                "Null second sink");
        }

        /// <summary>
        /// Verifies voice controls enforce their documented ranges.
        /// </summary>
        [TestMethod]
        public void InvalidVoiceOptionsAreRejected()
        {
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(pitchShift: 13); },
                "Pitch range");
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(speedPercent: 59); },
                "Speed range");
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(formantScalePercent: 121); },
                "Formant range");
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(intonationPercent: 201); },
                "Intonation range");
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(fricativeNoisePercent: 151); },
                "Noise range");
            Assert.ThrowsException(
                typeof(ArgumentOutOfRangeException),
                delegate { new VoiceOptions(transitionSmoothingPercent: 201); },
                "Smoothing range");
            Assert.ThrowsException(
                typeof(ArgumentNullException),
                delegate { new TtsSynthesizer(EnglishTtsLanguage.Instance, null); },
                "Null options");
        }

        /// <summary>
        /// Verifies a custom language frontend can drive synthesis and segmentation.
        /// </summary>
        [TestMethod]
        public void CustomLanguageFrontendUsesSharedRendererAndSegmenter()
        {
            TestLanguage language = new TestLanguage();
            TtsSynthesizer synthesizer = new TtsSynthesizer(language, new VoiceOptions());
            byte[] pcm = synthesizer.Speak("xx");
            string[] segments = new TtsSegmenter(language).Split("xx xx xx");

            Assert.AreEqual(language, synthesizer.Language, "Synthesizer language");
            Assert.AreEqual(800, pcm.Length, "Custom frontend sample count");
            Assert.AreEqual(3, segments.Length, "Custom language segment count");
            for (int i = 0; i < segments.Length; i++)
            {
                Assert.IsTrue(segments[i].Length <= language.MaximumTextLength, "Custom segment length");
                Assert.IsTrue(synthesizer.Speak(segments[i]).Length > 0, "Custom segment synthesis");
            }
        }

        /// <summary>
        /// Verifies the public phoneme workspace remains bounded and reusable.
        /// </summary>
        [TestMethod]
        public void PhonemeBufferEnforcesFixedCapacity()
        {
            TtsPhonemeBuffer buffer = new TtsPhonemeBuffer();
            for (int i = 0; i < buffer.Capacity; i++)
            {
                Assert.IsTrue(buffer.TryAdd(TestLanguage.Tone, 0), "Available phoneme slot");
            }

            Assert.IsFalse(buffer.TryAdd(TestLanguage.Tone, 0), "Full phoneme buffer");
            Assert.AreEqual(TtsPhonemeBuffer.MaximumPhonemes, buffer.Count, "Full phoneme count");
            buffer.Clear();
            Assert.AreEqual(0, buffer.Count, "Cleared phoneme count");
        }

        /// <summary>
        /// Verifies invalid input and destinations are rejected.
        /// </summary>
        [TestMethod]
        public void InvalidArgumentsAreRejected()
        {
            TtsSynthesizer synthesizer = CreateEnglishSynthesizer();
            Assert.ThrowsException(typeof(ArgumentNullException), delegate { synthesizer.Speak(null); }, "Null text");
            Assert.ThrowsException(typeof(ArgumentException), delegate { synthesizer.Speak(string.Empty); }, "Empty text");
            Assert.ThrowsException(typeof(ArgumentException), delegate { synthesizer.Speak("$$$$"); }, "Unsupported text");
            Assert.ThrowsException(typeof(ArgumentNullException), delegate { synthesizer.Speak("hello", null); }, "Null sink");
            Assert.ThrowsException(
                typeof(ArgumentException),
                delegate
                {
                    synthesizer.Speak(
                        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                },
                "Overlength text");
        }

        private static TtsSynthesizer CreateEnglishSynthesizer()
        {
            return new TtsSynthesizer(EnglishTtsLanguage.Instance, new VoiceOptions());
        }

        private static void AssertPcmEqual(byte[] expected, byte[] actual, string message)
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " length");
            Assert.AreEqual(ComputeHash(expected), ComputeHash(actual), message + " hash");
        }

        private static uint ComputeHash(byte[] data)
        {
            uint hash = 2166136261;
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash = unchecked(hash * 16777619);
            }

            return hash;
        }
    }
}
