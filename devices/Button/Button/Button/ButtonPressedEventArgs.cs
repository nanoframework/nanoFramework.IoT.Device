namespace Iot.Device.Button
{
    /// <summary>
    /// Notifies about a the channel statuses have been changed.
    /// Refresh period can be changed by setting PeriodRefresh property.
    /// </summary>
    /// <param name="sender">The sender MPR121</param>
    /// <param name="e">The even arguments</param>
    public delegate void ButtonClicked(object sender, ButtonClickedEventArgs e);

    /// <summary>
    /// Represents the arguments of event rising when the channel statuses have been changed.
    /// </summary>
    public class ButtonClickedEventArgs
    {
        /// <summary>
        /// The channel statuses.
        /// </summary>
        public string ButtonStatus { get; private set; }

        /// <summary>
        /// Initialize event arguments.
        /// </summary>
        /// <param name="channelStatuses">The channel statuses.</param>
        public ButtonClickedEventArgs(string buttonStatus)
            : base()
        {
            ButtonStatus = buttonStatus;
        }
    }
}
