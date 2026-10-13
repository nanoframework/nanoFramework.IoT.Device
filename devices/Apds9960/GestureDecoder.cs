// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Apds9960
{
    internal static class GestureDecoder
    {
        private const int MinimumSignal = 10;
        private const int MaximumSignal = 254;
        private const int DirectionThreshold = 12;

        internal static Gesture Decode(byte[] fifoData, int dataSetCount)
        {
            if (!TryGetRatios(
                fifoData,
                dataSetCount,
                out int initialUpDownRatio,
                out int initialLeftRightRatio,
                out int finalUpDownRatio,
                out int finalLeftRightRatio))
            {
                return Gesture.None;
            }

            return DecodeDeltas(
                finalUpDownRatio - initialUpDownRatio,
                finalLeftRightRatio - initialLeftRightRatio);
        }

        internal static Gesture DecodeDeltas(int upDownDelta, int leftRightDelta)
        {
            int upDownMagnitude = Absolute(upDownDelta);
            int leftRightMagnitude = Absolute(leftRightDelta);

            if (upDownMagnitude < DirectionThreshold && leftRightMagnitude < DirectionThreshold)
            {
                return Gesture.None;
            }

            if (upDownMagnitude > leftRightMagnitude)
            {
                return upDownDelta < 0 ? Gesture.Up : Gesture.Down;
            }

            return leftRightDelta < 0 ? Gesture.Left : Gesture.Right;
        }

        internal static bool TryCalculateDeltas(
            byte[] fifoData,
            int dataSetCount,
            out int upDownDelta,
            out int leftRightDelta)
        {
            if (!TryGetRatios(
                fifoData,
                dataSetCount,
                out int initialUpDownRatio,
                out int initialLeftRightRatio,
                out int finalUpDownRatio,
                out int finalLeftRightRatio))
            {
                upDownDelta = 0;
                leftRightDelta = 0;
                return false;
            }

            upDownDelta = finalUpDownRatio - initialUpDownRatio;
            leftRightDelta = finalLeftRightRatio - initialLeftRightRatio;
            return true;
        }

        /// <summary>
        /// Calculates the initial and final directional ratios from gesture FIFO data.
        /// </summary>
        /// <param name="fifoData">The gesture FIFO data in up, down, left, and right order.</param>
        /// <param name="dataSetCount">The number of datasets in <paramref name="fifoData"/>.</param>
        /// <param name="initialUpDownRatio">The initial up/down ratio.</param>
        /// <param name="initialLeftRightRatio">The initial left/right ratio.</param>
        /// <param name="finalUpDownRatio">The final up/down ratio.</param>
        /// <param name="finalLeftRightRatio">The final left/right ratio.</param>
        /// <returns><see langword="true"/> when valid initial and final datasets were found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fifoData"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="dataSetCount"/> is outside the supported range, or <paramref name="fifoData"/> does not contain the requested number of datasets.
        /// </exception>
        internal static bool TryGetRatios(
            byte[] fifoData,
            int dataSetCount,
            out int initialUpDownRatio,
            out int initialLeftRightRatio,
            out int finalUpDownRatio,
            out int finalLeftRightRatio)
        {
            if (fifoData == null)
            {
                throw new ArgumentNullException();
            }

            if (dataSetCount < 0 || dataSetCount > 32 || fifoData.Length < dataSetCount * 4)
            {
                throw new ArgumentException();
            }

            int firstIndex = FindFirstValidDataSet(fifoData, dataSetCount);
            int lastIndex = FindLastValidDataSet(fifoData, dataSetCount);
            if (firstIndex < 0 || lastIndex < 0)
            {
                initialUpDownRatio = 0;
                initialLeftRightRatio = 0;
                finalUpDownRatio = 0;
                finalLeftRightRatio = 0;
                return false;
            }

            initialUpDownRatio = CalculateRatio(fifoData[firstIndex], fifoData[firstIndex + 1]);
            initialLeftRightRatio = CalculateRatio(fifoData[firstIndex + 2], fifoData[firstIndex + 3]);
            finalUpDownRatio = CalculateRatio(fifoData[lastIndex], fifoData[lastIndex + 1]);
            finalLeftRightRatio = CalculateRatio(fifoData[lastIndex + 2], fifoData[lastIndex + 3]);
            return true;
        }

        private static int FindFirstValidDataSet(byte[] data, int dataSetCount)
        {
            for (int index = 0; index < dataSetCount * 4; index += 4)
            {
                if (IsValidDataSet(data, index))
                {
                    return index;
                }
            }

            return -1;
        }

        private static int FindLastValidDataSet(byte[] data, int dataSetCount)
        {
            for (int index = (dataSetCount - 1) * 4; index >= 0; index -= 4)
            {
                if (IsValidDataSet(data, index))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool IsValidDataSet(byte[] data, int index)
        {
            return data[index] > MinimumSignal
                && data[index] <= MaximumSignal
                && data[index + 1] > MinimumSignal
                && data[index + 1] <= MaximumSignal
                && data[index + 2] > MinimumSignal
                && data[index + 2] <= MaximumSignal
                && data[index + 3] > MinimumSignal
                && data[index + 3] <= MaximumSignal;
        }

        private static int CalculateRatio(int first, int second) => ((first - second) * 100) / (first + second);

        private static int Absolute(int value) => value < 0 ? -value : value;
    }
}
