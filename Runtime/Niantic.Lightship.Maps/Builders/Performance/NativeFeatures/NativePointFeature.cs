// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures
{
    /// <inheritdoc cref="IPointFeature"/>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NativePointFeature : INativeDisposable
    {
        /// <inheritdoc cref="IPointFeature.Points"/>
        public UnsafeList<float3> Points { get; }

        private readonly ulong _pointsHandle;

        /// <inheritdoc cref="IMapTileFeature.Label"/>
        public NativeLabelInfo LabelInfo { get; }

        /// <inheritdoc cref="IMapTileFeature.Kind"/>
        public readonly FeatureKind Kind;

        /// <inheritdoc cref="IMapTileFeature.Layer"/>
        public readonly LayerKind Layer;

        internal NativePointFeature(
            LayerKind layer,
            FeatureKind kind,
            in UnsafeList<float3> points,
            ulong pointsHandle,
            in NativeLabelInfo labelInfo)
        {
            Points = points;
            _pointsHandle = pointsHandle;
            LabelInfo = labelInfo;
            Layer = layer;
            Kind = kind;
        }

        /// <summary>
        /// Disposes this feature's Native Collections and releases any
        /// resources
        /// </summary>
        public void Dispose()
        {
            Points.Dispose();
            UnsafeUtility.ReleaseGCObject(_pointsHandle);

            LabelInfo.Dispose();
        }

        /// <summary>
        /// Schedules a Job which disposes this feature's
        /// Native Collections and releases any resources
        /// </summary>
        public JobHandle Dispose(JobHandle dependencies)
        {
            var disposeJob = new DisposeJob { PointsHandle = _pointsHandle }.Schedule(dependencies);
            return JobHandle.CombineDependencies(
                disposeJob,
                Points.Dispose(dependencies),
                LabelInfo.Dispose(dependencies));
        }

        [BurstCompile]
        private struct DisposeJob : IJob
        {
            public ulong PointsHandle;

            public void Execute()
            {
                UnsafeUtility.ReleaseGCObject(PointsHandle);
            }
        }
    }
}
