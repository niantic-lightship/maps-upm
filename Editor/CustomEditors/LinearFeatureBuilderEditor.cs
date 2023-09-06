// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Globalization;
using Niantic.Lightship.Maps.Builders.Performance.LinearFeatures;
using Niantic.Lightship.Maps.Builders.Standard.LinearFeatures;
using Niantic.Lightship.Maps.Builders.Standard.LinearFeatures.Editor;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.CustomEditors
{
    [CustomEditor(typeof(LinearFeatureBuilderAsync), true)]
    internal class LinearFeatureBuilderEditorAsync : LinearFeatureBuilderEditor
    {
    }

    [CustomEditor(typeof(LinearFeatureBuilder), true)]
    internal class LinearFeatureBuilderEditor : FeatureBuilderBaseEditor
    {
        private ILinearFeatureBuilderEditor _linearFeatureBuilder;
        private SerializedObject _roadValues;
        private SerializedProperty _roadSizeValues;

        private bool _showRoadsSection;

        #region Linear feature size values

        private SerializedProperty _roadSize;

        private SerializedProperty _customRoadMin;
        private SerializedProperty _customRoadMax;

        #endregion

        internal override void OnEnable()
        {
            base.OnEnable();
            _linearFeatureBuilder = (ILinearFeatureBuilderEditor)target;

            _roadSize = _linearFeatureBuilder.LinearFeatureSizeProperty(serializedObject);
            _showRoadsSection = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            #region Linear feature size values

            _customRoadMin = _linearFeatureBuilder.CustomLinearFeatureMinProperty(serializedObject);
            _customRoadMax = _linearFeatureBuilder.CustomLinearFeatureMaxProperty(serializedObject);

            #endregion

            if (Application.isPlaying)
            {
                var linearFeatureSize = _linearFeatureBuilder.LinearFeatureSize;
                var linearFeatureWidth = _linearFeatureBuilder.LinearFeatureWidth;

                var labelString = $"Linear Feature Size: {linearFeatureSize.ToString()}";
                EditorGUILayout.LabelField(labelString, EditorStyles.boldLabel);
                ShowReadOnlyValues();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Current Linear Feature Width: ");
                GUILayout.FlexibleSpace();

                GUILayout.Label(linearFeatureWidth.ToString(CultureInfo.InvariantCulture));
                GUILayout.EndHorizontal();
            }
            else
            {
                _showRoadsSection = EditorGUILayout.BeginFoldoutHeaderGroup(
                    _showRoadsSection, "Linear Feature Size Settings");

                if (_showRoadsSection)
                {
                    EditorGUILayout.PropertyField(_roadSize);

                    var linearFeatureSize = _linearFeatureBuilder.LinearFeatureSize;

                    if (linearFeatureSize == LinearFeatureSize.Custom)
                    {
                        ShowCustomFields();
                    }
                    else
                    {
                        ShowReadOnlyValues();
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                _roadValues?.ApplyModifiedProperties();
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        private void ShowReadOnlyValues()
        {
            var min = (LinearFeatureSize)_roadSize.enumValueIndex switch
            {
                LinearFeatureSize.Custom => _customRoadMin.floatValue,
                LinearFeatureSize.Small => LinearFeatureSizeSettings.SmallLinearFeatureMin,
                LinearFeatureSize.Medium => LinearFeatureSizeSettings.MedLinearFeatureMin,
                LinearFeatureSize.Large => LinearFeatureSizeSettings.LargeLinearFeatureMin,
                _ => throw new ArgumentOutOfRangeException()
            };

            var max = (LinearFeatureSize)_roadSize.enumValueIndex switch
            {
                LinearFeatureSize.Custom => _customRoadMax.floatValue,
                LinearFeatureSize.Small => LinearFeatureSizeSettings.SmallLinearFeatureMax,
                LinearFeatureSize.Medium => LinearFeatureSizeSettings.MedLinearFeatureMax,
                LinearFeatureSize.Large => LinearFeatureSizeSettings.LargeLinearFeatureMax,
                _ => throw new ArgumentOutOfRangeException()
            };

            GUILayout.BeginHorizontal();
            GUILayout.Label("Minimum Width: ");
            GUILayout.FlexibleSpace();
            GUILayout.Label(min.ToString(CultureInfo.InvariantCulture));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Maximum Width: ");
            GUILayout.FlexibleSpace();
            GUILayout.Label(max.ToString(CultureInfo.InvariantCulture));
            GUILayout.EndHorizontal();
        }

        private void ShowCustomFields()
        {
            EditorGUILayout.PropertyField(_customRoadMin);
            EditorGUILayout.PropertyField(_customRoadMax);
        }
    }
}
