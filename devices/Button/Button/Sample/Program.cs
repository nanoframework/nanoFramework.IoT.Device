using Iot.Device.Button;
using System.Diagnostics;
using System.Threading;

using Button button = new Button();

Debug.WriteLine("Button is initialized, starting to read state");

button.IsDoubleClickEnabled = true;
button.IsHoldingEnabled = true;

button.ButtonDown += (sender, e) =>
{
    Debug.WriteLine($"buttondown IsPressed={button.IsPressed}");
};

button.ButtonUp += (sender, e) =>
{
    Debug.WriteLine($"buttonup IsPressed={button.IsPressed}");
};

button.Click += (sender, e) =>
{
    Debug.WriteLine($"click IsPressed={button.IsPressed}");
};

button.DoubleClick += (sender, e) =>
{
    Debug.WriteLine($"double click IsPressed={button.IsPressed}");
};

button.Holding += (sender, e) =>
{
    switch (e.HoldingState)
    {
        case ButtonHoldingState.Started:
            Debug.WriteLine($"Holding Started IsPressed={button.IsPressed}");
            break;
        case ButtonHoldingState.Completed:
            Debug.WriteLine($"Holding Completed IsPressed={button.IsPressed}");
            break;
    }
};

Thread.Sleep(Timeout.Infinite);
