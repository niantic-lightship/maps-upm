// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Core;

namespace Niantic.Lightship.Maps.MapTileObjectHelpers
{
    /// <summary>
    /// <para>
    /// Represents an <see cref="IMapTileObject"/> and its
    /// corresponding <see cref="IMapTile"/> at any given time.
    /// </para>
    ///
    /// <para>
    /// This object is always paired to a given <see cref="IMapTileObject"/>
    /// while the <see cref="IMapTile"/> being referenced can change when
    /// the pooled <see cref="IMapTileObject"/> is reused.
    /// </para>
    /// </summary>
    [PublicAPI]
    public interface ITilePair
    {
        /// <summary>
        /// An object representing a single maptile in the scene.
        /// </summary>
        public IMapTileObject TileObject { get; }

        /// <summary>
        /// Contains the feature and coordinate information for the maptile
        /// </summary>
        public IMapTile Tile { get; }

        /// <summary>
        /// Notifies the tile that the builder has finished building
        /// it or will no longer build it (i.e., due to cancellation or exception handling)
        /// </summary>
        /// <param name="builder">The builder currently referencing/building this tile</param>
        public void MarkBuilderComplete(IFeatureBuilder builder);
    }
}
