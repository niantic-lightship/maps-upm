// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.Jobs
{
    /// <summary>
    /// Interface representing a manager of Build Jobs
    /// </summary>
    internal interface IBuilderJobMonitorManager
    {
        /// <summary>
        /// Creates, caches, and returns a JobMonitor for a given
        /// <see cref="Guid"/>-<see cref="IMapTileObject"/> pair.
        /// </summary>
        /// <param name="builderId">The ID of the feature
        /// builder building the <paramref name="tile"/>.</param>
        /// <param name="tile">The tile being built</param>
        /// <param name="monitor">The newly created <see cref="JobMonitor"/></param>
        /// <returns>True if the <see cref="Guid"/>-<see cref="IMapTileObject"/>
        /// pair has not already been cached, otherwise false.</returns>
        /// <remarks>If a <see cref="JobMonitor"/> is already cached for the
        /// <see cref="Guid"/>-<see cref="IMapTileObject"/> pair, the monitor is unregistered
        /// and replaced with a new one</remarks>
        bool TryRegisterMonitor(Guid builderId, IMapTileObject tile, out JobMonitor monitor);

        /// <summary>
        /// Attempts to remove the <see cref="JobMonitor"/> associated with the
        /// <see cref="Guid"/>-<see cref="IMapTileObject"/> pair from the cache.
        /// </summary>
        /// <param name="builderId">The ID of the feature
        /// builder building the <paramref name="tile"/>.</param>
        /// <param name="tile">The tile being built</param>
        /// <param name="destroyOnUnregister"></param>
        /// <returns>Whether or not the monitor was successfully unregistered</returns>
        bool TryUnregisterMonitor(Guid builderId, IMapTileObject tile, bool destroyOnUnregister = false);

        /// <summary>
        /// Attempts to get the <see cref="JobMonitor"/> associated with the
        /// <see cref="Guid"/>-<see cref="IMapTileObject"/> pair from the cache.
        /// </summary>
        /// <param name="builderId">The ID of the feature
        /// builder building the <paramref name="tile"/>.</param>
        /// <param name="tile">The tile being built</param>
        /// <param name="monitor">The <see cref="JobMonitor"/> to return</param>
        /// <returns>True if the monitor was found.</returns>
        bool TryGetMonitor(Guid builderId, IMapTileObject tile, out JobMonitor monitor);
    }
}
