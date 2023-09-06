// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.Linq;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance
{
    /// <inheritdoc cref="IMeshBuilderAsync" />
    [PublicAPI]
    public abstract partial class MeshBuilderAsync : MeshBuilderBase, IMeshBuilderAsync
    {
        /// <inheritdoc />
        public virtual bool PreBuild(IReadOnlyList<MeshTile> tiles, out IReadOnlyList<MeshTile> tilesToBuild)
        {
            // Grab the tiles that are valid for this builder and free any resources on invalid tiles
            tilesToBuild = GetValidTilesAndClearInvalidTiles(tiles, MinLOD, MaxLOD);

            // If there are no valid tiles, exit early
            return tilesToBuild.Count != 0;
        }

        /// <inheritdoc />
        public abstract void Build(IReadOnlyList<MeshTile> tiles);

        /// <summary>
        /// Filters out and clears <see cref="MeshTile"/>s that are
        /// not currently valid for a given <see cref="IFeatureBuilder"/>.
        /// </summary>
        /// <param name="tiles">A <see cref="IReadOnlyList{T}"/> of <see cref="MeshTile"/>s</param>
        /// <param name="minLOD">The minimum LOD for which the <see cref="IMapTile"/>s must be valid</param>
        /// <param name="maxLOD">The maximum LOD for which the <see cref="IMapTile"/>s must be valid</param>
        /// <returns>A <see cref="Task{T}"/> of <see cref="IReadOnlyList{T}"/> of the <see cref="MeshTile"/>s
        /// that are currently valid for the given <see cref="IFeatureBuilder"/></returns>
        protected IReadOnlyList<MeshTile> GetValidTilesAndClearInvalidTiles(
            IReadOnlyList<MeshTile> tiles,
            int minLOD,
            int maxLOD)
        {
            var (valid, invalid) =
                tiles.GroupBy(tile => MathEx.IsBetween(tile.Tile.ZoomLevel, minLOD, maxLOD));

            ClearInvalidTiles(invalid);

            return valid;
        }

        /// <summary>
        /// Gets a list of <see cref="IMapTileFeature"/>s per <see cref="MeshTile"/>.
        /// </summary>
        /// <param name="mapTiles">A <see cref="IReadOnlyList{T}"/> of all <see cref="MeshTile"/>s
        /// queued to be built by this <see cref="MeshBuilderAsync"/></param>
        /// <typeparam name="T">A feature type derived from <see cref="IMapTileFeature"/>
        /// that specifies which features to return.</typeparam>
        /// <returns>A list of lists of features per maptile.</returns>
        protected IReadOnlyList<IReadOnlyList<T>> GetFeaturesByTile<T>(
            IReadOnlyList<MeshTile> mapTiles)
            where T : IMapTileFeature
        {
            var featuresPerTile = new List<IReadOnlyList<T>>();

            foreach (var meshTile in mapTiles)
            {
                var mapTileFeatures = Features.IsEmpty()
                    ? GetAllFeaturesFromLayer<T>(meshTile.Tile, Layer)
                    : GetFeaturesFromLayer<T>(meshTile.Tile, Layer, Features);

                featuresPerTile.Add(mapTileFeatures);
            }

            return featuresPerTile;
        }

        /// <summary>
        /// Gets a list of <see cref="IMapTileFeature"/>s for a specific <see cref="MeshTile"/>.
        /// </summary>
        /// <param name="tile">The <see cref="MeshTile"/> whose features
        /// of type <typeparamref name="T"/> to retrieve</param>.
        /// <typeparam name="T">A feature type derived from
        /// <see cref="IMapTileFeature"/> that specifies which features to return.</typeparam>
        /// <returns>A list of features of the <see cref="MeshTile"/> at the specified index
        /// of the input <see cref="IReadOnlyList{T}"/></returns>
        protected IReadOnlyList<T> GetFeaturesForTile<T>(MeshTile tile)
            where T : IMapTileFeature
        {
            return Features.IsEmpty()
                ? GetAllFeaturesFromLayer<T>(tile.Tile, Layer)
                : GetFeaturesFromLayer<T>(tile.Tile, Layer, Features);
        }

        /// <summary>
        /// Gets all features of a given type in an <see cref="IMapTile"/>.  This
        /// method is called when the list of features for the current builder is empty.
        /// </summary>
        /// <param name="mapTile">The maptile containing features to extract.</param>
        /// <param name="layer">The layer associated with the returned features.</param>
        /// <typeparam name="T">A feature type derived from <see cref="IMapTileFeature"/>
        /// that specifies which features to return.</typeparam>
        /// <returns>A list of maptile features.</returns>
        private static IReadOnlyList<T> GetAllFeaturesFromLayer<T>(IMapTile mapTile, LayerKind layer)
            where T : IMapTileFeature
        {
            var features = new List<T>();

            foreach (var feature in mapTile.GetTileData(layer))
            {
                if (feature is T featureOfType)
                {
                    features.Add(featureOfType);
                }
            }

            return features;
        }

        /// <summary>
        /// Gets features of a given type and <see cref="FeatureKind"/> in an <see cref="IMapTile"/>
        /// </summary>
        /// <param name="mapTile">The maptile containing features to extract.</param>
        /// <param name="layer">The layer associated with the returned features.</param>
        /// <param name="featureKinds">The feature kinds to return.</param>
        /// <typeparam name="T">A feature type derived from <see cref="IMapTileFeature"/>
        /// that specifies which features to return.</typeparam>
        /// <returns>A list of maptile features.</returns>
        private static IReadOnlyList<T> GetFeaturesFromLayer<T>(
            IMapTile mapTile,
            LayerKind layer,
            List<FeatureKind> featureKinds)
            where T : IMapTileFeature
        {
            var features = new List<T>();

            foreach (var featureKind in featureKinds)
            {
                foreach (var feature in mapTile.GetTileData(layer, featureKind))
                {
                    if (feature is T featureOfType)
                    {
                        features.Add(featureOfType);
                    }
                }
            }

            return features;
        }

        /// <summary>
        /// Frees any resources used by the invalid <see cref="MeshTile"/>'s <see cref="IMapTileObject"/>s
        /// </summary>
        /// <param name="tiles">The set of <see cref="MeshTile"/>s that are not currently
        /// valid to be built by the current <see cref="MeshBuilderAsync"/></param>
        private void ClearInvalidTiles(IReadOnlyList<MeshTile> tiles)
        {
            int tileCount = tiles.Count;
            for (int i = 0; i < tileCount; i++)
            {
                tiles[i].MarkBuilderComplete(this);
                var filter = tiles[i].Filter;
                Destroy(filter.sharedMesh);
                SetMeshForFilter(null, filter);
            }
        }

        /// <summary>
        /// Applies the mesh that was generated via build Jobs to
        /// a Unity Mesh that will be rendered in the scene.
        /// </summary>
        /// <param name="tile">The current <see cref="MeshTile"/> being built</param>
        /// <param name="tileStruct"> A helper struct containing all relevant
        /// NativeCollections for the <see cref="tile"/>'s Build Jobs. This struct
        /// contains a <see cref="Mesh.MeshDataArray"/> (of length 1) whose only element
        /// is the mesh of all the current <see cref="MeshTile"/>'s features combined</param>
        /// <param name="token">Used to cancel the mesh assignment</param>
        internal void ApplyMesh(MeshTile tile, MapTileBuildStruct tileStruct, CancellationToken token)
        {
            ApplyMesh(tile, tileStruct.CombinedFeatureMeshes, token);
        }

        /// <summary>
        /// Applies the mesh that was generated via build Jobs
        /// to a Unity Mesh that will be rendered in the scene.
        /// </summary>
        /// <param name="tile">The current <see cref="MeshTile"/> being built</param>
        /// <param name="combinedMeshData"> <see cref="Mesh.MeshDataArray"/> (of length 1) whose only
        /// element is the mesh of all the current <see cref="MeshTile"/>'s features combined</param>
        /// <param name="token">Used to cancel the mesh assignment</param>
        internal void ApplyMesh(MeshTile tile, Mesh.MeshDataArray combinedMeshData, CancellationToken token)
        {
            if (ApplyMeshInternal(combinedMeshData, token, out var finalCombinedMesh))
            {
                SetMeshForFilter(finalCombinedMesh, tile.Filter);
            }
        }

        /// <summary>
        /// Outputs the <see cref="Mesh"/> populated from <paramref name="meshDataToApply"/>
        /// </summary>
        /// <param name="meshDataToApply">The <see cref="Mesh.MeshDataArray"/> whose first
        /// element contains the data needed to populate the <paramref name="outputMesh"/></param>
        /// <param name="token">Used to cancel the mesh assignment</param>
        /// <param name="outputMesh">The <see cref="Mesh"/> which will be
        /// populated with the data from <paramref name="meshDataToApply"/></param>
        /// <returns>Whether or not the Mesh was successfully applied</returns>
        protected bool ApplyMeshInternal(
            Mesh.MeshDataArray meshDataToApply,
            CancellationToken token,
            out Mesh outputMesh)
        {
            if (token.IsCancellationRequested)
            {
                meshDataToApply.Dispose();
                outputMesh = null;
                return false;
            }

            var result = meshDataToApply[0];

            result.subMeshCount = 1;

            var sm = new SubMeshDescriptor
            {
                indexStart = 0,
                indexCount = result.GetIndexData<int>().Length,
                firstVertex = 0,
                vertexCount = result.vertexCount
            };

            outputMesh = new Mesh();

            if (token.IsCancellationRequested)
            {
                meshDataToApply.Dispose();
                Destroy(outputMesh);
                outputMesh = null;
                return false;
            }

            result.SetSubMesh(0, sm);

            Mesh.ApplyAndDisposeWritableMeshData(meshDataToApply, outputMesh);

            if (token.IsCancellationRequested)
            {
                Destroy(outputMesh);
                outputMesh = null;
                return false;
            }

            outputMesh.RecalculateBounds();

            if (token.IsCancellationRequested)
            {
                Destroy(outputMesh);
                outputMesh = null;
                return false;
            }

            return true;
        }
    }
}
