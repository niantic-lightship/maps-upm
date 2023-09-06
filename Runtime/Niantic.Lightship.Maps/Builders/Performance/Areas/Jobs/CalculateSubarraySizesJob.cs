// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Builders.Performance.Areas.Jobs
{
    [BurstCompile]
    internal struct CalculateSubarraySizesJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<NativeAreaFeature> _allFeatures;

        [WriteOnly] private NativeArray<int> _vertexSubarraySizes;
        [WriteOnly] private NativeArray<int> _indexSubarraySizes;

        public CalculateSubarraySizesJob(
            NativeArray<NativeAreaFeature> allFeatures,
            NativeArray<int> vertexSubarraySizes,
            NativeArray<int> indexSubarraySizes)
        {
            _allFeatures = allFeatures;
            _vertexSubarraySizes = vertexSubarraySizes;
            _indexSubarraySizes = indexSubarraySizes;
        }

        public void Execute(int index)
        {
            var feature = _allFeatures[index];

            _vertexSubarraySizes[index] = feature.Points.Length;
            _indexSubarraySizes[index] = feature.Indices.Length;
        }
    }
}
