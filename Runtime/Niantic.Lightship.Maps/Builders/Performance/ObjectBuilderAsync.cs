// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.MapTileObjectHelpers;

namespace Niantic.Lightship.Maps.Builders.Performance
{
    /// <inheritdoc cref="IObjectBuilderAsync" />
    [PublicAPI]
    public abstract class ObjectBuilderAsync : ObjectBuilderBase, IObjectBuilderAsync
    {
        /// <inheritdoc />
        public abstract void Build(IReadOnlyList<ObjectTile> tiles);
    }
}
