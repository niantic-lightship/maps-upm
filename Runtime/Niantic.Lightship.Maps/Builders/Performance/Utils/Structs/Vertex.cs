// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Mathematics;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Structs
{
    /// <summary>
    /// A vertex type consisting of a position and set of UV coordinates.
    /// </summary>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vertex
    {
        /// <summary>
        /// The <see cref="Vertex"/>'s position
        /// </summary>
        public readonly float3 Position;

        /// <summary>
        /// The <see cref="Vertex"/>'s UV coordinates
        /// </summary>
        public readonly float2 UV;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">The new <see cref="Vertex"/>'s position</param>
        /// <param name="uv">The new <see cref="Vertex"/>'s UV coordinates</param>
        public Vertex(float3 pos, float2 uv)
        {
            Position = pos;
            UV = uv;
        }
    }
}
