// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;

namespace Niantic.Lightship.Maps.Builders.Standard.LinearFeatures.Editor
{
    /// <summary>
    /// This interface is meant to be used by custom
    /// editors of type <see cref="UnityEditor.Editor"/>.
    /// </summary>
    internal interface ILinearFeatureBuilderEditor
    {
        /// <summary>
        /// The builder's pre-set <see cref="LinearFeatureSize"/>
        /// </summary>
        LinearFeatureSize LinearFeatureSize { get; }

        /// <summary>
        /// The most recent width calculated by this builder
        /// </summary>
        float LinearFeatureWidth { get; }

        /// <summary>
        /// Gets a <see cref="SerializedProperty"/> from a field on a
        /// <see cref="SerializedObject"/>, for use in a custom editor.
        /// </summary>
        /// <param name="serializedObject">The object whose field to return.</param>
        /// <returns>A <see cref="SerializedProperty"/> for this field.</returns>
        SerializedProperty LinearFeatureSizeProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="LinearFeatureSizeProperty" />
        SerializedProperty CustomLinearFeatureMinProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="LinearFeatureSizeProperty" />
        SerializedProperty CustomLinearFeatureMaxProperty(SerializedObject serializedObject);
    }
}

#endif
