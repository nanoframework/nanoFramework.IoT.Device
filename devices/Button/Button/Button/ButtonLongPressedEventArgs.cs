namespace Iot.Device.Button
{
    /// <summary>
    /// Notifies about a the channel statuses have been changed.
    /// Refresh period can be changed by setting PeriodRefresh property.
    /// </summary>
    /// <param name="sender">The sender MPR121</param>
    /// <param name="e">The even arguments</param>
    public delegate void ButtonLongClicked(object sender, ButtonLongClickedEventArgs e);

    public class ButtonLongClickedEventArgs
    {
        /// <summary>
        /// The channel statuses.
        /// </summary>
        public string ButtonStatus { get; private set; }

        /// <summary>
        /// Initialize event arguments.
        /// </summary>
        /// <param name="channelStatuses">The channel statuses.</param>
        public ButtonLongClickedEventArgs(string buttonStatus)
            : base()
        {
            ButtonStatus = buttonStatus;
        }
    }
}
