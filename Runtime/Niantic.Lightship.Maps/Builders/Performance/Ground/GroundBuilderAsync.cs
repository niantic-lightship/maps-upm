// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Async.Logging;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Performance.Ground
{
    /// <summary>
    /// A builder that generates a base ground mesh covering the entire maptile.
    /// Due to the simplicity and ubiquity of ground meshes, each <see cref="IMapTile"/>
    /// using this builder shares the same <see cref="Mesh"/>.
    /// </summary>
    [PublicAPI]
    public class GroundBuilderAsync : MeshBuilderAsync
    {
        private Mesh _groundMesh;

        /// <inheritdoc />
        /// Additionally, creates the shared <see cref="Mesh"/>
        /// used by all <see cref="IMapTile"/>s using this builder.
        public override void Initialize(ILightshipMapView lightshipMapView)
        {
            base.Initialize(lightshipMapView);
            CreateGroundMesh();
        }

        /// <summary>
        /// No-op
        /// </summary>
        /// <param name="tiles">The tiles to be built</param>
        /// <param name="tilesToBuild">Assigned to <paramref name="tiles"/></param>
        /// <returns>Always true</returns>
        public override bool PreBuild(IReadOnlyList<MeshTile> tiles, out IReadOnlyList<MeshTile> tilesToBuild)
        {
            tilesToBuild = tiles;
            return true;
        }

        /// <summary>
        /// Generates the Meshes for the ground layer, which
        /// is always the unit square covering the tile.
        /// </summary>
        public override void Build(IReadOnlyList<MeshTile> tiles)
        {
            var validTileCount = tiles.Count;

            // If there are no valid tiles, exit early
            if (validTileCount == 0)
            {
                return;
            }

            try
            {
                // Pass the generated Meshes to their respective MeshFilters
                for (int i = 0; i < validTileCount; i++)
                {
                    BuildGroundMesh(tiles[i]);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates a simple quad and stores it in this builder
        /// </summary>
        private void CreateGroundMesh()
        {
            _groundMesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1)
                },
                uv = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 0),
                    new Vector2(1, 1)
                },
                normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up },
                triangles = new[] { 0, 1, 2, 2, 1, 3 }
            };
        }

        /// <summary>
        /// Assigns the shared ground <see cref="Mesh"/> to the
        /// <paramref name="tile"/>'s <see cref="MeshFilter"/>.
        /// </summary>
        /// <param name="tile">The <see cref="MeshTile"/> to build</param>
        private void BuildGroundMesh(MeshTile tile)
        {
            try
            {
                // If the mesh has already been set, early out
                if (tile.Filter.sharedMesh != null)
                {
                    return;
                }

                var token = tile.Token;

                token.ThrowIfCancellationRequested();

                SetMeshForFilter(_groundMesh, tile.Filter);
            }
            catch (OperationCanceledException)
            {
                SetMeshForFilter(null, tile.Filter);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
            finally
            {
                tile.MarkBuilderComplete(this);
            }
        }

        /// <summary>
        /// No-op override of <see cref="Release"/>
        /// </summary>
        public override void Release(MeshFilter meshFilter)
        {
        }

        /// <summary>
        /// Destroys the shared ground <see cref="Mesh"/> when this builder
        /// is destroyed
        /// </summary>
        private void OnDestroy()
        {
            Destroy(_groundMesh);
        }
    }
}
