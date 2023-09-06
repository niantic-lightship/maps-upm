// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Threading;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Builders.Performance;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Interfaces;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Internal;

namespace Niantic.Lightship.Maps.MapTileObjectHelpers
{
    /// <inheritdoc/>
    [PublicAPI]
    public class TilePair : ITilePair
    {
        #region ITilePair

        /// <inheritdoc/>
        public IMapTileObject TileObject => MapTileObject;

        /// <inheritdoc/>
        public IMapTile Tile { get; }

        /// <inheritdoc />
        public void MarkBuilderComplete(IFeatureBuilder builder)
        {
            if (--_pendingBuilderCount == 0)
            {
                _onTileBuildComplete?.Invoke(this);
                _onTileBuildComplete = null;
            }
        }

        #endregion

        /// <summary>
        /// A token used to cancel this TilePair's build
        /// process when the <see cref="IMapTileObject"/>
        /// needs to be reused by a new <see cref="IMapTile"/>.
        /// </summary>
        internal CancellationToken Token { get; private set; }

        /// <summary>
        /// Used to get the tile's <see cref="Jobs.JobMonitor"/>
        /// </summary>
        internal IBuilderJobMonitorManager Monitor { get; }

        /// <summary>
        /// An object representing a single maptile in the scene.
        /// </summary>
        internal MapTileObject MapTileObject { get; }

        private Action<TilePair> _onTileBuildComplete;
        private int _pendingBuilderCount;

        internal TilePair(MapTileObject tileObject, IMapTile tile, Action<TilePair> onBuildComplete)
        {
            MapTileObject = tileObject;
            Monitor = tileObject;
            Tile = tile;
            _onTileBuildComplete = onBuildComplete;

            foreach (var builder in tileObject.Builders)
            {
                if (builder is IMeshBuilderAsync or IObjectBuilderAsync)
                {
                    _pendingBuilderCount++;
                }
            }
        }

        internal void SetCancellationToken(CancellationToken token)
        {
            Token = token;
        }
    }
}
