using Iot.Device.Button;
using System.Diagnostics;
using System.Threading;

GpioButton button = new GpioButton();

Debug.WriteLine("Button is initialized, starting to read state");

button.IsDoubleClickEnabled = true;
button.IsHoldingEnabled = true;

//button.ButtonDown += (sender, e) =>
//{
//    Debug.WriteLine($"buttondown IsPressed={button.IsPressed}");
//};

//button.ButtonUp += (sender, e) =>
//{
//    Debug.WriteLine($"buttonup IsPressed={button.IsPressed}");
//};

button.Click += (sender, e) =>
{
    Debug.WriteLine($"click");
};

button.DoubleClick += (sender, e) =>
{
    Debug.WriteLine($"double click");
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
