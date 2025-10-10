// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer.Options
{
    internal class ParameterRequestListOption : Option
    {
        private string? _valueAsString;

        public static bool IsKnownOption(byte code) => IsKnownOption((DhcpOptionCode)code);

        public static bool IsKnownOption(DhcpOptionCode code) => DhcpOptionCode.ParameterList == code;

        public ParameterRequestListOption(byte[] data) : base(DhcpOptionCode.ParameterList, data)
        {
        }

        public byte[] Deserialize()
        {
            return Data;
        }

        public override string ToString()
        {
            if (_valueAsString is null)
            {
                var stringBuilder = new StringBuilder(Length);
                var started = false;

                stringBuilder.Append('{');

                foreach (var b in Data)
                {
                    if (started)
                    {
                        stringBuilder.Append(',');
                    }

                    started = true;
                    stringBuilder.Append(b);
                }

                stringBuilder.Append('}');

                _valueAsString = stringBuilder.ToString();
            }

            return ToString(_valueAsString);
        }
    }
}
