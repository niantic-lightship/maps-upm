// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Niantic.Lightship.Maps.Builders.Editor;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Linq;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps.Builders
{
    /// <summary>
    /// Editor-specific members of <see cref="FeatureBuilderBase"/>
    /// </summary>
    public abstract partial class FeatureBuilderBase : IFeatureBuilderEditor
    {
        #region Per-layer feature kinds

        [SerializeField]
        [HideInInspector]
        private List<UndefinedFeatureKind> _undefinedFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<BoundariesFeatureKind> _boundaryFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<BuildingsFeatureKind> _structureFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<LanduseFeatureKind> _landUseFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<PlacesFeatureKind> _placesFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<RoadsFeatureKind> _linearFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<TransitFeatureKind> _transitFeatures = new();

        [SerializeField]
        [HideInInspector]
        private List<WaterFeatureKind> _waterFeatures = new();

        #endregion
        #region IFeatureBuilderWritable

        /// <inheritdoc />
        float IFeatureBuilderWritable.ZOffset { set { _zOffset = value; } }

        /// <inheritdoc />
        string IFeatureBuilderWritable.BuilderName { set { _builderName = value; } }

        /// <inheritdoc />
        void IFeatureBuilderWritable.AddAllMapLayerFeatures()
        {
            var featureList = GetAndClearPerLayerFeatureList(_mapLayer);
            var enumValues = Enum.GetValues(GetPerLayerEnumType(_mapLayer))
                .Cast<Enum>()
                .OrderBy(e => e.ToString())
                .ToList();

            featureList.AddRange(enumValues);
            _features = featureList.Cast<FeatureKind>().ToList();
        }

        /// <inheritdoc />
        void IFeatureBuilderWritable.ClearAllMapLayerFeatures()
        {
            var featureList = GetAndClearPerLayerFeatureList(_mapLayer);
            _features = featureList.Cast<FeatureKind>().ToList();
        }

        /// <inheritdoc />
        LayerKind IFeatureBuilderWritable.UpdateMapLayerFeatures()
        {
            _features = GetPerLayerFeatureList(_mapLayer).Cast<FeatureKind>().ToList();
            return _mapLayer;
        }

        #endregion
        #region IFeatureBuilderEditor

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.MapLayerProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_mapLayer));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.UndefinedFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_undefinedFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.BoundaryFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_boundaryFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.StructureFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_structureFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.LandUseFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_landUseFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.PlacesFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_placesFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.LinearFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_linearFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.TransitFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_transitFeatures));

        /// <inheritdoc />
        SerializedProperty IFeatureBuilderEditor.WaterFeaturesProperty(
            SerializedObject serializedObject)
            => serializedObject.FindProperty(nameof(_waterFeatures));

        #endregion

        private IList GetAndClearPerLayerFeatureList(LayerKind layer)
        {
            var featureList = GetPerLayerFeatureList(layer);
            featureList.Clear();
            return featureList;
        }

        private IList GetPerLayerFeatureList(LayerKind layer)
        {
            return layer switch
            {
                LayerKind.Undefined => _undefinedFeatures,
                LayerKind.Boundaries => _boundaryFeatures,
                LayerKind.Buildings => _structureFeatures,
                LayerKind.Landuse => _landUseFeatures,
                LayerKind.Places => _placesFeatures,
                LayerKind.Roads => _linearFeatures,
                LayerKind.Transit => _transitFeatures,
                LayerKind.Water => _waterFeatures,
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
            };
        }

        private static Type GetPerLayerEnumType(LayerKind layer)
        {
            return layer switch
            {
                LayerKind.Undefined => typeof(UndefinedFeatureKind),
                LayerKind.Boundaries => typeof(BoundariesFeatureKind),
                LayerKind.Buildings => typeof(BuildingsFeatureKind),
                LayerKind.Landuse => typeof(LanduseFeatureKind),
                LayerKind.Places => typeof(PlacesFeatureKind),
                LayerKind.Roads => typeof(RoadsFeatureKind),
                LayerKind.Transit => typeof(TransitFeatureKind),
                LayerKind.Water => typeof(WaterFeatureKind),
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
            };
        }
    }
}

#endif
