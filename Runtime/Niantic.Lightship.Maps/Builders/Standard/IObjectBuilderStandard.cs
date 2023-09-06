// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard
{
    /// <inheritdoc />
    [PublicAPI]
    public interface IObjectBuilderStandard : IObjectBuilder
    {
        /// <summary>
        /// Called when a maptile is added to the scene.  This method
        /// instantiates objects for a specific set of maptile features.
        /// </summary>
        /// <param name="mapTile">The <see cref="IMapTile"/>
        /// containing features to build.</param>
        /// <param name="parent">The <see cref="GameObject"/>
        /// instantiated by <see cref="IObjectBuilder.CreateParent"/>
        /// that this builder's objects will be parented to.</param>
        void Build(IMapTile mapTile, GameObject parent);
    }
}
