// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard
{
    /// <inheritdoc />
    [PublicAPI]
    public interface IMeshBuilderStandard : IMeshBuilder
    {
        /// <summary>
        /// Called when a maptile is added to the scene.  This method
        /// generates meshes for a specific set of maptile features.
        /// </summary>
        /// <param name="mapTile">The <see cref="IMapTile"/> containing features to build.</param>
        /// <param name="meshFilter">The <see cref="MeshFilter"/> used to render the mesh.</param>
        void Build(IMapTile mapTile, MeshFilter meshFilter);
    }
}
