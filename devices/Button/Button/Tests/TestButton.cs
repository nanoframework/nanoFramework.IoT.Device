using Iot.Device.Button;
using System;

namespace Iot.Device.Tests
{
    public class TestButton : ButtonBase
    {
        public TestButton() : base()
        {

        }
        public void PressButton()
        {
            HandleButtonPressed();
        }

        public void ReleaseButton()
        {
            HandleButtonReleased();
        }
    }
}
