// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs
{
    [BurstCompile]
    internal struct CalculateSubarraySizesJob : IJobParallelFor
    {
        [ReadOnly] private readonly Mesh.MeshDataArray _allFeatureMeshes;

        [WriteOnly] private NativeArray<int> _vertexSubarraySizes;
        [WriteOnly] private NativeArray<int> _indexSubarraySizes;

        public CalculateSubarraySizesJob(
            Mesh.MeshDataArray allFeatureMeshes,
            NativeArray<int> vertexSubarraySizes,
            NativeArray<int> indexSubarraySizes)
        {
            _allFeatureMeshes = allFeatureMeshes;
            _vertexSubarraySizes = vertexSubarraySizes;
            _indexSubarraySizes = indexSubarraySizes;
        }

        public void Execute(int index)
        {
            var mesh = _allFeatureMeshes[index];

            _vertexSubarraySizes[index] = mesh.vertexCount;
            _indexSubarraySizes[index] = mesh.GetIndexData<int>().Length;
        }
    }
}
