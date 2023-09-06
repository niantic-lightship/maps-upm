// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Builders.Editor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.Editor
{
    /// <summary>
    /// This internal, Editor-only interface exposes serialized fields
    /// and other methods that are meant to be used to modify internal
    /// state of serialized builder assets programmatically.
    /// </summary>
    internal interface IMeshBuilderWritable : IFeatureBuilderWritable
    {
        /// <summary>
        /// Materials applied to generated meshes
        /// </summary>
        Material[] Materials { set; }
    }
}

#endif
