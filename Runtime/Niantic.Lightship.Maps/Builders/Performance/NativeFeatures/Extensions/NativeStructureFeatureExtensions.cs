// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions
{
    internal static class NativeStructureFeatureExtensions
    {
        /// <summary>
        /// Produces a Job- and Burst-friendly version of an <see cref="IStructureFeature"/>
        /// </summary>
        /// <param name="managedStructureFeature">The <see cref="IStructureFeature"/> to point to</param>
        public static unsafe NativeStructureFeature ToNative<T>(this T managedStructureFeature)
            where T : IStructureFeature
        {
            var managedPoints = managedStructureFeature.Points;
            var managedIndices = managedStructureFeature.Indices;
            var managedExteriorEdges = managedStructureFeature.ExteriorEdges;

            var points = new UnsafeList<float3>(
                (float3*)UnsafeUtility.PinGCArrayAndGetDataAddress(managedPoints, out var pointsHandle),
                managedPoints.Length);

            var indices = new UnsafeList<int>(
                (int*)UnsafeUtility.PinGCArrayAndGetDataAddress(managedIndices, out var indicesHandle),
                managedIndices.Length);

            var nativeExteriorEdges = new NativeLineSegment[managedExteriorEdges.Length];

            for (int i = 0; i < managedExteriorEdges.Length; i++)
            {
                nativeExteriorEdges[i] = (NativeLineSegment)managedExteriorEdges[i];
            }

            var exteriorEdges = new UnsafeList<NativeLineSegment>(
                (NativeLineSegment*)UnsafeUtility.PinGCArrayAndGetDataAddress(
                    nativeExteriorEdges, out var exteriorEdgesHandle),
                managedExteriorEdges.Length);

            var labelInfo = managedStructureFeature.Label.ToNative();

            return new NativeStructureFeature(
                managedStructureFeature.Layer,
                managedStructureFeature.Kind,
                managedStructureFeature.Height,
                managedStructureFeature.IsUnderground,
                in points,
                pointsHandle,
                in indices,
                indicesHandle,
                in exteriorEdges,
                exteriorEdgesHandle,
                in labelInfo);
        }
    }
}
