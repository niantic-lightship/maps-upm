// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Niantic.Lightship.Maps.Builders.Performance
{
    /// <summary>
    /// Represents a maptile feature builder which builds
    /// feature <see cref="GameObject"/>s asynchronously.
    /// </summary>
    [PublicAPI]
    public interface IObjectBuilderAsync : IObjectBuilder
    {
        /// <summary>
        /// Called when a maptile is added to the scene.  This method
        /// instantiates objects for a specific set of maptile features.
        /// </summary>
        /// <param name="tiles">An <see cref="IReadOnlyList{T}"/>
        /// of <see cref="ObjectTile"/>s containing features to build.</param>
        void Build(IReadOnlyList<ObjectTile> tiles);
    }
}
