// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders
{
    /// <summary>
    /// The base type for maptile feature builders that generate meshes.
    /// </summary>
    [PublicAPI]
    public interface IMeshBuilder : IFeatureBuilder
    {
        /// <summary>
        /// Creates <see cref="MeshRenderer"/> and <see cref="MeshFilter"/> components
        /// on a new <see cref="GameObject"/> that are used to render generated meshes.
        /// </summary>
        /// <param name="mapTileObject">The <see cref="IMapTileObject"/> that
        /// the new mesh rendering components will be attached to.</param>
        /// <returns>The <see cref="MeshFilter"/> component</returns>
        MeshFilter CreateMeshComponents(IMapTileObject mapTileObject);

        /// <summary>
        /// Called when a maptile is removed from the scene.
        /// This method cleans up meshes or other resources
        /// created when the maptile is built by this builder.
        /// </summary>
        /// <param name="meshFilter">The <see cref="MeshFilter"/>
        /// created by the call to <see cref="CreateMeshComponents"/></param>
        void Release(MeshFilter meshFilter);
    }
}
