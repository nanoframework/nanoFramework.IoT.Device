using Iot.Device.Button;
using Iot.Device.Tests;
using nanoFramework.TestFramework;

// DOES NOT WORK YET
namespace Tests
{
    [TestClass]
    class Buttontests
    {
        [TestMethod]
        public void TestOne()
        {

            TestButton button = new TestButton();

            bool IsPressed = false;

            button.Click += (sender, e) =>
            {
                IsPressed = true;
            };

            button.PressButton();
            button.ReleaseButton();

            Assert.Equals(IsPressed, true);
        }
    }
}
