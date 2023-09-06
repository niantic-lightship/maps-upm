// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Structs;
using Niantic.Lightship.Maps.Builders.Performance.Utils;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using IntReference = Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.UnsafeReference<int>;

namespace Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Jobs
{
    [BurstCompile]
    internal readonly struct CreateMeshJob : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        [WriteOnly] private readonly Mesh.MeshDataArray _output;
        [ReadOnly] private readonly int _meshIndex;

        [ReadOnly] private readonly float _smoothFactor;
        [ReadOnly] private readonly int _endCapPointCount;
        [ReadOnly] private readonly float _bendThreshold;
        [ReadOnly] private readonly float _thickness;

        private readonly IntReference _vertexCount;

        private readonly IntReference _indexCount;

        private readonly LinearFeatureSet _featureSet;

        public CreateMeshJob(
            IntReference vertexCount,
            IntReference indexCount,
            Mesh.MeshDataArray output,
            int meshIndex,
            float smoothFactor,
            int endCapPointCount,
            float bendThreshold,
            float thickness,
            LinearFeatureSet featureSet)
        {
            _vertexCount = vertexCount;
            _indexCount = indexCount;

            _output = output;
            _meshIndex = meshIndex;
            _smoothFactor = smoothFactor;
            _endCapPointCount = endCapPointCount;
            _bendThreshold = bendThreshold;
            _thickness = thickness;
            _featureSet = featureSet;
        }

        public void Execute()
        {
            using var linearFeatureMeshVertices = new NativeArray<Vertex>(_vertexCount.Value, Allocator.Temp);
            using var linearFeatureMeshIndices = new NativeArray<int>(_indexCount.Value, Allocator.Temp);

            ProcessLinearFeature(linearFeatureMeshVertices, linearFeatureMeshIndices);
            PopulateMeshData(_output[_meshIndex], linearFeatureMeshVertices, linearFeatureMeshIndices);
        }

        private void PopulateMeshData(Mesh.MeshData mesh, NativeArray<Vertex> vertices, NativeArray<int> indices)
        {
            using (var attrs = MapTilesJobsUtils.DefaultAttributeDescriptors())
            {
                mesh.SetVertexBufferParams(vertices.Length, attrs);
            }

            mesh.GetVertexData<float3>().CopyFrom(vertices.GetVertices()); // Set Mesh vertices

            using (var norms = vertices.GetWithValue(math.up()))
            {
                mesh.GetVertexData<float3>(stream: 1).CopyFrom(norms); // Set Mesh normals
            }

            // Set Mesh UVs
            using (var uvs = vertices.GetUvs())
            {
                mesh.GetVertexData<float2>(stream: 2).CopyFrom(uvs);
            }

            mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
            mesh.GetIndexData<int>().CopyFrom(indices); // Set Mesh indices
        }

        private void InsertIndices(NativeArray<int> indices, int vertexCount, IntReference indexIndex)
        {
            var currentVertexCount = vertexCount - 4;
            indices[indexIndex.Value++] = currentVertexCount;
            indices[indexIndex.Value++] = currentVertexCount + 2;
            indices[indexIndex.Value++] = currentVertexCount + 1;

            indices[indexIndex.Value++] = currentVertexCount + 1;
            indices[indexIndex.Value++] = currentVertexCount + 2;
            indices[indexIndex.Value++] = currentVertexCount + 3;
        }

        private void InsertSmoothingVertices(
            float3 p0,
            float3 p1,
            float3 p2,
            NativeArray<Vertex> vertices,
            NativeArray<int> indices,
            IntReference vertexIndex,
            IntReference indexIndex,
            float thickness,
            float targetCos,
            int depth)
        {
            // using http://graphics.cs.ucdavis.edu/education/CAGDNotes/Chaikins-Algorithm/Chaikins-Algorithm.html

            var p1ReplacementA = _smoothFactor * p0 + (1.0f - _smoothFactor) * p1;
            var p1ReplacementB = (1.0f - _smoothFactor) * p1 + _smoothFactor * p2;

            var insertedTangent0 = math.normalize(p1ReplacementA - p0);
            var insertedTangent1 = math.normalize(p1ReplacementB - p1ReplacementA);
            var insertedTangent2 = math.normalize(p2 - p1ReplacementB);

            if (depth > 0 && math.dot(insertedTangent0, insertedTangent1) < targetCos)
            {
                InsertSmoothingVertices(
                    p0,
                    p1ReplacementA,
                    p1ReplacementB,
                    vertices,
                    indices,
                    vertexIndex,
                    indexIndex,
                    thickness,
                    targetCos,
                    depth - 1);
            }
            else
            {
                var insertedBiNormal =
                    math.cross((insertedTangent0 + insertedTangent1) * 0.5f, math.up()) * 0.5f;

                var point1 = p1ReplacementA + insertedBiNormal * thickness;
                vertices[vertexIndex.Value++] = new Vertex(point1, point1.xz);

                var point2 = p1ReplacementA - insertedBiNormal * thickness;
                vertices[vertexIndex.Value++] = new Vertex(point2, point2.xz);

                InsertIndices(indices, vertexIndex.Value, indexIndex);
            }

            if (depth > 0 && math.dot(insertedTangent1, insertedTangent2) < targetCos)
            {
                InsertSmoothingVertices(
                    p1ReplacementA,
                    p1ReplacementB,
                    p2,
                    vertices,
                    indices,
                    vertexIndex,
                    indexIndex,
                    thickness,
                    targetCos,
                    depth - 1);
            }
            else
            {
                var insertedBiNormal =
                    math.cross((insertedTangent1 + insertedTangent2) * 0.5f, math.up()) * 0.5f;

                var point1 = p1ReplacementB + insertedBiNormal * thickness;
                vertices[vertexIndex.Value++] = new Vertex(point1, point1.xz);

                var point2 = p1ReplacementB - insertedBiNormal * thickness;
                vertices[vertexIndex.Value++] = new Vertex(point2, point2.xz);

                InsertIndices(indices, vertexIndex.Value, indexIndex);
            }
        }

        private void CreateMiteredWidePolyline(
            NativeArray<Vertex> vertices,
            NativeArray<int> indices,
            int startVertexIndex,
            int startIndexIndex,
            UnsafeList<float3> points,
            int pointOffset,
            int strip,
            float thickness,
            IntReference endVertexIndex,
            IntReference endIndexIndex)
        {
            endIndexIndex.Value = startVertexIndex;
            endIndexIndex.Value = startIndexIndex;

            if (strip < 2)
            {
                return;
            }

            var tangent0 = math.normalize(points[1 + pointOffset] - points[pointOffset]);
            var biNormal = math.cross(tangent0, math.up());

            // Hold this as the base index for the front cap
            var baseCapVertexIndex = endVertexIndex.Value;

            // Add front cap
            for (var i = 0; i < _endCapPointCount; ++i)
            {
                var angle = Mathf.PI * (i + 1) / (_endCapPointCount + 1);

                var point = points[pointOffset] -
                    0.5f * thickness * (Mathf.Sin(angle) * tangent0 + Mathf.Cos(angle) * biNormal);
                vertices[endVertexIndex.Value++] = new Vertex(point, point.xz);

                indices[endIndexIndex.Value++] = baseCapVertexIndex + i;
                indices[endIndexIndex.Value++] = baseCapVertexIndex + _endCapPointCount;

                if (i == 0)
                {
                    indices[endIndexIndex.Value++] = baseCapVertexIndex + _endCapPointCount + 1;
                }
                else
                {
                    indices[endIndexIndex.Value++] = baseCapVertexIndex + i - 1;
                }
            }

            for (var i = 0; i < strip - 1; ++i)
            {
                tangent0 = math.normalize(points[i + 1 + pointOffset] - points[i + pointOffset]);
                biNormal = math.cross(tangent0, math.up()) * 0.5f;

                if (i == 0)
                {
                    var point1 = points[i + pointOffset] + biNormal * thickness;
                    vertices[endVertexIndex.Value++] = new Vertex(point1, point1.xz);

                    var point2 = points[i + pointOffset] - biNormal * thickness;
                    vertices[endVertexIndex.Value++] = new Vertex(point2, point2.xz);
                }

                if (i < strip - 2)
                {
                    var tangent1 = math.normalize(points[i + 2 + pointOffset] - points[i + 1 + pointOffset]);

                    // if angle is to steep we iteratively insert new segments
                    if (math.dot(tangent0, tangent1) < _bendThreshold)
                    {
                        InsertSmoothingVertices(
                            points[i + pointOffset],
                            points[i + 1 + pointOffset],
                            points[i + 2 + pointOffset],
                            vertices,
                            indices,
                            endVertexIndex,
                            endIndexIndex,
                            thickness,
                            _bendThreshold,
                            3);
                        continue;
                    }

                    biNormal += math.cross(tangent1, math.up()) * 0.5f;
                    biNormal *= 0.5f;
                }

                var point3 = points[i + 1 + pointOffset] + biNormal * thickness;
                vertices[endVertexIndex.Value++] = new Vertex(point3, point3.xz);

                var point4 = points[i + 1 + pointOffset] - biNormal * thickness;
                vertices[endVertexIndex.Value++] = new Vertex(point4, point4.xz);

                InsertIndices(indices, endVertexIndex.Value, endIndexIndex);
            }

            tangent0 = math.normalize(points[pointOffset + strip - 1] - points[pointOffset + strip - 2]);
            biNormal = math.cross(tangent0, math.up());

            // Again, hold this value as base for the end cap
            baseCapVertexIndex = endVertexIndex.Value;

            // Add end cap
            for (var i = 0; i < _endCapPointCount; ++i)
            {
                var angle = Mathf.PI * (i + 1) / (_endCapPointCount + 1);

                var point = points[pointOffset + strip - 1] +
                    0.5f * thickness * (Mathf.Sin(angle) * tangent0 + Mathf.Cos(angle) * biNormal);
                vertices[endVertexIndex.Value++] = new Vertex(point, point.xz);

                indices[endIndexIndex.Value++] = baseCapVertexIndex + i;
                indices[endIndexIndex.Value++] = baseCapVertexIndex - 1;

                if (i == 0)
                {
                    indices[endIndexIndex.Value++] = baseCapVertexIndex - 2;
                }
                else
                {
                    indices[endIndexIndex.Value++] = baseCapVertexIndex + i - 1;
                }
            }
        }

        private void ProcessLinearFeature(NativeArray<Vertex> linearFeatureMeshVertices, NativeArray<int> linearFeatureMeshIndices)
        {
            var pointOffset = 0;
            var linearFeature = _featureSet.LinearFeature;

            foreach (var strip in linearFeature.LineStrips)
            {
                var startVertexIndex = _featureSet.VertStartIndex.Value;
                var startIndexIndex = _featureSet.IndexStartIndex.Value;

                CreateMiteredWidePolyline(
                    linearFeatureMeshVertices,
                    linearFeatureMeshIndices,
                    startVertexIndex,
                    startIndexIndex,
                    linearFeature.Points,
                    pointOffset,
                    strip,
                    _thickness,
                    _featureSet.VertStartIndex,
                    _featureSet.IndexStartIndex);

                pointOffset += strip;
            }
        }
    }
}
