// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs
{
    [BurstCompile]
    internal struct CreateVertexAndIndexArraysJob : IJob
    {
        [ReadOnly] private readonly NativeArray<int> _vertexSubarraySizes;
        [ReadOnly] private readonly NativeArray<int> _indexSubarraySizes;

        [WriteOnly] private NativeList<Vertex> _vertices;
        [WriteOnly] private NativeList<int> _indices;

        public CreateVertexAndIndexArraysJob(NativeList<Vertex> vertices, NativeArray<int> vertexSubarraySizes,
            NativeList<int> indices, NativeArray<int> indexSubarraySizes)
        {
            _vertices = vertices;
            _vertexSubarraySizes = vertexSubarraySizes;
            _indices = indices;
            _indexSubarraySizes = indexSubarraySizes;
        }

        public void Execute()
        {
            for (int i = 0; i < _vertexSubarraySizes.Length; i++)
            {
                for (int j = 0; j < _vertexSubarraySizes[i]; j++)
                {
                    _vertices.Add(new Vertex());
                }

                for (int j = 0; j < _indexSubarraySizes[i]; j++)
                {
                    _indices.Add(0);
                }
            }
        }
    }
}
