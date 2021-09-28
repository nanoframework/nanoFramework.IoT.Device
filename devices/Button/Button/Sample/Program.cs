using Iot.Device.Button;
using System.Diagnostics;
using System.Threading;

using Button button = new Button();

Debug.WriteLine("Button is initialized, starting to read state");

button.Click += (sender, e) =>
{
    Debug.WriteLine("click");
};

button.DoubleClick += (sender, e) =>
{
    Debug.WriteLine("double click");
};

button.Holding += (sender, e) =>
{
    Debug.WriteLine($"{e.HoldingState}");
};

Thread.Sleep(Timeout.Infinite);
