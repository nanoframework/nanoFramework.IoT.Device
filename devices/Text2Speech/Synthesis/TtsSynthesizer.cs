// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Converts bounded text into unsigned 8-bit mono PCM using a language frontend and the
    /// Text2Speech formant renderer.
    /// </summary>
    public sealed class TtsSynthesizer
    {
        private readonly VoiceOptions _options;
        private readonly object _syncRoot;
        private readonly ITtsLanguageFrontend _frontend;
        private readonly TtsPhonemeBuffer _phonemes;
        private readonly FormantRenderer _renderer;

        /// <summary>
        /// The generated PCM sample rate.
        /// </summary>
        public const int SampleRate = 8000;

        /// <summary>
        /// The number of bits in each generated sample.
        /// </summary>
        public const int BitsPerSample = 8;

        /// <summary>
        /// The number of generated audio channels.
        /// </summary>
        public const int Channels = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="TtsSynthesizer" /> class.
        /// </summary>
        /// <param name="language">The language definition and frontend factory.</param>
        /// <param name="options">The voice controls used for every synthesized utterance.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="language" /> or <paramref name="options" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The language reports a non-positive maximum input length.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The language does not create a frontend.
        /// </exception>
        public TtsSynthesizer(ITtsLanguage language, VoiceOptions options)
        {
            if (language == null || options == null)
            {
                throw new ArgumentNullException();
            }

            if (language.MaximumTextLength <= 0)
            {
                throw new ArgumentException();
            }

            ITtsLanguageFrontend frontend = language.CreateFrontend();
            if (frontend == null)
            {
                throw new InvalidOperationException();
            }

            Language = language;
            _options = options;
            _syncRoot = new object();
            _frontend = frontend;
            _phonemes = new TtsPhonemeBuffer();
            _renderer = new FormantRenderer(_options);
        }

        /// <summary>
        /// Gets the language definition used by this synthesizer.
        /// </summary>
        /// <value>The immutable language definition.</value>
        public ITtsLanguage Language { get; }

        /// <summary>
        /// Synthesizes text into a newly allocated PCM buffer.
        /// </summary>
        /// <param name="text">The text accepted by the configured language.</param>
        /// <returns>Unsigned 8-bit mono PCM at <see cref="SampleRate" /> Hz.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="text" /> is empty, exceeds the configured language limit, contains no
        /// supported text, or exceeds the phoneme capacity.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The rendered PCM length does not match the calculated output length.
        /// </exception>
        public byte[] Speak(string text)
        {
            lock (_syncRoot)
            {
                Prepare(text);
                ByteArrayPcmSink sink = new ByteArrayPcmSink(_phonemes.GetSampleCount(_options));
                int count = _renderer.Render(_phonemes, sink);
                if (count != sink.Buffer.Length)
                {
                    throw new InvalidOperationException();
                }

                return sink.Buffer;
            }
        }

        /// <summary>
        /// Synthesizes text and streams the PCM samples to a sink.
        /// </summary>
        /// <param name="text">The text accepted by the configured language.</param>
        /// <param name="sink">The destination for generated PCM blocks.</param>
        /// <returns>The number of PCM bytes written.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="text" /> or <paramref name="sink" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="text" /> is empty, exceeds the configured language limit, contains no
        /// supported text, or exceeds the phoneme capacity.
        /// </exception>
        public int Speak(string text, IPcmSink sink)
        {
            if (sink == null)
            {
                throw new ArgumentNullException();
            }

            lock (_syncRoot)
            {
                Prepare(text);
                return _renderer.Render(_phonemes, sink);
            }
        }

        private void Prepare(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            if (text.Length == 0 || text.Length > Language.MaximumTextLength)
            {
                throw new ArgumentException();
            }

            _phonemes.Clear();
            if (!_frontend.TryPrepare(text, _phonemes) || _phonemes.Count == 0)
            {
                throw new ArgumentException();
            }
        }
    }
}
