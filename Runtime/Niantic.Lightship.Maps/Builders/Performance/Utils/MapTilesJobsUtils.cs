// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils
{
    internal static class MapTilesJobsUtils
    {
        /// <summary>
        /// Allocates a <see cref="NativeArray{T}"/> of <see cref="VertexAttributeDescriptor"/>s
        /// which represent the default attributes for meshes created by <see cref="IMeshBuilderAsync"/>s.
        /// </summary>
        /// <param name="allocator">The allocation strategy used for the data</param>
        public static NativeArray<VertexAttributeDescriptor> DefaultAttributeDescriptors(
            Allocator allocator = Allocator.Temp)
        {
            var vertexBufferParams =
                new NativeArray<VertexAttributeDescriptor>(3, allocator, NativeArrayOptions.UninitializedMemory);

            vertexBufferParams[0] = new VertexAttributeDescriptor(VertexAttribute.Position);
            vertexBufferParams[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1);
            vertexBufferParams[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, stream: 2, dimension: 2);

            return vertexBufferParams;
        }

        /// <summary>
        /// Gets vertex positions from a <see cref="NativeArray{T}"/>
        /// of <see cref="Vertex"/> data.
        /// </summary>
        /// <param name="this">The array of vertices
        /// from which to extract positions.</param>
        /// <param name="padding">Padding added to the end of
        /// the returned <see cref="NativeArray{T}"/></param>
        /// <returns>A <see cref="NativeArray{T}"/> of vertex positions.</returns>
        public static NativeArray<float3> GetVertices(
            this NativeArray<Vertex> @this,
            int padding = 0)
        {
            var verts = new NativeArray<float3>(@this.Length + padding, Allocator.Temp);
            for (int i = 0; i < @this.Length; i++)
            {
                verts[i] = @this[i].Position;
            }

            return verts;
        }

        /// <summary>
        /// Gets UV coordinates from a <see cref="NativeArray{T}"/>
        /// of <see cref="Vertex"/> data.
        /// </summary>
        /// <param name="this">The array of vertices
        /// from which to extract UV coordinates.</param>
        /// <param name="padding">Padding added to the end of
        /// the returned <see cref="NativeArray{T}"/></param>
        /// <returns>A <see cref="NativeArray{T}"/> of UV coordinates.</returns>
        public static NativeArray<float2> GetUvs(
            this NativeArray<Vertex> @this,
            int padding = 0)
        {
            var uvs = new NativeArray<float2>(@this.Length + padding, Allocator.Temp);
            for (int i = 0; i < @this.Length; i++)
            {
                uvs[i] = @this[i].UV;
            }

            return uvs;
        }

        /// <summary>
        /// Gets vertex positions from a <see cref="NativeSlice{T}"/>
        /// of <see cref="Vertex"/> data.
        /// </summary>
        /// <param name="this">The array of vertices
        /// from which to extract positions.</param>
        /// <param name="padding">Padding added to the end of
        /// the returned <see cref="NativeArray{T}"/></param>
        /// <returns>A <see cref="NativeArray{T}"/> of vertex positions.</returns>
        public static NativeArray<float3> GetVertices(
            this NativeSlice<Vertex> @this,
            int padding = 0)
        {
            var verts = new NativeArray<float3>(@this.Length + padding, Allocator.Temp);
            for (int i = 0; i < @this.Length; i++)
            {
                verts[i] = @this[i].Position;
            }

            return verts;
        }

        /// <summary>
        /// Gets UV coordinates from a <see cref="NativeSlice{T}"/>
        /// of <see cref="Vertex"/> data.
        /// </summary>
        /// <param name="this">The array of vertices
        /// from which to extract UV coordinates.</param>
        /// <param name="padding">Padding added to the end of
        /// the returned <see cref="NativeArray{T}"/></param>
        /// <returns>A <see cref="NativeArray{T}"/> of UV coordinates.</returns>
        public static NativeArray<float2> GetUvs(
            this NativeSlice<Vertex> @this,
            int padding = 0)
        {
            var uvs = new NativeArray<float2>(@this.Length + padding, Allocator.Temp);
            for (int i = 0; i < @this.Length; i++)
            {
                uvs[i] = @this[i].UV;
            }

            return uvs;
        }

        /// <summary>
        /// Disposes any <see cref="IDisposable"/>s in the <see cref="NativeArray{T}"/>
        /// </summary>
        /// <param name="this">A <see cref="NativeArray{T}"/> of <see cref="IDisposable"/></param>
        /// <typeparam name="T">The type of <see cref="IDisposable"/> in the <see cref="NativeArray{T}"/></typeparam>
        public static void CleanUp<T>(this NativeArray<T> @this)
            where T : unmanaged, INativeDisposable
        {
            for (int i = 0; i < @this.Length; i++)
            {
                @this[i].Dispose();
            }

            @this.Dispose();
        }
    }
}
