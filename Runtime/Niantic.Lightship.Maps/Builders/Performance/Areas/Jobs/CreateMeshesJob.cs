// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.Utils;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance.Areas.Jobs
{
    [BurstCompile]
    internal struct CreateMeshesJob : IJobParallelFor
    {
        private Mesh.MeshDataArray _output;

        [ReadOnly] private readonly NativeArray<Vertex> _vertices;
        [ReadOnly] private readonly NativeArray<int> _vertexSubarraySizes;

        [ReadOnly] private readonly NativeArray<int> _indices;
        [ReadOnly] private readonly NativeArray<int> _indexSubarraySizes;

        public CreateMeshesJob(
            Mesh.MeshDataArray output,
            NativeArray<Vertex> vertices,
            NativeArray<int> vertexSubarraySizes,
            NativeArray<int> indices,
            NativeArray<int> indexSubarraySizes)
        {
            _output = output;
            _vertices = vertices;
            _vertexSubarraySizes = vertexSubarraySizes;
            _indices = indices;
            _indexSubarraySizes = indexSubarraySizes;
        }

        public void Execute(int index)
        {
            var mesh = _output[index];

            var currentVertices = _vertices.Slice(
                _vertexSubarraySizes.SumPreviousSizes(index),
                _vertexSubarraySizes[index]);

            var currentIndices = _indices.Slice(
                _indexSubarraySizes.SumPreviousSizes(index),
                _indexSubarraySizes[index]);

            SetMeshVertexAttributes(mesh, currentVertices);
            SetMeshIndexAttributes(mesh, currentIndices);
        }

        private void SetMeshVertexAttributes(Mesh.MeshData mesh, NativeSlice<Vertex> currentVertices)
        {
            // Configure the VertexBuffer for the MeshData
            // This allows for inclusion of Position/Normal/UV/etc data in the MeshData
            // Each of these are represented in a different "stream" of the VertexBuffer
            using (var attrs = MapTilesJobsUtils.DefaultAttributeDescriptors())
            {
                mesh.SetVertexBufferParams(_vertices.Length, attrs);
            }

            // Set the vertices of the Mesh using the positions in the vertices array
            var meshVertexData = mesh.GetVertexData<float3>();
            var vertexPadding = meshVertexData.Length - currentVertices.Length;
            using (var vertices = currentVertices.GetVertices(vertexPadding))
            {
                meshVertexData.CopyFrom(vertices);
            }

            // Set the normals of the Mesh, setting them all to Vector3.up
            var meshNormalData = mesh.GetVertexData<float3>(stream: 1);
            var normalPadding = meshNormalData.Length - currentVertices.Length;
            using (var normals = currentVertices.GetWithValue(math.up(), normalPadding))
            {
                meshNormalData.CopyFrom(normals);
            }

            // Set the UVs of the Mesh using the UVs in the vertices array
            var meshUvData = mesh.GetVertexData<float2>(stream: 2);
            var uvPadding = meshUvData.Length - currentVertices.Length;
            using (var uvs = currentVertices.GetUvs(uvPadding))
            {
                meshUvData.CopyFrom(uvs);
            }
        }

        private void SetMeshIndexAttributes(Mesh.MeshData mesh, NativeSlice<int> currentIndices)
        {
            // Configure the IndexBuffer for the MeshData
            // This primarily involves specifying the size of the Index value
            // (unsigned 32-bit int or unsigned 64-bit int)
            mesh.SetIndexBufferParams(currentIndices.Length, IndexFormat.UInt32);
            currentIndices.CopyTo(mesh.GetIndexData<int>()); // Set Mesh indices
        }
    }
}
