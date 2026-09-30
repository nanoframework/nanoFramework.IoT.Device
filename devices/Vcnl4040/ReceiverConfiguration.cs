// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040
{
    /// <summary>Defines the proximity sensor receiver configuration.</summary>
    public class ReceiverConfiguration
    {
        /// <summary>Initializes a new instance of the <see cref="ReceiverConfiguration"/> class.</summary>
        /// <param name="extendedOutputRange">The extendedOutputRange value.</param>
        /// <param name="cancellationLevel">The cancellationLevel value.</param>
        /// <param name="whiteChannelEnabled">The whiteChannelEnabled value.</param>
        /// <param name="sunlightCancellationEnabled">The sunlightCancellationEnabled value.</param>
        public ReceiverConfiguration(bool extendedOutputRange, ushort cancellationLevel, bool whiteChannelEnabled, bool sunlightCancellationEnabled)
        {
            ExtendedOutputRange = extendedOutputRange;
            CancellationLevel = cancellationLevel;
            WhiteChannelEnabled = whiteChannelEnabled;
            SunlightCancellationEnabled = sunlightCancellationEnabled;
        }

        /// <summary>Gets a value indicating whether the 16-bit output range is enabled.</summary>
        public bool ExtendedOutputRange { get; private set; }

        /// <summary>Gets the receiver cancellation level.</summary>
        public ushort CancellationLevel { get; private set; }

        /// <summary>Gets a value indicating whether the white channel is enabled.</summary>
        public bool WhiteChannelEnabled { get; private set; }

        /// <summary>Gets a value indicating whether sunlight cancellation is enabled.</summary>
        public bool SunlightCancellationEnabled { get; private set; }
    }
}
