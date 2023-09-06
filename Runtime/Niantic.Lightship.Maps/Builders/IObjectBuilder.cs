// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Niantic.Lightship.Maps.Builders
{
    /// <summary>
    /// The base type for maptile feature builders that instantiate Unity
    /// <see cref="Object"/>s attached to an <see cref="IMapTileObject"/>.
    /// </summary>
    [PublicAPI]
    public interface IObjectBuilder : IFeatureBuilder
    {
        /// <summary>
        /// Creates a <see cref="GameObject"/> that all <see cref="Object"/>s
        /// instantiated by this builder will be parented to.
        /// </summary>
        /// <param name="mapTileObject"> The <see cref="IMapTileObject"/> that
        /// the parent <see cref="GameObject"/> will be attached to.</param>
        /// <returns>A new parent <see cref="GameObject"/></returns>
        GameObject CreateParent(IMapTileObject mapTileObject);

        /// <summary>
        /// Called when a maptile is removed from the scene.
        /// This method cleans up objects or other resources
        /// created when the maptile is built by this builder.
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/>
        /// created by the call to <see cref="CreateParent"/>.</param>
        void Release(GameObject parent);
    }
}
