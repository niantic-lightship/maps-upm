// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Utilities
{
    /// <summary>
    /// Utilities for creating meshes at runtime.
    /// </summary>
    [PublicAPI]
    public static class MeshBuilderUtils
    {
        /// <summary>
        /// Build a new mesh and split vertex position into its own stream.
        /// </summary>
        /// <param name="meshName">Name of the new mesh</param>
        /// <param name="vertices">The vertices of the new mesh</param>
        /// <param name="indices">The indices of the new mesh</param>
        /// <returns>A new mesh</returns>
        internal static Mesh BuildSplitStreamMesh(string meshName, Vector3[] vertices, int[] indices)
        {
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = indices,
                name = meshName
            };

            SplitVertexStreams(mesh);

            return mesh;
        }

        /// <summary>
        /// Splits vertex position data into its own stream, leaving other data interleaved
        /// in a second stream. This is good for gpu performance. For more information see:
        /// https://developer.android.com/games/optimize/vertex-data-management
        /// </summary>
        /// <param name="mesh">The mesh to modify</param>
        private static void SplitVertexStreams(Mesh mesh)
        {
            var descriptors = mesh.GetVertexAttributes();
            for (var x = 0; x < descriptors.Length; x++)
            {
                descriptors[x].stream = descriptors[x].attribute == VertexAttribute.Position ? 0 : 1;
            }

            mesh.SetVertexBufferParams(mesh.vertexCount, descriptors);
        }

        /// <summary>
        /// Calculates the centroid of a polygon from its vertices
        /// </summary>
        /// <param name="vertices">The polygon's vertices</param>
        /// <returns>The centroid of the polygon</returns>
        public static Vector3 CalculateCentroid(Vector3[] vertices)
        {
            var centroid = Vector3.zero;

            foreach (var vertex in vertices)
            {
                centroid += vertex;
            }

            return centroid / vertices.Length;
        }
    }
}
