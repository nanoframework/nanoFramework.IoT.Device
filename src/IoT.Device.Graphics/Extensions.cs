using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IoT.Device.Graphics
{
    public static class GraphicsExtensions
    {
        public static bool TryGetValue(this Hashtable table, int character, out int index)
        {
            for (int i = 0; i < table.Count; i++)
            {
                if ((int)table[i] == character)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}
