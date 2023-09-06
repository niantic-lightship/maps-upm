// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Threading;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Builders.Performance;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Interfaces;
using Niantic.Lightship.Maps.Core;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapTileObjectHelpers
{
    /// <summary>
    /// An <see cref="ObjectBuilderAsync"/>-friendly
    /// wrapper for relevant MapTile references.
    /// </summary>
    [PublicAPI]
    public class ObjectTile : ITilePair
    {
        #region ITilePair

        /// <inheritdoc/>
        public IMapTileObject TileObject => _tilePair.TileObject;

        /// <inheritdoc/>
        public IMapTile Tile => _tilePair.Tile;

        /// <inheritdoc />
        public void MarkBuilderComplete(IFeatureBuilder builder)
        {
            _tilePair.MarkBuilderComplete(builder);
        }

        #endregion

        /// <summary>
        /// The <see cref="GameObject"/> this
        /// tile's features will be built under.
        /// </summary>
        public readonly GameObject Parent;

        /// <summary>
        /// The <see cref="ObjectTile"/>'s
        /// underlying <see cref="TilePair"/>.
        /// </summary>
        private readonly TilePair _tilePair;

        /// <summary>
        /// A token used to cancel this TilePair's build
        /// process when the <see cref="IMapTileObject"/>
        /// needs to be reused by a new <see cref="IMapTile"/>.
        /// </summary>
        internal CancellationToken Token => _tilePair.Token;

        /// <summary>
        /// Used to get the tile's <see cref="Jobs.JobMonitor"/>
        /// </summary>
        internal IBuilderJobMonitorManager Monitor => _tilePair.Monitor;

        /// <summary>
        /// Associates a <see cref="TilePair"/> with the <see cref="GameObject"/>
        /// used for a given <see cref="ObjectBuilderAsync"/>.
        /// </summary>
        /// <param name="tilePair">Represents the pairing of a <see cref="IMapTileObject"/>
        /// and the <see cref="IMapTile"/> whose features will be built on it.</param>
        /// <param name="parent">The <see cref="GameObject"/> parenting the
        /// features for a given <see cref="ObjectBuilderAsync"/> on the
        /// corresponding <see cref="IMapTileObject"/>.</param>
        public ObjectTile(TilePair tilePair, GameObject parent)
        {
            _tilePair = tilePair;
            Parent = parent;
        }

    }
}
