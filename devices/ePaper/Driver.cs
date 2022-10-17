namespace Iot.Device.ePaper
{
    public abstract class Driver
    {
        public int Width { get; }

        public int Height { get; }

        public PowerState PowerState { get; }

        public abstract void PowerOn();

        public abstract void PowerDown();

        public abstract void Initialize();

        public abstract void PerformFullRefresh();

        public abstract void PerformPartialRefresh();


        public abstract void SetPosition(int x, int y);

        public abstract void Clear();


        public abstract void SendCommand(params byte[] command);

        public abstract void SendData(params byte[] data);
    }
}
