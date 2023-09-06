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
    /// <inheritdoc cref="IStructureFeature"/>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NativeStructureFeature : INativeDisposable
    {
        private readonly ulong _pointsHandle;
        private readonly ulong _indicesHandle;
        private readonly ulong _exteriorEdgesHandle;

        /// <inheritdoc cref="IStructureFeature.Points"/>
        public UnsafeList<float3> Points { get; }

        /// <inheritdoc cref="IStructureFeature.Indices"/>
        public UnsafeList<int> Indices { get; }

        /// <inheritdoc cref="IStructureFeature.ExteriorEdges"/>
        public UnsafeList<NativeLineSegment> ExteriorEdges { get; }

        /// <inheritdoc cref="IStructureFeature.Height"/>
        public readonly float Height;

        /// <inheritdoc cref="IStructureFeature.IsUnderground"/>
        public readonly bool IsUnderground;

        /// <inheritdoc cref="IMapTileFeature.Label"/>
        public NativeLabelInfo LabelInfo { get; }

        /// <inheritdoc cref="IMapTileFeature.Kind"/>
        public readonly FeatureKind Kind;

        /// <inheritdoc cref="IMapTileFeature.Layer"/>
        public readonly LayerKind Layer;

        internal NativeStructureFeature(
            LayerKind layer,
            FeatureKind kind,
            float height,
            bool isUnderground,
            in UnsafeList<float3> points,
            ulong pointsHandle,
            in UnsafeList<int> indices,
            ulong indicesHandle,
            in UnsafeList<NativeLineSegment> exteriorEdges,
            ulong exteriorEdgesHandle,
            in NativeLabelInfo labelInfo)
        {
            Layer = layer;
            Kind = kind;
            Height = height;
            IsUnderground = isUnderground;
            Points = points;
            _pointsHandle = pointsHandle;
            Indices = indices;
            _indicesHandle = indicesHandle;
            ExteriorEdges = exteriorEdges;
            _exteriorEdgesHandle = exteriorEdgesHandle;
            LabelInfo = labelInfo;
        }

        /// <summary>
        /// Disposes this feature's Native Collections and releases any
        /// resources
        /// </summary>
        public void Dispose()
        {
            Points.Dispose();
            UnsafeUtility.ReleaseGCObject(_pointsHandle);

            Indices.Dispose();
            UnsafeUtility.ReleaseGCObject(_indicesHandle);

            ExteriorEdges.Dispose();
            UnsafeUtility.ReleaseGCObject(_exteriorEdgesHandle);

            LabelInfo.Dispose();
        }

        /// <summary>
        /// Schedules a Job which disposes this feature's
        /// Native Collections and releases any resources
        /// </summary>
        public JobHandle Dispose(JobHandle dependencies)
        {
            var handles = new NativeArray<JobHandle>(5, Allocator.TempJob);

            handles[0] = Points.Dispose(dependencies);
            handles[1] = Indices.Dispose(dependencies);
            handles[2] = ExteriorEdges.Dispose(dependencies);
            handles[3] = LabelInfo.Dispose(dependencies);
            handles[4] = new DisposeJob
            {
                PointsHandle = _pointsHandle,
                IndicesHandle = _indicesHandle,
                ExteriorEdgesHandle = _exteriorEdgesHandle
            }.Schedule(dependencies);

            var combinedHandles = JobHandle.CombineDependencies(handles);
            return handles.Dispose(combinedHandles);
        }

        [BurstCompile]
        private struct DisposeJob : IJob
        {
            public ulong PointsHandle;
            public ulong IndicesHandle;
            public ulong ExteriorEdgesHandle;

            public void Execute()
            {
                UnsafeUtility.ReleaseGCObject(PointsHandle);
                UnsafeUtility.ReleaseGCObject(IndicesHandle);

                if (ExteriorEdgesHandle != 0)
                {
                    UnsafeUtility.ReleaseGCObject(ExteriorEdgesHandle);
                }
            }
        }
    }
}
