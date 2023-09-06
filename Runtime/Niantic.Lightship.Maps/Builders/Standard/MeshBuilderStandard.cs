// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard
{
    /// <inheritdoc cref="IMeshBuilderStandard" />
    [PublicAPI]
    public abstract partial class MeshBuilderStandard : MeshBuilderBase, IMeshBuilderStandard
    {
        /// <inheritdoc />
        public abstract void Build(IMapTile mapTile, MeshFilter meshFilter);
    }
}
