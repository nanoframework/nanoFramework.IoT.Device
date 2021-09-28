using Iot.Device.Button;
using Iot.Device.Tests;
using nanoFramework.TestFramework;
using System.Threading;

// DOES NOT WORK YET
namespace Tests
{
    [TestClass]
    class Buttontests
    {
        [TestMethod]
        public void Test_click()
        {
            bool IsPressed = false;

            TestButton button = new TestButton();
            button.Click += (sender, e) =>
            {
                IsPressed = true;
            };

            button.PressButton();
            Thread.Sleep(200);
            button.ReleaseButton();

            Assert.Equals(IsPressed, false);
        }

        [TestMethod]
        public void Test_double_click()
        {
            TestButton button = new TestButton();
            button.IsDoubleClickEnabled = true;
            bool IsPressed = false;

            button.DoubleClick += (sender, e) =>
            {
                IsPressed = true;
            };

            button.PressButton();
            Thread.Sleep(200);
            button.ReleaseButton();

            Thread.Sleep(400);

            button.PressButton();
            Thread.Sleep(200);
            button.ReleaseButton();

            Assert.Equals(IsPressed, true);
        }
    }
}
