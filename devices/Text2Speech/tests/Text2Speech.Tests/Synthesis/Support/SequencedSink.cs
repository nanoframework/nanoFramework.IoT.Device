// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Verifies tee destinations are invoked in order.
    /// </summary>
    internal sealed class SequencedSink : IPcmSink
    {
        private readonly int _expectedPosition;
        private readonly SequenceState _state;

        internal SequencedSink(SequenceState state, int expectedPosition)
        {
            _state = state;
            _expectedPosition = expectedPosition;
        }

        internal int ByteCount { get; private set; }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            Assert.AreEqual(_expectedPosition, _state.Position, "Tee destination order");
            ByteCount += count;
            _state.Position++;
        }
    }
}
