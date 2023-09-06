// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Niantic.Lightship.Maps
{
    /// <summary>
    /// An object representing a single maptile in the scene.
    /// </summary>
    [PublicAPI]
    public interface IMapTileObject
    {
        /// <summary>
        /// The <see cref="Transform"/> belonging to
        /// this <see cref="IMapTileObject"/>'s Unity
        /// <see cref="UnityObject"/> in the current scene.
        /// </summary>
        Transform Transform { get; }
    }
}
