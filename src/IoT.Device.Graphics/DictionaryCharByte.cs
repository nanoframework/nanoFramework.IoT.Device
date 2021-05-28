using System;
using System.Collections;

namespace Iot.Device.Graphics
{
    internal class DictionaryCharByte
    {
        ArrayList _array = new ArrayList();
        public DictionaryCharByte()
        { }

        public void Add(CharByte cb)
        {
            _array.Add(cb);
        }

        public void Add(char cr, byte bt)
        {
            _array.Add(new CharByte(cr, bt));
        }

        public bool TryAdd(char cr, byte bt)
        {
            foreach (CharByte cb in _array)
            {
                if (cb.Cr == cr)
                {
                    return false;
                }
            }

            _array.Add(new CharByte(cr, bt));
            return true;
        }


        public bool TryGetValue(char cr, out byte val)
        {
            foreach (CharByte cb in _array)
            {
                if (cb.Cr == cr)
                {
                    val = cb.Bt;
                    return true;
                }
            }

            val = 0;
            return false;
        }

        public bool ContainsKey(char cr)
        {
            foreach (CharByte cb in _array)
            {
                if (cb.Cr == cr)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
