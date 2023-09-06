// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Builders.Standard.Editor;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps.Builders.Standard
{
    /// <summary>
    /// Editor-specific members of <see cref="MeshBuilderStandard"/>
    /// </summary>
    public partial class MeshBuilderStandard : IMeshBuilderWritable
    {
        /// <inheritdoc />
        Material[] IMeshBuilderWritable.Materials { set => _materials = value; }
    }
}

#endif
