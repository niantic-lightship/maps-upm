// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Geometry;
using Unity.Burst;
using Unity.Mathematics;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures
{
    /// <inheritdoc cref="LineSegment"/>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NativeLineSegment
    {
        /// <inheritdoc cref="LineSegment.VertexA"/>
        public readonly float3 VertexA;

        /// <inheritdoc cref="LineSegment.VertexB"/>
        public readonly float3 VertexB;

        internal NativeLineSegment(LineSegment segment)
        {
            VertexA = new float3(segment.VertexA.x, segment.VertexA.y, segment.VertexA.z);
            VertexB = new float3(segment.VertexB.x, segment.VertexB.y, segment.VertexB.z);
        }

        public static explicit operator NativeLineSegment(LineSegment segment)
        {
            return new NativeLineSegment(segment);
        }
    }
}
