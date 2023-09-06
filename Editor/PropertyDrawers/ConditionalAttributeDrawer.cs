// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Attributes;
using Niantic.Lightship.Maps.Utilities;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PropertyDrawers
{
    /// <summary>
    /// A <see cref="PropertyDrawer"/> used to customize
    /// the behavior of a serialized field that has a
    /// <see cref="ConditionalAttribute"/> applied to it.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalAttribute), true)]
    internal class ConditionalAttributeDrawer : PropertyDrawer
    {
        private bool _isHidden;
        private bool _isDisabled;

        private static ChannelLogger Log { get; } = new(nameof(ConditionalAttributeDrawer));

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (attribute is ConditionalAttribute conditionalAttribute)
            {
                // Find the other serialized field based on the property
                // path specified in this attribute's constructor.
                var serializedObj = property.serializedObject;
                var propertyPath = conditionalAttribute.ConditionalPropertyPath;
                var conditionalProperty = serializedObj.FindProperty(propertyPath);
                var conditionalPropertyType = conditionalProperty.propertyType;

                // Make sure the other property has a boolean value
                if (conditionalPropertyType != SerializedPropertyType.Boolean)
                {
                    var name = conditionalProperty.name;
                    var type = conditionalPropertyType.ToString();
                    Log.Error($"Conditional property '{name}' is of type '{type}' rather than bool");
                }
                else
                {
                    var value = conditionalProperty.boolValue;
                    _isHidden = conditionalAttribute.IsHidden(value);
                    _isDisabled = conditionalAttribute.IsDisabled(value);
                }
            }
            else
            {
                Log.Error($"Unexpected attribute type: '{attribute.GetType().Name}'");
            }

            return _isHidden
                ? -EditorGUIUtility.standardVerticalSpacing
                : EditorGUI.GetPropertyHeight(property);
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_isHidden)
            {
                using (new EditorGUI.DisabledScope(_isDisabled))
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
    }
}
