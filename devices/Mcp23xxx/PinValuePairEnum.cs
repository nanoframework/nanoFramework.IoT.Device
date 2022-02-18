using System;
using System.Collections;

namespace Iot.Device.Mcp23xxx
{
    public class PinValuePairEnum : IEnumerator
    {
        public Iot.Device.Mcp23xxx.PinValuePair[] _pair;
        int currentIndex = -1;
        public PinValuePairEnum(Iot.Device.Mcp23xxx.PinValuePair[] list)
        {
            _pair = list;
        }
        public bool MoveNext()
        {
            currentIndex++;
            return (currentIndex < _pair.Length);
        }
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }
        public Iot.Device.Mcp23xxx.PinValuePair Current
        {
            get
            {
                try
                {
                    return _pair[currentIndex];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public void Reset()
        {
            currentIndex = -1;
        }
    }
}
