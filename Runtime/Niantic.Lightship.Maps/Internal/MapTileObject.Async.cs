// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Builders.Performance;
using Niantic.Lightship.Maps.Builders.Standard;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.Jobs;
using Niantic.Lightship.Maps.Linq;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using IBuilderJobMonitorManager = Niantic.Lightship.Maps.Builders.Performance.Utils.Interfaces.IBuilderJobMonitorManager;

namespace Niantic.Lightship.Maps.Internal
{
    internal partial class MapTileObject : IBuilderJobMonitorManager
    {
        /// <summary>
        /// Used to alert any <see cref="TilePair"/>s with
        /// this <see cref="MapTileObject"/> that the pooled
        /// <see cref="MapTileObject"/> needs to be reused.
        /// </summary>
        private CancellationTokenSource _tokenSource = new();

        /// <summary>
        /// Used to register and unregister Jobs via <see cref="Jobs.JobMonitor"/>s
        /// </summary>
        private Jobs.IBuilderJobMonitorManager _builderJobMonitorManager;

        /// <summary>
        /// Triggers the build process for all of the <see cref="mapTiles"/>
        /// <para>Builders derived from <see cref="IFeatureBuilder"/>
        /// process all <see cref="mapTiles"/> in bulk.<br/>
        /// All other builders process tiles one at a time</para>.
        /// </summary>
        /// <param name="mapTiles">The tiles to be built</param>
        /// <param name="builderJobMonitorManager">The object managing tile building Jobs</param>
        public static void Build(
            IReadOnlyList<TilePair> mapTiles,
            Jobs.IBuilderJobMonitorManager builderJobMonitorManager)
        {
            // Get all the builders from the tiles to be built
            var builders = mapTiles.SelectManyUnique(t => t.MapTileObject.Builders);

            // Cancel any Build processes currently in progress for a given MapTileObject
            foreach (var tile in mapTiles)
            {
                var mto = tile.MapTileObject;
                mto._builderJobMonitorManager ??= builderJobMonitorManager;
                mto._tokenSource.Cancel();
                mto._tokenSource.Dispose();
                mto._tokenSource = new CancellationTokenSource();

                tile.SetCancellationToken(mto._tokenSource.Token);
            }

            var meshTilesAsync = new Dictionary<IMeshBuilderAsync, IReadOnlyList<MeshTile>>();
            var objectTilesAsync = new Dictionary<IObjectBuilderAsync, IReadOnlyList<ObjectTile>>();
            var tilePairsStandard = new Dictionary<IFeatureBuilder, IReadOnlyList<TilePair>>();

            // Grab the necessary additional references for various builder types
            foreach (var builder in builders)
            {
                switch (builder)
                {
                    case IMeshBuilderAsync meshBuilderAsync:
                        meshTilesAsync.Add(meshBuilderAsync, GetMeshFiltersAndTiles(builder, mapTiles));
                        break;

                    case IObjectBuilderAsync objectBuilderAsync:
                        objectTilesAsync.Add(objectBuilderAsync, GetParentsAndTiles(builder, mapTiles));
                        break;

                    case not null:
                        tilePairsStandard.Add(builder, mapTiles);
                        break;
                }
            }

            // Build tiles in async mesh builders
            foreach (var tilesByBuilder in meshTilesAsync)
            {
                var builder = tilesByBuilder.Key;

                if (builder.PreBuild(tilesByBuilder.Value, out var validTiles))
                {
                    builder.Build(validTiles);
                }
            }

            // Build tiles in async object builders
            foreach (var tilesByBuilder in objectTilesAsync)
            {
                tilesByBuilder.Key.Build(tilesByBuilder.Value);
            }

            // Build tiles in standard builders
            foreach (var (builder, tiles) in tilePairsStandard)
            {
                var tileCount = tiles.Count;
                for (int i = 0; i < tileCount; i++)
                {
                    var tile = tiles[i];
                    BuildStandardBuilder(builder, tile);
                    tile.MarkBuilderComplete(builder);
                }
            }
        }

        /// <summary>
        /// Completes the Build process of a given <paramref name="tile"/>
        /// for a given Standard <see cref="IFeatureBuilder"/>.
        /// </summary>
        /// <param name="builder">The builder that will build the tile's features</param>
        /// <param name="tile">The tile whose features will be built</param>
        private static void BuildStandardBuilder(IFeatureBuilder builder, TilePair tile)
        {
            var mapTile = tile.Tile;
            if (MathEx.IsBetween(mapTile.ZoomLevel, builder.MinLOD, builder.MaxLOD))
            {
                switch (builder)
                {
                    case IMeshBuilderStandard meshBuilder:
                        meshBuilder.Build(mapTile, tile.MapTileObject._meshesByBuilder[builder.Id]);
                        break;

                    case IObjectBuilderStandard objectBuilder:
                        objectBuilder.Build(mapTile, tile.MapTileObject._objectBuilderParents[builder.Id]);
                        break;
                }
            }
        }

        /// <summary>
        /// Filters an <see cref="IMeshBuilderAsync"/>'s
        /// tiles into only those containing that builder.
        /// </summary>
        /// <param name="builder">A key-value pair consisting of a builder
        /// and all <see cref="TilePair"/>s associated with it.</param>
        /// <param name="tilePairs">The tiles to be built</param>
        /// <returns>A filtered list of <see cref="MeshTile"/>s</returns>
        private static IReadOnlyList<MeshTile> GetMeshFiltersAndTiles(
            IFeatureBuilder builder,
            IReadOnlyList<TilePair> tilePairs)
        {
            var id = builder.Id;

            bool ContainsMeshBuilder(TilePair t) =>
                ((MapTileObject)t.TileObject)._meshesByBuilder.ContainsKey(id);

            var tiles = tilePairs
                .SelectWhere(
                    ContainsMeshBuilder,
                    t => new MeshTile(t, t.MapTileObject._meshesByBuilder[id]));

            return tiles;
        }

        /// <summary>
        /// Filters an <see cref="IObjectBuilderAsync"/>'s
        /// tiles into only those containing that builder.
        /// </summary>
        /// <param name="builder">A key-value pair consisting of a builder
        /// and all <see cref="TilePair"/>s associated with it.</param>
        /// <param name="tilePairs">The tiles to be built</param>
        /// <returns>A filtered list of <see cref="ObjectTile"/>s</returns>
        private static IReadOnlyList<ObjectTile> GetParentsAndTiles(IFeatureBuilder builder,
            IReadOnlyList<TilePair> tilePairs)
        {
            var id = builder.Id;

            bool ContainsObjectBuilder(TilePair t) =>
                t.MapTileObject._objectBuilderParents.ContainsKey(id);

            var tiles = tilePairs
                .SelectWhere(
                    ContainsObjectBuilder,
                    t => new ObjectTile(t, t.MapTileObject._objectBuilderParents[id]));

            return tiles;
        }

        /// <inheritdoc />
        JobMonitor IBuilderJobMonitorManager.GetMonitor(IFeatureBuilder builder)
        {
            _builderJobMonitorManager.TryGetMonitor(builder.Id, this, out var monitor);
            return monitor;
        }

        /// <inheritdoc />
        JobMonitor IBuilderJobMonitorManager.CreateJobMonitor(IFeatureBuilder builder)
        {
            if (!_builderJobMonitorManager.TryRegisterMonitor(builder.Id, this, out var monitor))
            {
                _builderJobMonitorManager.TryUnregisterMonitor(builder.Id, this, true);
            }

            _builderJobMonitorManager.TryRegisterMonitor(builder.Id, this, out monitor);

            return monitor;
        }
    }
}
