// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs
{
    [BurstCompile]
    internal struct PrepareCombinedMeshJob : IJob
    {
        [WriteOnly] private Mesh.MeshData _combinedMesh;
        [ReadOnly] private readonly NativeArray<int> _vertexSubarraySizes;
        [ReadOnly] private readonly NativeArray<int> _indexSubarraySizes;

        public PrepareCombinedMeshJob(
            Mesh.MeshData combinedMesh,
            NativeArray<int> vertexSubarraySizes,
            NativeArray<int> indexSubarraySizes)
        {
            _combinedMesh = combinedMesh;
            _vertexSubarraySizes = vertexSubarraySizes;
            _indexSubarraySizes = indexSubarraySizes;
        }

        public void Execute()
        {
            _combinedMesh.SetIndexBufferParams(
                _indexSubarraySizes.Sum(),
                IndexFormat.UInt32);

            using var attrs = MapTilesJobsUtils.DefaultAttributeDescriptors();
            _combinedMesh.SetVertexBufferParams(_vertexSubarraySizes.Sum(), attrs);
        }
    }
}
