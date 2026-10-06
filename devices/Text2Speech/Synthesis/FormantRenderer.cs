// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Renders a parsed phoneme sequence through the integer formant synthesis engine.
    /// </summary>
    internal sealed class FormantRenderer
    {
        private const int MidiHzBase = 36;
        private const int BlendSamples = 200;
        private const int ControlPeriod = 4;
        private const int InterpolationShift = 16;
        private const int IncrementInterpolationShift = 8;
        private const int PhaseMask = 0x00FFFFFF;
        private const int PhaseHalf = 0x00800000;
        private const int PhaseIndexShift = 16;
        private const int PhaseIncrementPerHz = 2097;
        private const int AmplitudeReciprocal = 259;
        private const int AmplitudeShift = 15;
        private const int OutputBufferSize = 2048;
        private const int SilenceBlockSize = 256;

        private static readonly byte[] SilenceBlock = CreateSilenceBlock();

        private readonly VoiceOptions _options;
        private readonly byte[] _outputBuffer = new byte[OutputBufferSize];
        private ushort _lfsr;
        private int _lowPass1;
        private int _lowPass2;
        private int _previousNoise;
        private IPcmSink _sink;
        private int _outputPosition;
        private int _outputTotal;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormantRenderer" /> class.
        /// </summary>
        /// <param name="options">The immutable voice controls used during rendering.</param>
        public FormantRenderer(VoiceOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Renders all prepared phonemes to a PCM sink.
        /// </summary>
        /// <param name="phonemes">The prepared phoneme sequence.</param>
        /// <param name="sink">The destination for unsigned 8-bit PCM blocks.</param>
        /// <returns>The number of PCM samples written.</returns>
        public int Render(TtsPhonemeBuffer phonemes, IPcmSink sink)
        {
            _lfsr = 0xACE1;
            _lowPass1 = 0;
            _lowPass2 = 0;
            _previousNoise = 0;
            _sink = sink;
            _outputPosition = 0;
            _outputTotal = 0;

            for (int i = 0; i < phonemes.Count; i++)
            {
                RenderPhoneme(phonemes, i);
            }

            FlushOutput();
            return _outputTotal;
        }

        private static byte[] CreateSilenceBlock()
        {
            byte[] block = new byte[SilenceBlockSize];
            for (int i = 0; i < block.Length; i++)
            {
                block[i] = 128;
            }

            return block;
        }

        private static int RampStep(int peak, int samples)
        {
            return samples > 0 ? (peak << InterpolationShift) / samples : 0;
        }

        private static int Envelope(
            int index,
            int duration,
            int attack,
            int releaseStart,
            int peak,
            int attackStep,
            int releaseStep)
        {
            if (index < attack)
            {
                return (index * attackStep) >> InterpolationShift;
            }

            if (index >= releaseStart)
            {
                return ((duration - 1 - index) * releaseStep) >> InterpolationShift;
            }

            return peak;
        }

        private static int ShiftTowardZero(int value, int shift)
        {
            return value >= 0 ? value >> shift : -((-value) >> shift);
        }

        private static int PhaseIncrement(int hertz)
        {
            return hertz * PhaseIncrementPerHz;
        }

        private static int MidiToHz(int midi)
        {
            return RendererData.MidiHz[ClampMidi(midi) - MidiHzBase];
        }

        private static int ClampMidi(int midi)
        {
            if (midi < MidiHzBase)
            {
                return MidiHzBase;
            }

            return midi > MidiHzBase + 48 ? MidiHzBase + 48 : midi;
        }

        private static int ClampSample(int sample)
        {
            if (sample > 127)
            {
                return 127;
            }

            return sample < -127 ? -127 : sample;
        }

        private static int ControlBlockLength(int index, int duration)
        {
            int remaining = duration - index;
            return remaining < ControlPeriod ? remaining : ControlPeriod;
        }

        private int FormantIncrement(int hertz)
        {
            return PhaseIncrement(_options.ScaleFormant(hertz));
        }

        private void RenderPhoneme(TtsPhonemeBuffer phonemes, int index)
        {
            PhonemeEntry entry = phonemes[index];
            TtsPhoneme phoneme = entry.Phoneme;
            int nextPitchOffset = index + 1 < phonemes.Count
                ? phonemes[index + 1].PitchOffset
                : entry.PitchOffset;

            int previousF1 = 0;
            int previousF2 = 0;
            int previousF3 = 0;
            for (int i = index - 1; i >= 0; i--)
            {
                TtsPhoneme previous = phonemes[i].Phoneme;
                if (previous.Type == TtsPhonemeType.Voiced)
                {
                    previousF1 = previous.Glide1 > 0 ? previous.Glide1 : previous.F1;
                    previousF2 = previous.Glide2 > 0 ? previous.Glide2 : previous.F2;
                    previousF3 = previous.Glide3 > 0 ? previous.Glide3 : previous.F3;
                    break;
                }
            }

            int nextF1 = 0;
            int nextF2 = 0;
            int nextF3 = 0;
            for (int i = index + 1; i < phonemes.Count; i++)
            {
                TtsPhoneme next = phonemes[i].Phoneme;
                if (next.Type == TtsPhonemeType.Voiced)
                {
                    nextF1 = next.F1;
                    nextF2 = next.F2;
                    nextF3 = next.F3;
                    break;
                }
            }

            if (phoneme.Type == TtsPhonemeType.Voiced)
            {
                EmitVoiced(
                    phoneme,
                    entry.PitchOffset,
                    nextPitchOffset,
                    entry.DurationPercent,
                    previousF1,
                    previousF2,
                    previousF3,
                    nextF1,
                    nextF2,
                    nextF3);
            }
            else if (phoneme.Type == TtsPhonemeType.VoicedFricative)
            {
                EmitVoicedFricative(phoneme, entry.PitchOffset, entry.DurationPercent);
            }
            else
            {
                EmitPhoneme(phoneme, entry.PitchOffset, entry.DurationPercent);
            }
        }

        private void EmitVoiced(
            TtsPhoneme phoneme,
            int pitchOffset,
            int nextPitchOffset,
            int durationPercent,
            int previousF1,
            int previousF2,
            int previousF3,
            int nextF1,
            int nextF2,
            int nextF3)
        {
            int duration = _options.DurationToSamples(phoneme.Duration, durationPercent);
            int attack = duration / 8;
            if (attack < ControlPeriod)
            {
                attack = ControlPeriod;
            }

            int releaseStart = duration - (duration / 6);
            if (releaseStart < attack)
            {
                releaseStart = attack;
            }

            int blend = _options.TransitionSamples(BlendSamples);
            if (blend > duration / 2)
            {
                blend = duration / 2;
            }

            int startPitch = ClampMidi(
                phoneme.PitchMidi + _options.PitchShift + _options.ScaleIntonation(pitchOffset));
            int endPitch = ClampMidi(
                phoneme.PitchMidi + _options.PitchShift + _options.ScaleIntonation(nextPitchOffset));
            int pitchIncrement = PhaseIncrement(MidiToHz(startPitch));
            int pitchIncrementEnd = PhaseIncrement(MidiToHz(endPitch));
            int pitchPosition = pitchIncrement << IncrementInterpolationShift;
            int pitchStep = ((pitchIncrementEnd - pitchIncrement) << IncrementInterpolationShift)
                / duration;

            int baseIncrement1 = FormantIncrement(phoneme.F1);
            int baseIncrement2 = FormantIncrement(phoneme.F2);
            int baseIncrement3 = FormantIncrement(phoneme.F3);
            int glideIncrement1 = phoneme.Glide1 > 0
                ? FormantIncrement(phoneme.Glide1)
                : baseIncrement1;
            int glideIncrement2 = phoneme.Glide2 > 0
                ? FormantIncrement(phoneme.Glide2)
                : baseIncrement2;
            int glideIncrement3 = phoneme.Glide3 > 0
                ? FormantIncrement(phoneme.Glide3)
                : baseIncrement3;
            int basePosition1 = baseIncrement1 << IncrementInterpolationShift;
            int basePosition2 = baseIncrement2 << IncrementInterpolationShift;
            int basePosition3 = baseIncrement3 << IncrementInterpolationShift;
            int baseStep1 = ((glideIncrement1 - baseIncrement1) << IncrementInterpolationShift)
                / duration;
            int baseStep2 = ((glideIncrement2 - baseIncrement2) << IncrementInterpolationShift)
                / duration;
            int baseStep3 = ((glideIncrement3 - baseIncrement3) << IncrementInterpolationShift)
                / duration;

            int startPosition1 = FormantIncrement(previousF1) << IncrementInterpolationShift;
            int startPosition2 = FormantIncrement(previousF2) << IncrementInterpolationShift;
            int startPosition3 = FormantIncrement(previousF3) << IncrementInterpolationShift;
            int startStep1 = blend > 0 && previousF1 > 0
                ? ((basePosition1 + (baseStep1 * blend)) - startPosition1) / blend
                : 0;
            int startStep2 = blend > 0 && previousF2 > 0
                ? ((basePosition2 + (baseStep2 * blend)) - startPosition2) / blend
                : 0;
            int startStep3 = blend > 0 && previousF3 > 0
                ? ((basePosition3 + (baseStep3 * blend)) - startPosition3) / blend
                : 0;

            int endStart = duration - blend;
            int endPosition1 = basePosition1 + (baseStep1 * endStart);
            int endPosition2 = basePosition2 + (baseStep2 * endStart);
            int endPosition3 = basePosition3 + (baseStep3 * endStart);
            int endStep1 = blend > 0 && nextF1 > 0 && phoneme.Glide1 == 0
                ? ((FormantIncrement(nextF1) << IncrementInterpolationShift) - endPosition1) / blend
                : 0;
            int endStep2 = blend > 0 && nextF2 > 0 && phoneme.Glide2 == 0
                ? ((FormantIncrement(nextF2) << IncrementInterpolationShift) - endPosition2) / blend
                : 0;
            int endStep3 = blend > 0 && nextF3 > 0 && phoneme.Glide3 == 0
                ? ((FormantIncrement(nextF3) << IncrementInterpolationShift) - endPosition3) / blend
                : 0;

            int attackStep = RampStep(127, attack);
            int releaseStep = RampStep(127, duration - releaseStart);
            int phase0 = 0;
            int phase1 = 0;
            int phase2 = 0;
            int phase3 = 0;
            int a1 = phoneme.A1;
            int a2 = phoneme.A2;
            int a3 = phoneme.A3;
            int[] sine = RendererData.Sine;

            for (int i = 0; i < duration; i += ControlPeriod)
            {
                int blockLength = ControlBlockLength(i, duration);
                int increment1;
                int increment2;
                int increment3;
                increment1 = basePosition1 >> IncrementInterpolationShift;
                if (i < blend && previousF1 > 0)
                {
                    increment1 = startPosition1 >> IncrementInterpolationShift;
                    startPosition1 += startStep1 * blockLength;
                }
                else if (i >= endStart && nextF1 > 0 && phoneme.Glide1 == 0)
                {
                    increment1 = endPosition1 >> IncrementInterpolationShift;
                    endPosition1 += endStep1 * blockLength;
                }

                increment2 = basePosition2 >> IncrementInterpolationShift;
                if (i < blend && previousF2 > 0)
                {
                    increment2 = startPosition2 >> IncrementInterpolationShift;
                    startPosition2 += startStep2 * blockLength;
                }
                else if (i >= endStart && nextF2 > 0 && phoneme.Glide2 == 0)
                {
                    increment2 = endPosition2 >> IncrementInterpolationShift;
                    endPosition2 += endStep2 * blockLength;
                }

                increment3 = basePosition3 >> IncrementInterpolationShift;
                if (i < blend && previousF3 > 0)
                {
                    increment3 = startPosition3 >> IncrementInterpolationShift;
                    startPosition3 += startStep3 * blockLength;
                }
                else if (i >= endStart && nextF3 > 0 && phoneme.Glide3 == 0)
                {
                    increment3 = endPosition3 >> IncrementInterpolationShift;
                    endPosition3 += endStep3 * blockLength;
                }

                int increment0 = pitchPosition >> IncrementInterpolationShift;
                int amplitude = Envelope(
                    i,
                    duration,
                    attack,
                    releaseStart,
                    127,
                    attackStep,
                    releaseStep);
                for (int sampleIndex = 0; sampleIndex < blockLength; sampleIndex++)
                {
                    int previousPhase0 = phase0;
                    phase0 = (phase0 + increment0) & PhaseMask;
                    if (phase0 < previousPhase0)
                    {
                        phase1 = 0;
                        phase2 = 0;
                        phase3 = 0;
                    }

                    phase1 = (phase1 + increment1) & PhaseMask;
                    phase2 = (phase2 + increment2) & PhaseMask;
                    phase3 = (phase3 + increment3) & PhaseMask;
                    int rectangle = phase3 >= PhaseHalf ? 64 : -64;
                    int mixed = (sine[phase1 >> PhaseIndexShift] * a1)
                        + (sine[phase2 >> PhaseIndexShift] * a2)
                        + (rectangle * a3);
                    int sample = mixed >= 0 ? mixed >> 4 : -((-mixed) >> 4);
                    int scaled = sample * (amplitude * AmplitudeReciprocal);
                    int outputSample = scaled >= 0
                        ? scaled >> AmplitudeShift
                        : -((-scaled) >> AmplitudeShift);
                    if (outputSample > 127)
                    {
                        outputSample = 127;
                    }
                    else if (outputSample < -127)
                    {
                        outputSample = -127;
                    }

                    _outputBuffer[_outputPosition++] = (byte)(outputSample + 128);
                    _outputTotal++;
                    if (_outputPosition == _outputBuffer.Length)
                    {
                        FlushOutput();
                    }
                }

                pitchPosition += pitchStep * blockLength;
                basePosition1 += baseStep1 * blockLength;
                basePosition2 += baseStep2 * blockLength;
                basePosition3 += baseStep3 * blockLength;
            }
        }

        private void EmitVoicedFricative(
            TtsPhoneme phoneme,
            int pitchOffset,
            int durationPercent)
        {
            int duration = _options.DurationToSamples(phoneme.Duration, durationPercent);
            int attack = duration / 6;
            if (attack < ControlPeriod)
            {
                attack = ControlPeriod;
            }

            int releaseStart = duration - (duration / 5);
            if (releaseStart < attack)
            {
                releaseStart = attack;
            }

            int pitchHz = MidiToHz(
                ClampMidi(
                    phoneme.PitchMidi + _options.PitchShift
                    + _options.ScaleIntonation(pitchOffset)));
            int attackStep = RampStep(127, attack);
            int releaseStep = RampStep(127, duration - releaseStart);
            int phase0 = 0;
            int phase1 = 0;
            int phase2 = 0;
            int increment0 = PhaseIncrement(pitchHz);
            int increment1 = FormantIncrement(phoneme.F1);
            int increment2 = FormantIncrement(phoneme.F2);
            int a1 = phoneme.A1;
            int a2 = phoneme.A2;
            int noiseMode = phoneme.A3;
            int[] sine = RendererData.Sine;

            for (int i = 0; i < duration; i += ControlPeriod)
            {
                int blockLength = ControlBlockLength(i, duration);
                int amplitude = Envelope(
                    i,
                    duration,
                    attack,
                    releaseStart,
                    127,
                    attackStep,
                    releaseStep);
                for (int sampleIndex = 0; sampleIndex < blockLength; sampleIndex++)
                {
                    int previousPhase0 = phase0;
                    phase0 = (phase0 + increment0) & PhaseMask;
                    if (phase0 < previousPhase0)
                    {
                        phase1 = 0;
                        phase2 = 0;
                    }

                    phase1 = (phase1 + increment1) & PhaseMask;
                    phase2 = (phase2 + increment2) & PhaseMask;
                    int secondVoice = sine[phase2 >> PhaseIndexShift] * a1;
                    secondVoice = secondVoice >= 0 ? secondVoice >> 1 : -((-secondVoice) >> 1);
                    int mixedVoice = (sine[phase1 >> PhaseIndexShift] * a1) + secondVoice;
                    int voice = mixedVoice >= 0 ? mixedVoice >> 4 : -((-mixedVoice) >> 4);
                    int mixedNoise = _options.ScaleNoise(NoiseShaped(noiseMode)) * a2;
                    int noise = mixedNoise >= 0 ? mixedNoise >> 3 : -((-mixedNoise) >> 3);
                    int combined = voice + noise;
                    int scaled = combined * (amplitude * AmplitudeReciprocal);
                    int outputSample = scaled >= 0
                        ? scaled >> AmplitudeShift
                        : -((-scaled) >> AmplitudeShift);
                    if (outputSample > 127)
                    {
                        outputSample = 127;
                    }
                    else if (outputSample < -127)
                    {
                        outputSample = -127;
                    }

                    _outputBuffer[_outputPosition++] = (byte)(outputSample + 128);
                    _outputTotal++;
                    if (_outputPosition == _outputBuffer.Length)
                    {
                        FlushOutput();
                    }
                }
            }
        }

        private void EmitPhoneme(
            TtsPhoneme phoneme,
            int pitchOffset,
            int durationPercent)
        {
            int duration = _options.DurationToSamples(phoneme.Duration, durationPercent);
            int attack = duration / 8;
            if (attack < 4)
            {
                attack = 4;
            }

            int releaseStart = duration - (duration / 6);
            if (releaseStart < attack)
            {
                releaseStart = attack;
            }

            if (phoneme.Type == TtsPhonemeType.Fricative)
            {
                int peak = 12 * phoneme.A2;
                int attackStep = RampStep(peak, attack);
                int releaseStep = RampStep(peak, duration - releaseStart);
                for (int i = 0; i < duration; i += ControlPeriod)
                {
                    int blockLength = ControlBlockLength(i, duration);
                    int amplitude = Envelope(
                        i,
                        duration,
                        attack,
                        releaseStart,
                        peak,
                        attackStep,
                        releaseStep);
                    for (int sampleIndex = 0; sampleIndex < blockLength; sampleIndex++)
                    {
                        int sample = _options.ScaleNoise(NoiseShaped(phoneme.A1));
                        int scaled = sample * (amplitude * AmplitudeReciprocal);
                        int outputSample = scaled >= 0
                            ? scaled >> AmplitudeShift
                            : -((-scaled) >> AmplitudeShift);
                        if (outputSample > 127)
                        {
                            outputSample = 127;
                        }
                        else if (outputSample < -127)
                        {
                            outputSample = -127;
                        }

                        _outputBuffer[_outputPosition++] = (byte)(outputSample + 128);
                        _outputTotal++;
                        if (_outputPosition == _outputBuffer.Length)
                        {
                            FlushOutput();
                        }
                    }
                }

                return;
            }

            if (phoneme.Type == TtsPhonemeType.Stop)
            {
                int pitchHz = MidiToHz(
                    ClampMidi(
                        phoneme.PitchMidi + _options.PitchShift
                        + _options.ScaleIntonation(pitchOffset)));
                if (phoneme.Voiced)
                {
                    int murmur = _options.DurationToSamples(18, durationPercent);
                    EmitWos(
                        phoneme.F1 > 0 ? phoneme.F1 : 300,
                        700,
                        2400,
                        3,
                        2,
                        0,
                        pitchHz,
                        murmur,
                        murmur / 4,
                        murmur);
                }

                int closure = _options.DurationToSamples(20, durationPercent);
                EmitSilence(closure);

                int burstMilliseconds;
                if (phoneme.Voiced)
                {
                    burstMilliseconds = 18;
                }
                else
                {
                    burstMilliseconds = phoneme.Duration - 20;
                    if (burstMilliseconds < 20)
                    {
                        burstMilliseconds = 20;
                    }
                    else if (burstMilliseconds > 45)
                    {
                        burstMilliseconds = 45;
                    }
                }

                int burst = _options.DurationToSamples(burstMilliseconds, durationPercent);
                int peak = 12 * phoneme.A2;
                int burstStep = RampStep(peak, burst);
                for (int i = 0; i < burst; i += ControlPeriod)
                {
                    int blockLength = ControlBlockLength(i, burst);
                    int amplitude = ((burst - 1 - i) * burstStep) >> InterpolationShift;
                    for (int sampleIndex = 0; sampleIndex < blockLength; sampleIndex++)
                    {
                        int sample = _options.ScaleNoise(NoiseShaped(phoneme.A1));
                        int scaled = sample * (amplitude * AmplitudeReciprocal);
                        int outputSample = scaled >= 0
                            ? scaled >> AmplitudeShift
                            : -((-scaled) >> AmplitudeShift);
                        if (outputSample > 127)
                        {
                            outputSample = 127;
                        }
                        else if (outputSample < -127)
                        {
                            outputSample = -127;
                        }

                        _outputBuffer[_outputPosition++] = (byte)(outputSample + 128);
                        _outputTotal++;
                        if (_outputPosition == _outputBuffer.Length)
                        {
                            FlushOutput();
                        }
                    }
                }

                return;
            }

            EmitSilence(duration);
        }

        private void EmitSilence(int count)
        {
            while (count > 0)
            {
                int available = _outputBuffer.Length - _outputPosition;
                int blockLength = count;
                if (blockLength > available)
                {
                    blockLength = available;
                }

                if (blockLength > SilenceBlock.Length)
                {
                    blockLength = SilenceBlock.Length;
                }

                Array.Copy(SilenceBlock, 0, _outputBuffer, _outputPosition, blockLength);
                _outputPosition += blockLength;
                _outputTotal += blockLength;
                count -= blockLength;
                if (_outputPosition == _outputBuffer.Length)
                {
                    FlushOutput();
                }
            }
        }

        private void EmitWos(
            int f1,
            int f2,
            int f3,
            int a1,
            int a2,
            int a3,
            int pitchHz,
            int duration,
            int attack,
            int releaseStart)
        {
            int increment0 = PhaseIncrement(pitchHz);
            int increment1 = FormantIncrement(f1);
            int increment2 = FormantIncrement(f2);
            int increment3 = FormantIncrement(f3);
            int attackStep = RampStep(127, attack);
            int releaseStep = RampStep(127, duration - releaseStart);
            int phase0 = 0;
            int phase1 = 0;
            int phase2 = 0;
            int phase3 = 0;

            for (int i = 0; i < duration; i += ControlPeriod)
            {
                int blockLength = ControlBlockLength(i, duration);
                int amplitude = Envelope(
                    i,
                    duration,
                    attack,
                    releaseStart,
                    127,
                    attackStep,
                    releaseStep);
                for (int sampleIndex = 0; sampleIndex < blockLength; sampleIndex++)
                {
                    int previousPhase0 = phase0;
                    phase0 = (phase0 + increment0) & PhaseMask;
                    if (phase0 < previousPhase0)
                    {
                        phase1 = 0;
                        phase2 = 0;
                        phase3 = 0;
                    }

                    phase1 = (phase1 + increment1) & PhaseMask;
                    phase2 = (phase2 + increment2) & PhaseMask;
                    phase3 = (phase3 + increment3) & PhaseMask;
                    int rectangle = phase3 >= PhaseHalf ? 64 : -64;
                    int mixed = (RendererData.Sine[phase1 >> PhaseIndexShift] * a1)
                        + (RendererData.Sine[phase2 >> PhaseIndexShift] * a2)
                        + (rectangle * a3);
                    int sample = mixed >= 0 ? mixed >> 5 : -((-mixed) >> 5);
                    int scaled = sample * (amplitude * AmplitudeReciprocal);
                    int outputSample = scaled >= 0
                        ? scaled >> AmplitudeShift
                        : -((-scaled) >> AmplitudeShift);
                    if (outputSample > 127)
                    {
                        outputSample = 127;
                    }
                    else if (outputSample < -127)
                    {
                        outputSample = -127;
                    }

                    _outputBuffer[_outputPosition++] = (byte)(outputSample + 128);
                    _outputTotal++;
                    if (_outputPosition == _outputBuffer.Length)
                    {
                        FlushOutput();
                    }
                }
            }
        }

        private void FlushOutput()
        {
            if (_outputPosition == 0)
            {
                return;
            }

            _sink.Write(_outputBuffer, 0, _outputPosition);
            _outputPosition = 0;
        }

        private int NoiseShaped(int mode)
        {
            int input = Noise();
            int output;
            if (mode <= 0)
            {
                _lowPass1 += ShiftTowardZero(input - _lowPass1, 1);
                _lowPass2 += ShiftTowardZero(_lowPass1 - _lowPass2, 1);
                output = _lowPass2 * 2;
            }
            else if (mode == 1)
            {
                output = input;
            }
            else
            {
                output = ShiftTowardZero(input - _previousNoise, 1);
            }

            _previousNoise = input;
            return ClampSample(output);
        }

        private int Noise()
        {
            int leastSignificantBit = _lfsr & 1;
            _lfsr = (ushort)(_lfsr >> 1);
            if (leastSignificantBit != 0)
            {
                _lfsr ^= 0xB400;
            }

            int value = _lfsr >> 8;
            return value >= 128 ? value - 256 : value;
        }
    }
}
