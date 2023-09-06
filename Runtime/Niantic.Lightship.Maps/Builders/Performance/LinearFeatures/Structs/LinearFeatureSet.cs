// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using IntReference = Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.UnsafeReference<int>;

namespace Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Structs
{
    /// <summary>
    /// A helper struct used to calculate linear feature meshes
    /// after the <see cref="ILinearFeature"/> has been appraised
    /// </summary>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public struct LinearFeatureSet : INativeDisposable
    {
        /// <summary>
        /// Total vertex count for this feature
        /// </summary>
        public IntReference NeededVerts { get; }

        /// <summary>
        /// Used as an offset for calculating <see cref="NeededVerts"/>
        /// </summary>
        public IntReference VertStartIndex { get; }

        /// <summary>
        /// Total index count for this feature
        /// </summary>
        public IntReference NeededIndices { get; }

        /// <summary>
        /// Used as an offset for calculating <see cref="NeededIndices"/>
        /// </summary>
        public IntReference IndexStartIndex { get; }

        /// <summary>
        /// A Job-friendly representation of a given <see cref="ILinearFeature"/>
        /// </summary>
        public NativeLinearFeature LinearFeature;

        public LinearFeatureSet(NativeLinearFeature feature)
        {
            LinearFeature = feature;
            IndexStartIndex = new IntReference(0);
            NeededIndices = new IntReference(0);
            VertStartIndex = new IntReference(0);
            NeededVerts = new IntReference(0);
        }

        /// <summary>
        /// Disposes this struct's NativeContainers
        /// </summary>
        public void Dispose()
        {
            NeededVerts.Dispose();
            VertStartIndex.Dispose();
            NeededIndices.Dispose();
            IndexStartIndex.Dispose();
            LinearFeature.Dispose();
        }

        /// <summary>
        /// Schedules Jobs to dispose this feature's NativeContainers
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public JobHandle Dispose(JobHandle dependencies)
        {
            var depsArray = new NativeArray<JobHandle>(5, Allocator.TempJob);
            depsArray[0] = NeededVerts.Dispose(dependencies);
            depsArray[1] = VertStartIndex.Dispose(dependencies);
            depsArray[2] = NeededIndices.Dispose(dependencies);
            depsArray[3] = IndexStartIndex.Dispose(dependencies);
            depsArray[4] = LinearFeature.Dispose(dependencies);

            var handle = JobHandle.CombineDependencies(depsArray);
            return depsArray.Dispose(handle);
        }
    }
}
