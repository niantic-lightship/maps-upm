// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs
{
    [BurstCompile]
    internal struct CombineMeshesJob : IJobParallelFor
    {
        public Mesh.MeshData Output;

        [ReadOnly] public Mesh.MeshDataArray Input;

        [ReadOnly] public NativeArray<int> VertexSubarraySizes;
        [ReadOnly] public NativeArray<int> IndexSubarraySizes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float3> _tempVertices;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float3> _tempNormals;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float2> _tempUvs;

        public void Execute(int index)
        {
            var data = Input[index];
            var vCount = data.vertexCount;
            var vStart = VertexSubarraySizes.SumPreviousSizes(index);

            // Allocate temporary arrays for input mesh vertices/normals
            if (!_tempVertices.IsCreated || _tempVertices.Length < vCount)
            {
                if (_tempVertices.IsCreated)
                {
                    _tempVertices.Dispose();
                }

                _tempVertices = new NativeArray<float3>(
                    vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            }

            if (!_tempNormals.IsCreated || _tempNormals.Length < vCount)
            {
                if (_tempNormals.IsCreated)
                {
                    _tempNormals.Dispose();
                }

                _tempNormals = new NativeArray<float3>(
                    vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            }

            if (!_tempUvs.IsCreated || _tempUvs.Length < vCount)
            {
                if (_tempUvs.IsCreated)
                {
                    _tempUvs.Dispose();
                }

                _tempUvs = new NativeArray<float2>(
                    vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            }

            // Read input mesh vertices/normals into temporary arrays.
            // This will do any necessary format conversions into float3 data.
            data.GetVertices(_tempVertices.Reinterpret<Vector3>());
            data.GetUVs(0, _tempUvs.Reinterpret<Vector2>());
            data.GetNormals(_tempNormals.Reinterpret<Vector3>());

            // Transform input mesh vertices/normals, write into
            // destination mesh, compute transformed mesh bounds.
            int sum = VertexSubarraySizes.SumPreviousSizes(index);
            int length = VertexSubarraySizes[index % VertexSubarraySizes.Length];

            var arr = Output.GetVertexData<float3>();

            var currentVertices = arr.Slice(sum, length);
            var currentNormals = Output.GetVertexData<float3>(stream: 1).Slice(sum, length);
            var currentUvs = Output.GetVertexData<float2>(stream: 2);

            for (var i = 0; i < vCount; ++i)
            {
                if (i >= currentVertices.Length || i >= currentNormals.Length || i >= currentUvs.Length)
                {
                    continue;
                }

                var pos = _tempVertices[i];
                currentVertices[i] = pos;

                var nor = _tempNormals[i];
                currentNormals[i] = nor;

                var uv = _tempUvs[i];
                currentUvs[i] = uv;
            }

            // Write input mesh indices into destination index buffer
            var tStart = IndexSubarraySizes.SumPreviousSizes(index);
            var tCount = data.GetIndexData<int>().Length;
            var outputTris = Output.GetIndexData<int>();

            if (data.indexFormat == IndexFormat.UInt16)
            {
                var tris = data.GetIndexData<ushort>();
                for (var i = 0; i < tCount; ++i)
                {
                    if (i + tStart >= outputTris.Length || i >= tris.Length)
                    {
                        continue;
                    }

                    outputTris[i + tStart] = vStart + tris[i];
                }
            }
            else
            {
                var tris = data.GetIndexData<int>();
                for (var i = 0; i < tCount; ++i)
                {
                    if (i + tStart >= outputTris.Length || i >= tris.Length)
                    {
                        continue;
                    }

                    outputTris[i + tStart] = vStart + tris[i];
                }
            }
        }
    }
}
