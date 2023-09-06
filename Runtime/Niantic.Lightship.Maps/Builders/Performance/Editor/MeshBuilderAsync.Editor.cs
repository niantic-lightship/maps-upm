// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Builders.Standard.Editor;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps.Builders.Performance
{
    /// <summary>
    /// Editor-specific members of <see cref="MeshBuilderAsync"/>
    /// </summary>
    public abstract partial class MeshBuilderAsync : IMeshBuilderWritable
    {
        /// <inheritdoc />
        Material[] IMeshBuilderWritable.Materials { set => _materials = value; }
    }
}

#endif
