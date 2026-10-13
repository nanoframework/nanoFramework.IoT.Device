// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100.Samples.Esp32S3BoxLite
{
    internal partial class Resource
    {
        private static System.Resources.ResourceManager _resourceManager;

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = new System.Resources.ResourceManager(
                        "Iot.Device.Max30100.Samples.Esp32S3BoxLite.Resource",
                        typeof(Resource).Assembly);
                }

                return _resourceManager;
            }
        }

        internal static nanoFramework.UI.Font GetFont(FontResources id)
        {
            return (nanoFramework.UI.Font)nanoFramework.Runtime.Native.ResourceUtility.GetObject(ResourceManager, id);
        }

        [System.Serializable]
        internal enum FontResources : short
        {
            Consolas23 = -31666,
            NinaB = 18060,
            segoeuiregular12 = 21930,
        }
    }
}
