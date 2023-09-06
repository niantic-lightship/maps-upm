// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps.Coordinates
{
    /// <summary>
    /// Editor-specific members of <see cref="SerializableLatLng"/>
    /// </summary>
    public partial class SerializableLatLng
    {
        /// <summary>
        /// Gets a <see cref="SerializedProperty"/> from
        /// a field on a <see cref="SerializedProperty"/>,
        /// for use in a custom property drawer.
        /// </summary>
        /// <param name="property">A property corresponding
        /// to the object whose field to return.</param>
        /// <returns>A <see cref="SerializedProperty"/> for this field.</returns>
        internal static SerializedProperty FindLatitudeProperty(SerializedProperty property)
            => property.FindPropertyRelative(nameof(_latitude));

        /// <inheritdoc cref="FindLatitudeProperty" />
        internal static SerializedProperty FindLongitudeProperty(SerializedProperty property)
            => property.FindPropertyRelative(nameof(_longitude));
    }
}

#endif
