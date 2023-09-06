// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;

namespace Niantic.Lightship.Maps.Builders.Editor
{
    /// <summary>
    /// This interface is meant to be used by custom
    /// editors of type <see cref="UnityEditor.Editor"/>.
    /// </summary>
    internal interface IFeatureBuilderEditor : IFeatureBuilderWritable
    {
        /// <summary>
        /// Gets a <see cref="SerializedProperty"/> from a field on a
        /// <see cref="SerializedObject"/>, for use in a custom editor.
        /// </summary>
        /// <param name="serializedObject">The object whose field to return.</param>
        /// <returns>A <see cref="SerializedProperty"/> for this field.</returns>
        SerializedProperty MapLayerProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty UndefinedFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty BoundaryFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty StructureFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty LandUseFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty PlacesFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty LinearFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty TransitFeaturesProperty(SerializedObject serializedObject);

        /// <inheritdoc cref="MapLayerProperty" />
        SerializedProperty WaterFeaturesProperty(SerializedObject serializedObject);
    }
}

#endif
