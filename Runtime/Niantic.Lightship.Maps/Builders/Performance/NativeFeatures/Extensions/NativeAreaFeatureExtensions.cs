// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Async.Logging;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions
{
    internal static class NativeAreaFeatureExtensions
    {
        /// <summary>
        /// Produces a Job- and Burst-friendly version of an <see cref="IAreaFeature"/>
        /// </summary>
        /// <param name="managedAreaFeature">The <see cref="IAreaFeature"/> to point to</param>
        public static unsafe NativeAreaFeature ToNative<T>(this T managedAreaFeature)
            where T : IAreaFeature
        {
            try
            {
                var managedPoints = managedAreaFeature.Points;
                var managedIndices = managedAreaFeature.Indices;
                var managedExteriorEdges = managedAreaFeature.ExteriorEdges;

                var points = new UnsafeList<float3>(
                    (float3*)UnsafeUtility.PinGCArrayAndGetDataAddress(managedPoints, out var pointsHandle),
                    managedPoints.Length);

                var indices = new UnsafeList<int>(
                    (int*)UnsafeUtility.PinGCArrayAndGetDataAddress(managedIndices, out var indicesHandle),
                    managedIndices.Length);

                UnsafeList<NativeLineSegment> exteriorEdges;
                ulong exteriorEdgesHandle = 0;

                if (managedExteriorEdges != null)
                {
                    var nativeExteriorEdges = new NativeLineSegment[managedExteriorEdges.Length];

                    for (int i = 0; i < managedExteriorEdges.Length; i++)
                    {
                        nativeExteriorEdges[i] = (NativeLineSegment)managedExteriorEdges[i];
                    }

                    exteriorEdges = new UnsafeList<NativeLineSegment>(
                        (NativeLineSegment*)UnsafeUtility.PinGCArrayAndGetDataAddress(
                            nativeExteriorEdges, out exteriorEdgesHandle),
                        managedExteriorEdges.Length);
                }
                else
                {
                    exteriorEdges = new UnsafeList<NativeLineSegment>(
                        (NativeLineSegment*)IntPtr.Zero.ToPointer(), 0);
                }

                var labelInfo = managedAreaFeature.Label.ToNative();

                var layer = managedAreaFeature.Layer;
                var kind = managedAreaFeature.Kind;

                return new NativeAreaFeature(
                    layer,
                    kind,
                    in points,
                    pointsHandle,
                    in indices,
                    indicesHandle,
                    in exteriorEdges,
                    exteriorEdgesHandle,
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
