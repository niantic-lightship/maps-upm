// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Builders.Editor;
using Niantic.Lightship.Maps.Core.Features;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.CustomEditors
{
    [CustomEditor(typeof(FeatureBuilderBase), true)]
    internal class FeatureBuilderBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty _mapLayer;
        private SerializedProperty _currentFeatures;
        private IFeatureBuilderEditor _builderBase;

        #region Per-layer feature kinds

        private SerializedProperty _undefinedFeatures;
        private SerializedProperty _boundaryFeatures;
        private SerializedProperty _buildingFeatures;
        private SerializedProperty _landUseFeatures;
        private SerializedProperty _placesFeatures;
        private SerializedProperty _roadFeatures;
        private SerializedProperty _transitFeatures;
        private SerializedProperty _waterFeatures;

        #endregion

        private string _mapLayerName;
        private LayerKind _currentMapLayer;

        internal virtual void OnEnable()
        {
            _builderBase = (IFeatureBuilderEditor)target;
            _mapLayer = _builderBase.MapLayerProperty(serializedObject);

            #region Per-layer feature kinds

            _undefinedFeatures = _builderBase.UndefinedFeaturesProperty(serializedObject);
            _boundaryFeatures = _builderBase.BoundaryFeaturesProperty(serializedObject);
            _buildingFeatures = _builderBase.StructureFeaturesProperty(serializedObject);
            _landUseFeatures = _builderBase.LandUseFeaturesProperty(serializedObject);
            _placesFeatures = _builderBase.PlacesFeaturesProperty(serializedObject);
            _roadFeatures = _builderBase.LinearFeaturesProperty(serializedObject);
            _transitFeatures = _builderBase.TransitFeaturesProperty(serializedObject);
            _waterFeatures = _builderBase.WaterFeaturesProperty(serializedObject);

            #endregion

            _currentMapLayer = _builderBase.UpdateMapLayerFeatures();
            SetFeatures(_currentMapLayer);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_mapLayer, true);
            EditorGUILayout.PropertyField(_currentFeatures, new GUIContent($"Features for {_currentMapLayer} Layer"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                _currentMapLayer = _builderBase.UpdateMapLayerFeatures();
                SetFeatures(_currentMapLayer);
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add All Layer Features"))
            {
                _builderBase.AddAllMapLayerFeatures();
            }

            if (GUILayout.Button("Clear All Layer Features"))
            {
                _builderBase.ClearAllMapLayerFeatures();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        private void SetFeatures(LayerKind layer)
        {
            _currentFeatures = layer switch
            {
                LayerKind.Undefined => _undefinedFeatures,
                LayerKind.Boundaries => _boundaryFeatures,
                LayerKind.Buildings => _buildingFeatures,
                LayerKind.Landuse => _landUseFeatures,
                LayerKind.Places => _placesFeatures,
                LayerKind.Roads => _roadFeatures,
                LayerKind.Transit => _transitFeatures,
                LayerKind.Water => _waterFeatures,
                _ => _currentFeatures
            };
        }
    }
}
