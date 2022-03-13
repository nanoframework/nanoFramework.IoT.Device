using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.Graphics
{
    internal static class FontExtensions
    {
        internal static bool StartsWith(this SpanChar spanCar, string toSearch)
        {
            bool found = true;
            for (int i = 0; i < toSearch.Length; i++)
            {
                if (spanCar[i] != toSearch[i])
                {
                    found = false;
                    break;
                }
            }

            return found;
        }
    }
}
