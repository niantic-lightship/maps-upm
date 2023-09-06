// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Coordinates;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PropertyDrawers
{
    /// <summary>
    /// Custom <see cref="PropertyDrawer"/> for showing
    /// editable <see cref="SerializableLatLng"/> fields
    /// in a single row in the Inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableLatLng))]
    internal class SerializableLatLngDrawer : PropertyDrawer
    {
        // Latitude and longitude prefix labels
        private static readonly GUIContent LatLabel = new("Lat:", null, "Latitude");
        private static readonly GUIContent LngLabel = new("Lng:", null, "Longitude");

        // Calculate the width of the prefix labels above
        private static readonly float LatLabelWidth = EditorStyles.label.CalcSize(LatLabel).x;
        private static readonly float LngLabelWidth = EditorStyles.label.CalcSize(LngLabel).x;

        // Padding between UI elements
        private static readonly RectOffset LabelPadding = new(0, 6, 0, 0);
        private static readonly RectOffset LatPadding = new(0, 6, 0, 0);
        private static readonly RectOffset LngPadding = new(0, 0, 0, 0);

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get SerializedProperty instances for serialized lat/lng fields
            var latProperty = SerializableLatLng.FindLatitudeProperty(property);
            var lngProperty = SerializableLatLng.FindLongitudeProperty(property);

            // Add a prefix label for the serialized property's name
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var remaining = EditorGUI.PrefixLabel(position, controlId, label);

            // Calculate positions used for layout
            var width = remaining.width / 2.0f;
            var height = remaining.height;
            var top = remaining.y;
            var left = remaining.x;
            var midl = left + width;

            // Calculate the size of the latitude and longitude controls
            var latRect = LatPadding.Remove(new Rect(left, top, width, height));
            var lngRect = LngPadding.Remove(new Rect(midl, top, width, height));

            // Calculate the size of the latitude and longitude field labels
            var latLabelRect = LabelPadding.Add(new Rect(latRect.x, latRect.y, LatLabelWidth, height));
            var lngLabelRect = LabelPadding.Add(new Rect(lngRect.x, lngRect.y, LngLabelWidth, height));

            // Calculate the width of the lat/lng value edit fields
            var latFieldWidth = latRect.width - latLabelRect.width;
            var lngFieldWidth = lngRect.width - lngLabelRect.width;

            // Calculate the x position of the edit fields
            var latFieldX = latRect.x + latLabelRect.width;
            var lngFieldX = lngRect.x + lngLabelRect.width;

            // Calculate the size of the latitude and longitude value edit fields
            var latFieldRect = new Rect(latFieldX, latRect.y, latFieldWidth, height);
            var lngFieldRect = new Rect(lngFieldX, lngRect.y, lngFieldWidth, height);

            // Add label prefixes for the edit fields
            EditorGUI.LabelField(latLabelRect, LatLabel);
            EditorGUI.LabelField(lngLabelRect, LngLabel);

            EditorGUI.BeginChangeCheck();

            // Add double fields for editing latitude and longitude values
            var latitude = EditorGUI.DoubleField(latFieldRect, latProperty.doubleValue);
            var longitude = EditorGUI.DoubleField(lngFieldRect, lngProperty.doubleValue);

            if (EditorGUI.EndChangeCheck())
            {
                // Save the new latitude and longitude values back to
                // their serialized fields when either value changes.

                latProperty.doubleValue = latitude;
                lngProperty.doubleValue = longitude;

                // Apply changes back to the parent serialized object
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}
