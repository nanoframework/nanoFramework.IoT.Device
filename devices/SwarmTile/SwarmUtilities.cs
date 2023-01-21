// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.//

namespace Iot.Device.Swarm
{
    internal class SwarmUtilities
    {
        public static OperationalQuality GetOperationalQuality(int backgroundNoiseRssi)
        {
            if (backgroundNoiseRssi >= -90)
            {
                return OperationalQuality.Bad;
            }
            else if (backgroundNoiseRssi >= -93)
            {
                return OperationalQuality.Marginal;
            }
            else if (backgroundNoiseRssi >= -97)
            {
                return OperationalQuality.OK;
            }
            else if (backgroundNoiseRssi >= -100)
            {
                return OperationalQuality.Good;
            }
            else if (backgroundNoiseRssi <= -105)
            {
                return OperationalQuality.Great;
            }
            else
            {
                return OperationalQuality.Unknown;
            }
        }
    }
}
