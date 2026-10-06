// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Text2Speech.Samples
{
    /// <summary>
    /// Synthesizes English and French speech directly to I2S on an ESP32-S3-BOX-Lite.
    /// </summary>
    public static class Program
    {
        private const string FrenchStreamingText =
            "Cette phrase française démontre la synthèse en diffusion continue. Elle est assez "
            + "longue pour être divisée en plusieurs segments, puis jouée sans interruption.";

        private static readonly TtsSynthesizer EnglishSmoothVoice =
            new TtsSynthesizer(
                EnglishTtsLanguage.Instance,
                new VoiceOptions(
                    pitchShift: -1,
                    speedPercent: 90,
                    formantScalePercent: 98,
                    intonationPercent: 140,
                    fricativeNoisePercent: 80,
                    transitionSmoothingPercent: 180));

        private static readonly TtsSynthesizer FrenchFastBrightVoice =
            new TtsSynthesizer(
                new FrenchTtsLanguage(),
                new VoiceOptions(
                    pitchShift: 3,
                    speedPercent: 125,
                    formantScalePercent: 108,
                    intonationPercent: 160,
                    fricativeNoisePercent: 90,
                    transitionSmoothingPercent: 130));

        private static readonly TtsSynthesizer EnglishDeepVoice =
            new TtsSynthesizer(
                EnglishTtsLanguage.Instance,
                new VoiceOptions(
                    pitchShift: -4,
                    speedPercent: 100,
                    formantScalePercent: 90,
                    intonationPercent: 80,
                    fricativeNoisePercent: 110,
                    transitionSmoothingPercent: 120));

        private static readonly TtsSynthesizer FrenchStreamingVoice =
            new TtsSynthesizer(
                new FrenchTtsLanguage(),
                new VoiceOptions(
                    pitchShift: 1,
                    speedPercent: 115,
                    formantScalePercent: 102,
                    intonationPercent: 130,
                    fricativeNoisePercent: 85,
                    transitionSmoothingPercent: 150));

        private static readonly TtsSegmenter EnglishSegmenter =
            new TtsSegmenter(EnglishTtsLanguage.Instance);

        private static readonly TtsSegmenter FrenchSegmenter =
            new TtsSegmenter(new FrenchTtsLanguage());

        private static Esp32S3BoxLiteWavPlayer _player = null;

        /// <summary>
        /// Synthesizes buffered and streamed sample phrases in both built-in languages.
        /// </summary>
        public static void Main()
        {
            _player = new Esp32S3BoxLiteWavPlayer(I2sPlaybackMode.Fast8Khz);
            Play(
                EnglishSegmenter,
                EnglishSmoothVoice,
                "english-smooth",
                "Hello. I love nano Framework. I run on dot net nano Framework.");
            Play(
                FrenchSegmenter,
                FrenchFastBrightVoice,
                "french-fast-bright",
                "Bonjour. J'aime nano Framework et je parle maintenant français.");
            Play(
                EnglishSegmenter,
                EnglishDeepVoice,
                "english-deep",
                "All this is optimized for low memory and low power consumption.");
            PlayStreaming(
                FrenchSegmenter,
                FrenchStreamingVoice,
                "french-streaming",
                FrenchStreamingText);
            Thread.Sleep(Timeout.Infinite);
        }

        private static void PlayStreaming(
            TtsSegmenter segmenter,
            TtsSynthesizer synthesizer,
            string voiceName,
            string text)
        {
            try
            {
                EnsurePlayer();
                string[] segments = segmenter.Split(text);
                DateTime started = DateTime.UtcNow;
                int pcmLength = _player.SpeakStreaming(synthesizer, segments);

                long totalMilliseconds = ElapsedMilliseconds(started);
                int audioMilliseconds = AudioMilliseconds(pcmLength);
                long overheadMilliseconds = totalMilliseconds - audioMilliseconds;

                Debug.WriteLine(
                    "[streaming:" + voiceName + "] rounds=" + segments.Length.ToString()
                    + ", samples=" + pcmLength.ToString()
                    + ", audio=" + audioMilliseconds.ToString() + " ms"
                    + ", total=" + totalMilliseconds.ToString() + " ms"
                    + ", overhead=" + overheadMilliseconds.ToString() + " ms.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Text2Speech streaming sample failed: " + ex.Message);
                throw;
            }
        }

        private static void Play(
            TtsSegmenter segmenter,
            TtsSynthesizer synthesizer,
            string voiceName,
            string text)
        {
            try
            {
                EnsurePlayer();
                string[] segments = segmenter.Split(text);
                int pcmLength = 0;
                long synthesisMilliseconds = 0;
                long playbackMilliseconds = 0;
                for (int i = 0; i < segments.Length; i++)
                {
                    DateTime synthesisStarted = DateTime.UtcNow;
                    byte[] pcm = synthesizer.Speak(segments[i]);
                    synthesisMilliseconds += ElapsedMilliseconds(synthesisStarted);
                    pcmLength += pcm.Length;

                    DateTime playbackStarted = DateTime.UtcNow;
                    _player.Play(pcm);
                    playbackMilliseconds += ElapsedMilliseconds(playbackStarted);
                }

                int audioMilliseconds = AudioMilliseconds(pcmLength);
                int realTimePercent = synthesisMilliseconds > 0
                    ? (int)((audioMilliseconds * 100) / synthesisMilliseconds)
                    : 0;
                Debug.WriteLine(
                    "[buffered:" + voiceName + "] rounds=" + segments.Length.ToString()
                    + ", samples=" + pcmLength.ToString()
                    + ", audio=" + audioMilliseconds.ToString() + " ms"
                    + ", synthesis=" + synthesisMilliseconds.ToString() + " ms"
                    + ", synthesis-speed=" + realTimePercent.ToString() + "% real-time"
                    + ", playback=" + playbackMilliseconds.ToString() + " ms.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Text2Speech sample failed: " + ex.Message);
                throw;
            }
        }

        private static int AudioMilliseconds(int sampleCount)
        {
            return (sampleCount * 1000) / TtsSynthesizer.SampleRate;
        }

        private static long ElapsedMilliseconds(DateTime started)
        {
            return (DateTime.UtcNow - started).Ticks / TimeSpan.TicksPerMillisecond;
        }

        private static void EnsurePlayer()
        {
            if (_player == null)
            {
                _player = new Esp32S3BoxLiteWavPlayer();
            }
        }
    }
}
