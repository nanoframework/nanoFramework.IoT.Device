using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapKeypressNotificationEventArgs.
    /// </summary>
    public class GapKeypressNotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the underlying Pairing.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Type of Keypress input notified/signaled by peer device.
        /// </summary>
        public readonly KeyPressNotificationType NotificationType;

        internal GapKeypressNotificationEventArgs(ushort connectionHandle, KeyPressNotificationType notificationType)
        {
            ConnectionHandle = connectionHandle;
            NotificationType = notificationType;
        }
    }
}
