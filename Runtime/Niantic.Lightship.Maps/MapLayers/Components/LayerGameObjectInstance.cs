// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Attributes;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.MapLayers.Components.BaseTypes;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapLayers.Components
{
    /// <summary>
    /// This <see cref="MapLayerComponent"/> contains and
    /// manages a single <see cref="GameObject"/> instance
    /// </summary>
    [PublicAPI]
    public class LayerGameObjectInstance : MapLayerComponent
    {
        [Tooltip("A GameObject that this component is responsible for positioning.")]
        [SerializeField]
        private Transform _gameObject;

        [Tooltip("If set, objects placed from this component will " +
            "be scaled relative to the map view's radius.  This is " +
            "useful for objects that need to maintain a consistent " +
            "size in screen space when the map is zoomed in and out.")]
        [SerializeField]
        private bool _scaleWithMapRadius;

        [Tooltip("When 'Scale With Map Radius' is set, this value defines " +
            "the ratio of the object's size to the map's radius, which " +
            "is used to calculate the object's local scale.")]
        [SerializeField]
        [DisabledIfFalse(nameof(_scaleWithMapRadius))]
        private double _relativeScale = 1.0d;

        [Tooltip("The object's minimum allowed scale.")]
        [SerializeField]
        [DisabledIfFalse(nameof(_scaleWithMapRadius))]
        private double _minimumScale;

        [Tooltip("The object's maximum allowed scale.")]
        [SerializeField]
        private double _maximumScale = double.PositiveInfinity;

        private Vector3 _originalLocalScale;

        /// <inheritdoc />
        public override void OnMapOriginChanged()
        {
        }

        /// <inheritdoc />
        public override void Initialize(LightshipMapView lightshipMapView, GameObject parent)
        {
            base.Initialize(lightshipMapView, parent);

            // Hook our GameObject up to its parent
            _originalLocalScale = _gameObject.transform.localScale;
            _gameObject.SetParent(ParentMapLayer.transform, false);

            if (_scaleWithMapRadius)
            {
                lightshipMapView.MapRadiusChanged += OnMapRadiusChanged;
            }
        }

        private void OnDestroy()
        {
            if (_scaleWithMapRadius)
            {
                LightshipMapView.MapRadiusChanged -= OnMapRadiusChanged;
            }
        }

        private void OnMapRadiusChanged(double mapRadius)
        {
            _gameObject.transform.localScale = GetLocalScale(mapRadius);
        }

        /// <summary>
        /// Gets a local scale for object instances.  If "scale
        /// with map radius" is set, this scale will be based on
        /// the <see cref="LightshipMapView"/>'s viewable map radius.
        /// </summary>
        /// <param name="mapRadius">The map's radius</param>
        /// <returns></returns>
        private Vector3 GetLocalScale(double mapRadius)
        {
            if (!_scaleWithMapRadius)
            {
                return _originalLocalScale;
            }

            var scale = mapRadius * _relativeScale;
            var clampedScale = MathEx.Clamp(scale, _minimumScale, _maximumScale);
            return (float)clampedScale * _originalLocalScale;
        }
    }
}
