using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace IoT.Device.Graphics
{
    /// <summary>
    /// Graphics Extensions for nanoFramework
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Tries to get the value from an integer
        /// </summary>
        /// <param name="table">The table</param>
        /// <param name="character">The character</param>
        /// <param name="index">The index</param>
        /// <returns>True or False</returns>
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
