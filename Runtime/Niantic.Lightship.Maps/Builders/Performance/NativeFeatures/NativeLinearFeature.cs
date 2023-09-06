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
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NativeLinearFeature : INativeDisposable
    {
        private readonly ulong _pointsHandle;
        private readonly ulong _lineStripsHandle;

        /// <inheritdoc cref="ILinearFeature.Points"/>
        public UnsafeList<float3> Points { get; }

        /// <inheritdoc cref="ILinearFeature.LineStrips"/>
        public UnsafeList<int> LineStrips { get; }

        /// <inheritdoc cref="IMapTileFeature.Label"/>
        public NativeLabelInfo LabelInfo { get; }

        /// <inheritdoc cref="IMapTileFeature.Kind"/>
        public readonly FeatureKind Kind;

        /// <inheritdoc cref="IMapTileFeature.Layer"/>
        public readonly LayerKind Layer;

        internal NativeLinearFeature(
            LayerKind layer,
            FeatureKind kind,
            in UnsafeList<float3> points,
            ulong pointsHandle,
            in UnsafeList<int> lineStrips,
            ulong lineStripsHandle,
            in NativeLabelInfo labelInfo)
        {
            Points = points;
            LineStrips = lineStrips;
            _pointsHandle = pointsHandle;
            _lineStripsHandle = lineStripsHandle;
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

            LineStrips.Dispose();
            UnsafeUtility.ReleaseGCObject(_lineStripsHandle);

            LabelInfo.Dispose();
        }

        /// <summary>
        /// Schedules a Job which disposes this feature's
        /// Native Collections and releases any resources
        /// </summary>
        public JobHandle Dispose(JobHandle dependencies)
        {
            var handles = new NativeArray<JobHandle>(4, Allocator.TempJob);
            handles[0] = new DisposeJob { PointsHandle = _pointsHandle, LineStripsHandle = _lineStripsHandle }
                .Schedule(dependencies);

            handles[1] = LabelInfo.Dispose(dependencies);
            handles[2] = Points.Dispose(dependencies);
            handles[3] = LineStrips.Dispose(dependencies);

            var combinedDependencies = JobHandle.CombineDependencies(handles);

            return handles.Dispose(combinedDependencies);
        }

        [BurstCompile]
        private struct DisposeJob : IJob
        {
            public ulong PointsHandle;
            public ulong LineStripsHandle;

            public void Execute()
            {
                UnsafeUtility.ReleaseGCObject(PointsHandle);
                UnsafeUtility.ReleaseGCObject(LineStripsHandle);
            }
        }
    }
}
