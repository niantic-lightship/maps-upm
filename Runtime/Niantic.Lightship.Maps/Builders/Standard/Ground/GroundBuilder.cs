// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.Ground
{
    /// <summary>
    /// A builder that generates a base ground mesh covering the entire maptile
    /// </summary>
    [PublicAPI]
    public class GroundBuilder : MeshBuilderStandard, IMeshBuilderStandard
    {
        /// <summary>
        /// Returns the mesh for the ground layer, which is always the unit square covering the tile.
        /// </summary>
        public override void Build(IMapTile mapTile, MeshFilter meshFilter)
        {
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1)
                },
                uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) },
                normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up },
                triangles = new[] { 0, 1, 2, 2, 1, 3 }
            };

            SetMeshForFilter(mesh, meshFilter);
        }
    }
}
