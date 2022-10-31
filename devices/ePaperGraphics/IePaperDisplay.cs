using System;

using Iot.Device.ePaperGraphics;

namespace Iot.Device.ePaper
{
    public interface IePaperDisplay : IDisposable
    {
        int Width { get; }

        int Height { get; }


        void PerformFullRefresh();

        void PerformPartialRefresh();


        void Clear(bool triggerPageRefresh = false);


        void SetPosition(int x, int y);

        void DrawPixel(int x, int y, bool inverted);

        void DrawPixel(int x, int y, Color color);

        void DrawBuffer(int startXPos, int startYPos, params byte[] bitmap);

        void DirectDrawBuffer(int startXPos, int startYPos, params byte[] bitmap);


        void BeginFrameDraw();

        bool NextFramePage();

        void EndFrameDraw();


        void SendCommand(params byte[] command);

        void SendData(params byte[] data);

        void WaitReady();
    }
}
