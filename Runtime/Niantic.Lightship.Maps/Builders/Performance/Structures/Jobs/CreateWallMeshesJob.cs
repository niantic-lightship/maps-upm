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
    internal struct CreateWallMeshesJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        private Mesh.MeshDataArray _output;

        [ReadOnly]
        private readonly int _featureCount;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<NativeLineSegment> _exteriorEdges;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<int> _exteriorEdgeSubarraySizes;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        private readonly NativeArray<float> _heights;

        public CreateWallMeshesJob(
            NativeArray<NativeStructureFeature> structureFeatures,
            Mesh.MeshDataArray output,
            float minHeight,
            float maxHeight, Allocator allocator = Allocator.TempJob)
        {
            _output = output;
            _featureCount = structureFeatures.Length;
            int totalExteriorEdges = 0;

            // Iterate through our structure features once to
            // compute the total number of exterior edges.
            for (int i = 0; i < _featureCount; i++)
            {
                var structureFeature = structureFeatures[i];
                totalExteriorEdges += structureFeature.ExteriorEdges.Length;
            }

            // Allocate all of our NativeArrays
            _exteriorEdges = new NativeArray<NativeLineSegment>(totalExteriorEdges, allocator);
            _exteriorEdgeSubarraySizes = new NativeArray<int>(_featureCount, allocator);
            _heights = new NativeArray<float>(_featureCount, allocator);

            int exteriorEdgeIndex = 0;

            // Iterate through our structure features again to set
            // all the values in our newly-allocated NativeArrays.
            for (int i = 0; i < _featureCount; i++)
            {
                var structureFeature = structureFeatures[i];
                var exteriorEdges = structureFeature.ExteriorEdges;
                var exteriorEdgeCount = exteriorEdges.Length;

                _exteriorEdgeSubarraySizes[i] = exteriorEdgeCount;
                _heights[i] = Mathf.Clamp(structureFeature.Height, minHeight, maxHeight);

                for (int j = 0; j < exteriorEdgeCount; j++)
                {
                    _exteriorEdges[exteriorEdgeIndex++] = exteriorEdges[j];
                }
            }
        }

        public void Execute(int index)
        {
            var mesh = _output[index + _featureCount];

            var currentExteriorEdges = _exteriorEdges.Slice(
                _exteriorEdgeSubarraySizes.SumPreviousSizes(index),
                _exteriorEdgeSubarraySizes[index]);

            GenerateWallMesh(currentExteriorEdges, _heights[index], mesh);
        }

        private void GenerateWallMesh(
            NativeSlice<NativeLineSegment> exteriorEdges,
            float height,
            Mesh.MeshData mesh)
        {
            var wallCount = exteriorEdges.Length;

            var vertices = new NativeArray<Vertex>(wallCount * 4, Allocator.Temp);
            var normals = new NativeArray<float3>(vertices.Length, Allocator.Temp);
            var triangles = new NativeArray<int>(wallCount * 6, Allocator.Temp);

            var heightOffset = math.up() * height;

            int vertexIndex = 0;
            int triIndex = 0;

            try
            {
                for (var i = 0; i < exteriorEdges.Length; i++)
                {
                    var exteriorEdge = exteriorEdges[i];
                    var edge = exteriorEdge.VertexB - exteriorEdge.VertexA;
                    var normal = math.normalize(math.cross(math.up(), edge));
                    var uv = new float2(0.75f, 0.75f);

                    // Vertices
                    var vertexIndex0 = vertexIndex++;
                    var vertexIndex1 = vertexIndex++;
                    var vertexIndex2 = vertexIndex++;
                    var vertexIndex3 = vertexIndex++;

                    vertices[vertexIndex0] = new Vertex(exteriorEdge.VertexA, uv);
                    vertices[vertexIndex1] = new Vertex(exteriorEdge.VertexB, uv);
                    vertices[vertexIndex2] = new Vertex(exteriorEdge.VertexA + heightOffset, uv);
                    vertices[vertexIndex3] = new Vertex(exteriorEdge.VertexB + heightOffset, uv);

                    // Normals
                    normals[vertexIndex0] = normal;
                    normals[vertexIndex1] = normal;
                    normals[vertexIndex2] = normal;
                    normals[vertexIndex3] = normal;

                    // Triangles
                    triangles[triIndex++] = vertexIndex2;
                    triangles[triIndex++] = vertexIndex1;
                    triangles[triIndex++] = vertexIndex0;

                    triangles[triIndex++] = vertexIndex2;
                    triangles[triIndex++] = vertexIndex3;
                    triangles[triIndex++] = vertexIndex1;
                }

                using (var attrs = MapTilesJobsUtils.DefaultAttributeDescriptors())
                {
                    mesh.SetVertexBufferParams(vertices.Length, attrs);
                }

                using (var verts = vertices.GetVertices())
                {
                    mesh.GetVertexData<float3>().CopyFrom(verts);
                }

                // Set Normals
                mesh.GetVertexData<float3>(stream: 1).CopyFrom(normals);

                // Set UVs
                using (var uvs = vertices.GetUvs())
                {
                    mesh.GetVertexData<float2>(stream: 2).CopyFrom(uvs);
                }

                mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
                mesh.GetIndexData<int>().CopyFrom(triangles); // Set Mesh indices
            }
            finally
            {
                vertices.Dispose();
                normals.Dispose();
                triangles.Dispose();
            }
        }
    }
}
