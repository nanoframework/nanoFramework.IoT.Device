using Iot.Device.Button;
using System.Diagnostics;
using System.Threading;

using Button button = new Button();

Debug.WriteLine("Button is initialized, starting to read state");

button.OnButtonClicked += (object sender, ButtonClickedEventArgs e) =>
{
    Debug.WriteLine(e.ButtonStatus);
};

button.OnButtonDoubleClicked += (object sender, ButtonDoubleClickedEventArgs e) =>
{
    Debug.WriteLine(e.ButtonStatus);
};

button.OnButtonLongClicked += (object sender, ButtonLongClickedEventArgs e) =>
{
    Debug.WriteLine(e.ButtonStatus);
};

Thread.Sleep(Timeout.Infinite);
