using Iot.Device.Button;
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
            string buttonStatus = null;
            Button button = new Button();
            button.OnButtonClicked += delegate (object sender, ButtonClickedEventArgs e)
            {
                buttonStatus = e.ButtonStatus;
            };

            //button.ButtonStateChangedSinglePress(button, new PinValueChangedEventArgs(changeType: PinEventTypes.Rising, 0));

            Assert.NotNull(buttonStatus);
            Assert.Equals(buttonStatus, "click");
        }
    }
}
