using System;

namespace Iot.Device.ePaper
{
    public interface IePaperDisplay : IDisposable
    {
        int Width { get; }

        int Height { get; }

        PowerState PowerState { get; }


        void PerformFullRefresh();

        void PerformPartialRefresh();


        void SetPosition(int x, int y);


        void SendCommand(params byte[] command);

        void SendData(params byte[] data);

        void WaitReady();
    }
}
