// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Async.Logging;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions
{
    internal static class NativeLinearFeatureExtensions
    {
        /// <summary>
        /// Produces a Job- and Burst-friendly version of an <see cref="ILinearFeature"/>
        /// </summary>
        /// <param name="managedLinearFeature">The <see cref="ILinearFeature"/> to point to</param>
        public static unsafe NativeLinearFeature ToNative<T>(this T managedLinearFeature)
            where T : ILinearFeature
        {
            try
            {
                var managedPoints = managedLinearFeature.Points;
                var managedLineStrips = managedLinearFeature.LineStrips;

                var points = new UnsafeList<float3>(
                    (float3*)UnsafeUtility.PinGCArrayAndGetDataAddress(managedPoints, out var pointsHandle),
                    managedPoints.Length);

                var lineStrips = new UnsafeList<int>(
                    (int*)UnsafeUtility.PinGCArrayAndGetDataAddress(managedLineStrips, out var lineStripsHandle),
                    managedLineStrips.Length);

                var labelInfo = managedLinearFeature.Label.ToNative();
                var layer = managedLinearFeature.Layer;
                var kind = managedLinearFeature.Kind;

                return new NativeLinearFeature(
                    layer,
                    kind,
                    in points,
                    pointsHandle,
                    in lineStrips,
                    lineStripsHandle,
                    in labelInfo);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }
    }
}
