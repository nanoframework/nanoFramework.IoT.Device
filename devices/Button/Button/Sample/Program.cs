using Iot.Device.Button;
using System.Diagnostics;
using System.Threading;

GpioButton button = new GpioButton();

Debug.WriteLine("Button is initialized, starting to read state");

button.IsDoublePressEnabled = true;
button.IsHoldingEnabled = true;

button.ButtonDown += (sender, e) =>
{
    Debug.WriteLine($"buttondown IsPressed={button.IsPressed}");
};

button.ButtonUp += (sender, e) =>
{
    Debug.WriteLine($"buttonup IsPressed={button.IsPressed}");
};

button.Press += (sender, e) =>
{
    Debug.WriteLine($"Press");
};

button.DoublePress += (sender, e) =>
{
    Debug.WriteLine($"Double press");
};

button.Holding += (sender, e) =>
{
    switch (e.HoldingState)
    {
        case ButtonHoldingState.Started:
            Debug.WriteLine($"Holding Started");
            break;
        case ButtonHoldingState.Completed:
            Debug.WriteLine($"Holding Completed");
            break;
    }
};

Thread.Sleep(Timeout.Infinite);
