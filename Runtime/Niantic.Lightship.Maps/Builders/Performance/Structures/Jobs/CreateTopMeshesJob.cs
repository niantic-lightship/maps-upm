// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.Utils;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance.Structures.Jobs
{
    [BurstCompile]
    internal struct CreateTopMeshesJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        private Mesh.MeshDataArray _output;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<Vertex> _vertices;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<int> _vertexSubarraySizes;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<int> _indices;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<int> _indexSubarraySizes;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<float> _heights;

        public CreateTopMeshesJob(
            NativeArray<NativeStructureFeature> allFeatures,
            Mesh.MeshDataArray output,
            float minHeight,
            float maxHeight,
            Allocator allocator = Allocator.TempJob)
        {
            _output = output;

            var featureCount = allFeatures.Length;

            int totalVertexCount = 0;
            int totalIndexCount = 0;

            // Iterate through our structure features once to
            // compute the total number of vertices and indices.
            for (int i = 0; i < featureCount; i++)
            {
                var structureFeature = allFeatures[i];

                totalVertexCount += structureFeature.Points.Length;
                totalIndexCount += structureFeature.Indices.Length;
            }

            // Allocate all of our NativeArrays
            _indices = new NativeArray<int>(totalIndexCount, allocator);
            _vertices = new NativeArray<Vertex>(totalVertexCount, allocator);
            _vertexSubarraySizes = new NativeArray<int>(featureCount, allocator);
            _indexSubarraySizes = new NativeArray<int>(featureCount, allocator);
            _heights = new NativeArray<float>(featureCount, allocator);

            int vertex = 0;
            int index = 0;

            // Iterate through our structure features again to set
            // all the values in our newly-allocated NativeArrays.
            for (int i = 0; i < featureCount; i++)
            {
                var structureFeature = allFeatures[i];
                var indices = structureFeature.Indices;
                var points = structureFeature.Points;
                var indexCount = indices.Length;
                var pointCount = points.Length;

                _heights[i] = Mathf.Clamp(structureFeature.Height, minHeight, maxHeight);

                _vertexSubarraySizes[i] = pointCount;
                _indexSubarraySizes[i] = indexCount;

                for (int j = 0; j < pointCount; j++)
                {
                    _vertices[vertex++] = new Vertex(points[j], float2.zero);
                }

                for (int j = 0; j < indexCount; j++)
                {
                    _indices[index++] = indices[j];
                }
            }
        }

        public void Execute(int index)
        {
            var mesh = _output[index];

            var currentPoints = _vertices.Slice(
                _vertexSubarraySizes.SumPreviousSizes(index),
                _vertexSubarraySizes[index]);

            var currentIndices = _indices.Slice(
                _indexSubarraySizes.SumPreviousSizes(index),
                _indexSubarraySizes[index]);

            using (var attrs = MapTilesJobsUtils.DefaultAttributeDescriptors())
            {
                mesh.SetVertexBufferParams(currentPoints.Length, attrs);
            }

            using (var vertices = MoveUp(currentPoints, _heights[index]))
            {
                mesh.GetVertexData<float3>().CopyFrom(vertices);
            }

            // Set Normals
            using (var normals = currentPoints.GetWithValue(math.up()))
            {
                mesh.GetVertexData<float3>(stream: 1).CopyFrom(normals);
            }

            // Set UVs
            using (var uvs = currentPoints.GetWithValue(new float2(0.25f, 0.25f)))
            {
                mesh.GetVertexData<float2>(stream: 2).CopyFrom(uvs);
            }

            mesh.SetIndexBufferParams(currentIndices.Length, IndexFormat.UInt32);
            using (var indices = currentIndices.ToNativeArray())
            {
                mesh.GetIndexData<int>().CopyFrom(indices); // Set Mesh indices
            }
        }

        private static NativeArray<float3> MoveUp(NativeSlice<Vertex> currentPoints, float height)
        {
            var delta = math.up() * height;
            var result = new NativeArray<float3>(currentPoints.Length, Allocator.Temp);

            for (int i = 0; i < currentPoints.Length; i++)
            {
                result[i] = currentPoints[i].Position + delta;
            }

            return result;
        }
    }
}
