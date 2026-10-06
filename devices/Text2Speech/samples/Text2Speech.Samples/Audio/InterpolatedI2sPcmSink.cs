// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2s;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Text2Speech.Samples
{
    /// <summary>
    /// Queues PCM and converts it into interpolated signed 16-bit stereo I2S frames.
    /// </summary>
    internal sealed class InterpolatedI2sPcmSink : IPcmSink
    {
        private const int QueueBlockSize = 2048;
        private const int QueueBlockCount = 8;
        private const int StartupBlockCount = 7;
        private const int HeartbeatIntervalMilliseconds = 50;

        private readonly I2sDevice _i2sDevice;
        private readonly bool _interpolate;
        private readonly byte[][] _queue = new byte[QueueBlockCount][];
        private readonly int[] _queueCounts = new int[QueueBlockCount];
        private readonly byte[] _output;
        private readonly object _syncRoot = new object();
        private readonly AutoResetEvent _dataAvailable = new AutoResetEvent(false);
        private readonly AutoResetEvent _spaceAvailable = new AutoResetEvent(false);
        private Thread _playbackThread;
        private Thread _heartbeatThread;
        private Exception _playbackException;
        private int _readIndex;
        private int _writeIndex;
        private int _queuedBlocks;
        private int _outputLength;
        private short _previousSample;
        private bool _hasPreviousSample;
        private bool _producerCompleted;
        private bool _playbackStarted;
        private bool _aborted;
        private bool _diagnosticsEnabled;
        private bool _heartbeatStopped;
        private int _heartbeatCount;
        private int _sourceWriteCount;
        private int _sourceSamplesQueued;
        private int _maximumQueueDepth;
        private int _producerWaitCount;
        private long _producerWaitTicks;
        private int _consumerWaitCount;
        private long _consumerWaitTicks;
        private int _i2sWriteCount;
        private long _i2sWriteTicks;
        private long _maximumI2sWriteTicks;
        private int _conversionCount;
        private long _conversionTicks;
        private long _maximumConversionTicks;
        private int _heartbeatsDuringI2sWrites;
        private int _samplesQueuedDuringI2sWrites;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterpolatedI2sPcmSink" /> class.
        /// </summary>
        /// <param name="i2sDevice">The initialized I2S output device.</param>
        /// <param name="interpolate">Whether to insert midpoints for 16 kHz playback.</param>
        public InterpolatedI2sPcmSink(I2sDevice i2sDevice, bool interpolate)
        {
            _i2sDevice = i2sDevice;
            _interpolate = interpolate;
            _output = new byte[interpolate ? 16384 : 8192];
            for (int i = 0; i < _queue.Length; i++)
            {
                _queue[i] = new byte[QueueBlockSize];
            }
        }

        /// <summary>
        /// Resets interpolation and starts the playback consumer for a new utterance.
        /// </summary>
        /// <param name="enableDiagnostics">Whether to measure queue and native I2S activity.</param>
        /// <exception cref="InvalidOperationException">The previous I2S playback is still active.</exception>
        public void Reset(bool enableDiagnostics = false)
        {
            if (_playbackThread != null && _playbackThread.IsAlive)
            {
                throw new InvalidOperationException();
            }

            _playbackException = null;
            _readIndex = 0;
            _writeIndex = 0;
            _queuedBlocks = 0;
            _outputLength = 0;
            _previousSample = 0;
            _hasPreviousSample = false;
            _producerCompleted = false;
            _playbackStarted = false;
            _aborted = false;
            _diagnosticsEnabled = enableDiagnostics;
            _heartbeatStopped = false;
            _heartbeatCount = 0;
            _sourceWriteCount = 0;
            _sourceSamplesQueued = 0;
            _maximumQueueDepth = 0;
            _producerWaitCount = 0;
            _producerWaitTicks = 0;
            _consumerWaitCount = 0;
            _consumerWaitTicks = 0;
            _i2sWriteCount = 0;
            _i2sWriteTicks = 0;
            _maximumI2sWriteTicks = 0;
            _conversionCount = 0;
            _conversionTicks = 0;
            _maximumConversionTicks = 0;
            _heartbeatsDuringI2sWrites = 0;
            _samplesQueuedDuringI2sWrites = 0;

            _dataAvailable.Reset();
            _spaceAvailable.Reset();
            _playbackThread = new Thread(PlaybackWorker);
            _playbackThread.Priority = ThreadPriority.BelowNormal;
            _playbackThread.Start();
            if (_diagnosticsEnabled)
            {
                _heartbeatThread = new Thread(HeartbeatWorker);
                _heartbeatThread.Priority = ThreadPriority.BelowNormal;
                _heartbeatThread.Start();
            }
        }

        /// <summary>
        /// Copies a source PCM block into the bounded playback queue.
        /// </summary>
        /// <param name="buffer">The unsigned 8-bit mono PCM source.</param>
        /// <param name="offset">The source offset.</param>
        /// <param name="count">The number of source samples.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            _sourceWriteCount++;
            while (count > 0)
            {
                int blockCount = count < QueueBlockSize ? count : QueueBlockSize;
                bool queued = false;
                while (!queued)
                {
                    lock (_syncRoot)
                    {
                        ThrowPlaybackException();
                        if (_queuedBlocks < QueueBlockCount)
                        {
                            Array.Copy(buffer, offset, _queue[_writeIndex], 0, blockCount);
                            _queueCounts[_writeIndex] = blockCount;
                            _writeIndex = (_writeIndex + 1) % QueueBlockCount;
                            _queuedBlocks++;
                            _sourceSamplesQueued += blockCount;
                            if (_queuedBlocks > _maximumQueueDepth)
                            {
                                _maximumQueueDepth = _queuedBlocks;
                            }

                            queued = true;
                        }
                    }

                    if (queued)
                    {
                        _dataAvailable.Set();
                    }
                    else
                    {
                        long waitStarted = _diagnosticsEnabled ? DateTime.UtcNow.Ticks : 0;
                        _spaceAvailable.WaitOne();
                        if (_diagnosticsEnabled)
                        {
                            _producerWaitCount++;
                            _producerWaitTicks += DateTime.UtcNow.Ticks - waitStarted;
                        }
                    }
                }

                offset += blockCount;
                count -= blockCount;
            }
        }

        /// <summary>
        /// Finishes the queue and waits until all generated samples have played.
        /// </summary>
        public void Complete()
        {
            lock (_syncRoot)
            {
                _producerCompleted = true;
            }

            _dataAvailable.Set();
            _playbackThread.Join();
            StopHeartbeat();
            ThrowPlaybackException();
            WriteDiagnostics();
        }

        /// <summary>
        /// Stops a pending playback after synthesis or input fails.
        /// </summary>
        public void Abort()
        {
            lock (_syncRoot)
            {
                _aborted = true;
                _queuedBlocks = 0;
            }

            _dataAvailable.Set();
            _spaceAvailable.Set();
            if (_playbackThread != null)
            {
                _playbackThread.Join();
            }

            StopHeartbeat();
        }

        private void PlaybackWorker()
        {
            try
            {
                while (true)
                {
                    byte[] block = null;
                    int count = 0;
                    bool completed = false;
                    lock (_syncRoot)
                    {
                        if (_aborted)
                        {
                            return;
                        }

                        if (_producerCompleted && _queuedBlocks == 0)
                        {
                            completed = true;
                        }
                        else if (CanConsumeBlock())
                        {
                            _playbackStarted = true;
                            block = _queue[_readIndex];
                            count = _queueCounts[_readIndex];
                        }
                    }

                    if (completed)
                    {
                        break;
                    }

                    if (block == null)
                    {
                        long waitStarted = _diagnosticsEnabled ? DateTime.UtcNow.Ticks : 0;
                        _dataAvailable.WaitOne();
                        if (_diagnosticsEnabled)
                        {
                            _consumerWaitCount++;
                            _consumerWaitTicks += DateTime.UtcNow.Ticks - waitStarted;
                        }

                        continue;
                    }

                    if (_diagnosticsEnabled)
                    {
                        long conversionStarted = DateTime.UtcNow.Ticks;
                        ConvertAndWrite(block, count);
                        long conversionElapsed = DateTime.UtcNow.Ticks - conversionStarted;
                        _conversionCount++;
                        _conversionTicks += conversionElapsed;
                        if (conversionElapsed > _maximumConversionTicks)
                        {
                            _maximumConversionTicks = conversionElapsed;
                        }
                    }
                    else
                    {
                        ConvertAndWrite(block, count);
                    }

                    lock (_syncRoot)
                    {
                        _readIndex = (_readIndex + 1) % QueueBlockCount;
                        _queuedBlocks--;
                    }

                    _spaceAvailable.Set();
                    Thread.Sleep(0);
                }

                CompleteOutput();
            }
            catch (Exception ex)
            {
                lock (_syncRoot)
                {
                    _playbackException = ex;
                }

                _dataAvailable.Set();
                _spaceAvailable.Set();
            }
        }

        private bool CanConsumeBlock()
        {
            return _queuedBlocks > 0
                && (_playbackStarted
                    || _producerCompleted
                    || _queuedBlocks >= StartupBlockCount);
        }

        private void ConvertAndWrite(byte[] buffer, int count)
        {
            if (!_interpolate)
            {
                ConvertFastAndWrite(buffer, count);
                return;
            }

            ConvertInterpolatedAndWrite(buffer, count);
        }

        private void ConvertFastAndWrite(byte[] buffer, int count)
        {
            byte[] output = _output;
            int outputLength = _outputLength;
            for (int i = 0; i < count; i++)
            {
                if (outputLength + 4 > output.Length)
                {
                    _outputLength = outputLength;
                    Flush();
                    outputLength = 0;
                }

                byte sampleHigh = (byte)(buffer[i] - 128);
                output[outputLength++] = 0;
                output[outputLength++] = sampleHigh;
                output[outputLength++] = 0;
                output[outputLength++] = sampleHigh;
            }

            _outputLength = outputLength;
        }

        private void ConvertInterpolatedAndWrite(byte[] buffer, int count)
        {
            byte[] output = _output;
            int outputLength = _outputLength;
            short previousSample = _previousSample;
            bool hasPreviousSample = _hasPreviousSample;

            for (int i = 0; i < count; i++)
            {
                short currentSample = (short)((buffer[i] - 128) << 8);
                if (hasPreviousSample)
                {
                    if (outputLength + 8 > output.Length)
                    {
                        _outputLength = outputLength;
                        Flush();
                        outputLength = 0;
                    }

                    byte previousLow = (byte)previousSample;
                    byte previousHigh = (byte)(previousSample >> 8);
                    output[outputLength++] = previousLow;
                    output[outputLength++] = previousHigh;
                    output[outputLength++] = previousLow;
                    output[outputLength++] = previousHigh;

                    int midpointSum = previousSample + currentSample;
                    short midpoint = (short)(
                        (midpointSum + ((midpointSum >> 31) & 1)) >> 1);
                    byte midpointLow = (byte)midpoint;
                    byte midpointHigh = (byte)(midpoint >> 8);
                    output[outputLength++] = midpointLow;
                    output[outputLength++] = midpointHigh;
                    output[outputLength++] = midpointLow;
                    output[outputLength++] = midpointHigh;
                }

                previousSample = currentSample;
                hasPreviousSample = true;
            }

            _outputLength = outputLength;
            _previousSample = previousSample;
            _hasPreviousSample = hasPreviousSample;
        }

        private void CompleteOutput()
        {
            if (_interpolate && _hasPreviousSample)
            {
                WriteStereoFrame(_previousSample);
                WriteStereoFrame(_previousSample);
            }

            Flush();
        }

        private void WriteStereoFrame(short sample)
        {
            if (_outputLength + 4 > _output.Length)
            {
                Flush();
            }

            byte low = (byte)sample;
            byte high = (byte)(sample >> 8);
            _output[_outputLength++] = low;
            _output[_outputLength++] = high;
            _output[_outputLength++] = low;
            _output[_outputLength++] = high;
        }

        private void Flush()
        {
            if (_outputLength == 0)
            {
                return;
            }

            SpanByte outputSpan = new SpanByte(_output);
            if (!_diagnosticsEnabled)
            {
                _i2sDevice.Write(
                    _outputLength == _output.Length
                        ? outputSpan
                        : outputSpan.Slice(0, _outputLength));
                _outputLength = 0;
                return;
            }

            int heartbeatBefore = _heartbeatCount;
            int samplesBefore = _sourceSamplesQueued;
            long started = DateTime.UtcNow.Ticks;
            _i2sDevice.Write(
                _outputLength == _output.Length
                    ? outputSpan
                    : outputSpan.Slice(0, _outputLength));
            long elapsed = DateTime.UtcNow.Ticks - started;

            _i2sWriteCount++;
            _i2sWriteTicks += elapsed;
            if (elapsed > _maximumI2sWriteTicks)
            {
                _maximumI2sWriteTicks = elapsed;
            }

            _heartbeatsDuringI2sWrites += _heartbeatCount - heartbeatBefore;
            _samplesQueuedDuringI2sWrites += _sourceSamplesQueued - samplesBefore;
            _outputLength = 0;
        }

        private void HeartbeatWorker()
        {
            while (!_heartbeatStopped)
            {
                Thread.Sleep(HeartbeatIntervalMilliseconds);
                _heartbeatCount++;
            }
        }

        private void StopHeartbeat()
        {
            if (_heartbeatThread == null)
            {
                return;
            }

            _heartbeatStopped = true;
            _heartbeatThread.Join();
            _heartbeatThread = null;
        }

        private void WriteDiagnostics()
        {
            if (!_diagnosticsEnabled)
            {
                return;
            }

            long i2sMilliseconds = _i2sWriteTicks / TimeSpan.TicksPerMillisecond;
            long maximumI2sMilliseconds = _maximumI2sWriteTicks / TimeSpan.TicksPerMillisecond;
            long averageI2sMilliseconds = _i2sWriteCount > 0
                ? i2sMilliseconds / _i2sWriteCount
                : 0;
            long conversionMilliseconds = _conversionTicks / TimeSpan.TicksPerMillisecond;
            long maximumConversionMilliseconds =
                _maximumConversionTicks / TimeSpan.TicksPerMillisecond;
            Debug.WriteLine(
                "[i2s-diagnostic] mode="
                + (_interpolate ? "interpolated-16khz" : "fast-8khz")
                + ", source-writes=" + _sourceWriteCount.ToString()
                + ", samples=" + _sourceSamplesQueued.ToString()
                + ", max-queue=" + _maximumQueueDepth.ToString() + "/" + QueueBlockCount.ToString()
                + ", producer-waits=" + _producerWaitCount.ToString()
                + " (" + (_producerWaitTicks / TimeSpan.TicksPerMillisecond).ToString() + " ms)"
                + ", consumer-waits=" + _consumerWaitCount.ToString()
                + " (" + (_consumerWaitTicks / TimeSpan.TicksPerMillisecond).ToString() + " ms)"
                + ", conversions=" + _conversionCount.ToString()
                + " (total=" + conversionMilliseconds.ToString() + " ms"
                + ", max=" + maximumConversionMilliseconds.ToString() + " ms)"
                + ", i2s-writes=" + _i2sWriteCount.ToString()
                + " (total=" + i2sMilliseconds.ToString() + " ms"
                + ", average=" + averageI2sMilliseconds.ToString() + " ms"
                + ", max=" + maximumI2sMilliseconds.ToString() + " ms)"
                + ", managed-heartbeats-during-i2s=" + _heartbeatsDuringI2sWrites.ToString()
                + ", samples-queued-during-i2s=" + _samplesQueuedDuringI2sWrites.ToString() + ".");
        }

        private void ThrowPlaybackException()
        {
            if (_playbackException != null)
            {
                throw _playbackException;
            }
        }
    }
}
