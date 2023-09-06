// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Builders.Performance.Areas.Jobs
{
    [BurstCompile]
    internal struct PopulateFlattenedArraysJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<NativeAreaFeature> _allFeatures;

        [ReadOnly] private readonly NativeArray<int> _vertexSubarraySizes;
        [ReadOnly] private readonly NativeArray<int> _indexSubarraySizes;

        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<Vertex> _vertices;

        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<int> _indices;

        public PopulateFlattenedArraysJob(
            NativeArray<NativeAreaFeature> allFeatures,
            NativeArray<Vertex> vertices,
            NativeArray<int> vertexSubarraySizes,
            NativeArray<int> indices,
            NativeArray<int> indexSubarraySizes)
        {
            _allFeatures = allFeatures;
            _vertices = vertices;
            _vertexSubarraySizes = vertexSubarraySizes;
            _indices = indices;
            _indexSubarraySizes = indexSubarraySizes;
        }

        public void Execute(int index)
        {
            var feature = _allFeatures[index];

            var vertexOffset = _vertexSubarraySizes.SumPreviousSizes(index);
            var indexOffset = _indexSubarraySizes.SumPreviousSizes(index);

            for (int j = 0; j < _vertexSubarraySizes[index]; j++)
            {
                var point = feature.Points[j];
                _vertices[vertexOffset + j] = new Vertex(point, point.xz);
            }

            for (int j = 0; j < _indexSubarraySizes[index]; j++)
            {
                _indices[indexOffset + j] = feature.Indices[j];
            }
        }
    }
}
