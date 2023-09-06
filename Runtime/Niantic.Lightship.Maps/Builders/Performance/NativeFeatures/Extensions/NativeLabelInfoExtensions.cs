// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Async.Logging;
using Niantic.Lightship.Maps.Core.Extensions;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections.LowLevel.Unsafe;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions
{
    internal static class NativeLabelInfoExtensions
    {
        /// <summary>
        /// Produces a Job- and Burst-friendly version of an <see cref="ILabelInfo"/>
        /// </summary>
        /// <param name="labelInfo">The <see cref="ILabelInfo"/> to point to</param>
        public static unsafe NativeLabelInfo ToNative<T>(this T labelInfo)
            where T : ILabelInfo
        {
            try
            {
                if (labelInfo == null)
                {
                    return new NativeLabelInfo();
                }

                var priority = labelInfo.Priority;
                var minZoom = labelInfo.MinZoom;
                var maxZoom = labelInfo.MaxZoom;
                var posX = labelInfo.PosX;
                var posY = labelInfo.PosY;
                var text = labelInfo.Text;

                var textPtr = (byte*)IntPtr.Zero;
                ulong textHandle = 0;
                UnsafeList<byte> textList;

                if (text != null && text.IsEmpty())
                {
                    textPtr = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(text, out textHandle);
                    textList = new UnsafeList<byte>(textPtr, text.Length);
                }
                else
                {
                    textList = new UnsafeList<byte>(textPtr, 0);
                }

                return new NativeLabelInfo(priority, minZoom, maxZoom, posX, posY, in textList, textHandle);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }
    }
}
