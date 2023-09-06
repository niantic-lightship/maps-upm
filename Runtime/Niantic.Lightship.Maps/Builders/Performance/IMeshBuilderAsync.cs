// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Performance
{
    /// <summary>
    /// Represents a maptile feature builder which builds
    /// feature <see cref="Mesh"/>es asynchronously.
    /// </summary>
    [PublicAPI]
    public interface IMeshBuilderAsync : IMeshBuilder
    {
        /// <summary>
        /// Processes tiles before building
        /// </summary>
        /// <param name="tiles">The initial list of tiles to be built</param>
        /// <param name="tilesToBuild">The processed tiles
        /// that are currently valid for this builder.</param>
        /// <returns>Whether or not the build should continue</returns>
        /// <remarks>Valid tiles are typically those whose
        /// Zoom Level is between MinLOD and MaxLOD.</remarks>
        bool PreBuild(IReadOnlyList<MeshTile> tiles, out IReadOnlyList<MeshTile> tilesToBuild);

        /// <summary>
        /// Called when one or more maptiles are added to the scene.  This
        /// method generates meshes for a specific set of maptile features.
        /// </summary>
        /// <param name="tiles">An <see cref="IReadOnlyList{T}"/>
        /// of <see cref="MeshTile"/>s containing features to build</param>
        void Build(IReadOnlyList<MeshTile> tiles);
    }
}
