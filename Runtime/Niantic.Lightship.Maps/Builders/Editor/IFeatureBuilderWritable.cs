// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Core.Features;

namespace Niantic.Lightship.Maps.Builders.Editor
{
    /// <summary>
    /// This internal, Editor-only interface exposes serialized fields
    /// and other methods that are meant to be used to modify internal
    /// state of serialized builder assets programmatically.
    /// </summary>
    internal interface IFeatureBuilderWritable
    {
        /// <summary>
        /// An offset applied to generated meshes to prevent z-fighting
        /// </summary>
        float ZOffset { set; }

        /// <summary>
        /// The builder's name (primarily for display
        /// in the Editor's Hierarchy window).
        /// </summary>
        string BuilderName { set; }

        /// <summary>
        /// Updates the main features list with values from the
        /// per-layer list corresponding to the selected layer
        /// </summary>
        /// <returns>The current selected layer</returns>
        LayerKind UpdateMapLayerFeatures();

        /// <summary>
        /// Adds all available features from the selected layer
        /// </summary>
        void AddAllMapLayerFeatures();

        /// <summary>
        /// Clears all features from the selected layer
        /// </summary>
        void ClearAllMapLayerFeatures();
    }
}

#endif
