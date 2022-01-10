// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Helper to get the enumeration description instead of enumeration name
    /// </summary>
    internal static class EnumHelper
    {
        // This doens't work at all in nano, we have to implement a total different way
        ///// <summary>
        ///// Returns the description attribute or the name of the enum
        ///// </summary>
        ///// <typeparam name = "T" > A valid enumeration</typeparam>
        ///// <param name = "enumerationValue" > The type of enumeration</param>
        ///// <returns>The description attribute or name if not existing</returns>
        ////public static string GetDescription<T>(this T enumerationValue)
        ////    where T : struct
        ////{
        ////    if (!enumerationValue.GetType().IsEnum)
        ////    {
        ////        throw new ArgumentException($"EnumerationValue {nameof(enumerationValue)} must be of Enum type");
        ////    }

        ////    var enumVal = enumerationValue.ToString();
        ////    enumVal = enumVal ?? string.Empty;

        ////    MemberInfo[]? memberInfo = enumerationValue.GetType().GetMember(enumVal);
        ////    if (memberInfo != null && memberInfo?.Length > 0)
        ////    {
        ////        object[] attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

        ////        if (attributes != null && attributes?.Length > 0)
        ////        {
        ////            return the description
        ////            return ((DescriptionAttribute)attributes[0]).Description;
        ////        }
        ////    }

        ////    Return just the name if attribute can't be found
        ////    return enumVal;
        ////}

        public static string GetDescription(UriType uriType, string uri)
        {
            switch (uriType)
            {
                default:
                case UriType.NoFormat:
                    return $"{uri}";
                case UriType.HttpWww:
                    return $"http://www.{uri}";
                case UriType.HttpsWww:
                    return $"https://www.{uri}";
                case UriType.Http:
                    return $"http://{uri}";
                case UriType.Https:
                    return $"https://{uri}";
                case UriType.Tel:
                    return $"tel:{uri}";
                case UriType.MailTo:
                    return $"mailto:{uri}";
                case UriType.FtpAnonymousAnonymous:
                    return $"ftp://anonymous:anonymous@{uri}";
                case UriType.FtpFtp:
                    return $"ftp://ftp.{uri}";
                case UriType.Ftps:
                    return $"ftps://{uri}";
                case UriType.Sftp:
                    return $"sftp://{uri}";
                case UriType.Smb:
                    return $"smb://{uri}";
                case UriType.Nfs:
                    return $"nfs://{uri}";
                case UriType.Ftp:
                    return $"ftp://{uri}";
                case UriType.Dav:
                    return $"dav://{uri}";
                case UriType.News:
                    return $"news:{uri}";
                case UriType.Telnet:
                    return $"telnet://{uri}";
                case UriType.Imap:
                    return $"imap:{uri}";
                case UriType.Rtsp:
                    return $"rtsp://{uri}";
                case UriType.Urn:
                    return $"urn:{uri}";
                case UriType.Pop:
                    return $"pop:{uri}";
                case UriType.Sip:
                    return $"sip:{uri}";
                case UriType.Sips:
                    return $"sips:{uri}";
                case UriType.Tftp:
                    return $"tftp:{uri}";
                case UriType.Btspp:
                    return $"btspp://{uri}";
                case UriType.Btl2Cap:
                    return $"btl2cap://{uri}";
                case UriType.Btgoep:
                    return $"btgoep://{uri}";
                case UriType.Tcpobex:
                    return $"tcpobex://{uri}";
                case UriType.Irdaobex:
                    return $"irdaobex://{uri}";
                case UriType.File:
                    return $"file://{uri}";
                case UriType.UrnEpcId:
                    return $"urn:epc:id:{uri}";
                case UriType.UrnEpcTag:
                    return $"urn:epc:tag:{uri}";
                case UriType.UrnEpcPat:
                    return $"urn:epc:pat:{uri}";
                case UriType.UrnEpcRaw:
                    return $"urn:epc:raw:{uri}";
                case UriType.UrnEpc:
                    return $"urn:epc:{uri}";
                case UriType.UrnNfc:
                    return $"urn:nfc:{uri}";
            }
        }
    }
}
