// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Builders.Standard.LinearFeatures;
using Niantic.Lightship.Maps.Builders.Standard.LinearFeatures.Editor;
using UnityEditor;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps.Builders.Performance.LinearFeatures
{
    /// <inheritdoc cref="ILinearFeatureBuilderEditor" />
    public partial class LinearFeatureBuilderAsync : ILinearFeatureBuilderEditor
    {
        #region ILinearFeatureBuilderEditor

        /// <inheritdoc />
        LinearFeatureSize ILinearFeatureBuilderEditor.LinearFeatureSize => _linearFeatureSize;

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        float ILinearFeatureBuilderEditor.LinearFeatureWidth => _linearFeatureWidth;

        /// <inheritdoc />
        SerializedProperty ILinearFeatureBuilderEditor.LinearFeatureSizeProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_linearFeatureSize));

        /// <inheritdoc />
        SerializedProperty ILinearFeatureBuilderEditor.CustomLinearFeatureMinProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_customLinearFeatureMin));

        /// <inheritdoc />
        SerializedProperty ILinearFeatureBuilderEditor.CustomLinearFeatureMaxProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_customLinearFeatureMax));

        #endregion
    }
}

#endif
