// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Attributes;
using Niantic.Lightship.Maps.Builders.Standard;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.Linq;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Utilities;
using Niantic.Platform.Debugging;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Experimental.Labels
{
    /// <summary>
    /// This builder class is responsible for instantiating
    /// <see cref="LabelObject"/> types containing text
    /// strings and positioning and orienting them on the map.
    /// </summary>
    /// <typeparam name="T">The concrete <see cref="LabelObject"/>
    /// type that this builder will instantiate.</typeparam>
    [PublicAPI]
    public abstract class LabelBuilder<T> : ObjectBuilderStandard
        where T : LabelObject
    {
        [Flags]
        private enum FeatureType
        {
            // @formatter:off
            PointFeatures =     1 << 0,
            AreaFeatures =      1 << 1,
            LinearFeatures =    1 << 2,
            StructureFeatures = 1 << 3,

            /// <summary>
            /// This value represents all feature types.
            /// The InspectorName(null) attribute hides it
            /// from the dropdown list in the Inspector.
            /// </summary>
            [InspectorName(null)]
            AllFeatureTypes =   (1 << 4) - 1
            // @formatter:on
        }

        [Tooltip("A prefab containing a LabelObject to " +
            "instantiate for each label placed on the map")]
        [SerializeField]
        private T _labelPrefab;

        [Tooltip(
            "The maximum number of unique label strings allowed by " +
            "this builder per maptile.  This value is useful for limiting " +
            "visual clutter that can occur when a single real-world feature " +
            "is split into multiple maptile features in a given tile or for " +
            "any other case where duplicate labels may appear on a maptile.")]
        [SerializeField]
        private int _maxLabelStringsPerTile = 1;

        [Tooltip("If set, the label is scaled relative to the map's " +
            "viewable radius, which can be used to maintain a constant " +
            "size in screen space when a map view is zoomed in or out.")]
        [SerializeField]
        private bool _scaleWithMapRadius;

        [Tooltip("If 'Scale With Map Radius' is set, this value defines " +
            "the ratio of the label's size to the map's radius, which is " +
            "used to calculate the LabelObject's local scale.")]
        [SerializeField]
        [DisabledIfFalse(nameof(_scaleWithMapRadius))]
        private double _scaleFactor = 1.0f;

        [Tooltip("The label's minimum allowed scale, in scene units.")]
        [SerializeField]
        [DisabledIfFalse(nameof(_scaleWithMapRadius))]
        private double _minimumScale;

        [Tooltip("The label's maximum allowed scale, in scene units.")]
        [SerializeField]
        [DisabledIfFalse(nameof(_scaleWithMapRadius))]
        private double _maximumScale = double.PositiveInfinity;

        [Tooltip("If set, labels for linear features will be aligned with the " +
            "line's tangent at the point at which the label is positioned.")]
        [SerializeField]
        private bool _alignWithLinearFeatures = true;

        [Tooltip("Flags indicating which maptile feature types will " +
            "be included as a potential source of label strings.")]
        [SerializeField]
        private FeatureType _includedFeatures = FeatureType.AllFeatureTypes;

        private readonly Dictionary<GameObject, PooledObject<T>> _pooledObjects = new();
        private readonly Dictionary<GameObject, Dictionary<string, int>> _labelsPerTile = new();

        private ObjectPool<T> _objectPool;
        private ILightshipMapView _lightshipMapView;
        private Vector3 _originalScale;

        private static ChannelLogger Log { get; } = new(nameof(LabelBuilder<T>));

        /// <inheritdoc />
        public override void Initialize(ILightshipMapView lightshipMapView)
        {
            base.Initialize(lightshipMapView);
            _lightshipMapView = lightshipMapView;
            _originalScale = _labelPrefab.transform.localScale;
            _objectPool = new ObjectPool<T>(_labelPrefab, onAcquire: OnLabelAcquired, onRelease: OnLabelReleased);

            if (_scaleWithMapRadius)
            {
                _lightshipMapView.MapRadiusChanged += OnMapViewRadiusChanged;
            }
        }

        /// <inheritdoc />
        public override void Release(GameObject parent)
        {
            while (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(0);
                var childGameObject = child.gameObject;

                if (!_pooledObjects.Remove(childGameObject, out var pooledObject))
                {
                    // If we can't find this child in our list of pooled objects,
                    // just destroy it rather than releasing it back to the pool
                    Log.Warning("Couldn't find a pooled object to release!");
                    childGameObject.transform.SetParent(null, false);
                    Destroy(childGameObject);
                    continue;
                }

                // Return the object to the pool
                pooledObject.Dispose();
            }

            // Remove the tile's unique label collection
            _labelsPerTile.Remove(parent);

            // Disable the parent GameObject
            parent.SetActive(false);
        }

        /// <inheritdoc />
        protected override void BuildFeature(IMapTile mapTile, GameObject parent, IMapTileFeature feature)
        {
            var label = feature.Label;

            if (string.IsNullOrEmpty(feature.Label?.Text))
            {
                // Early-out if this feature doesn't have a label
                return;
            }

            if (IsFeatureDisabled(feature))
            {
                // Early-out if this feature type isn't enabled
                return;
            }

            // Keep track of the number of unique label strings per
            // tile in order to limit the number of duplicate labels.
            var tileLabels = _labelsPerTile.GetOrCreateValue(parent);
            var labelCount = tileLabels.GetOrCreateValue(label.Text);

            if (labelCount >= _maxLabelStringsPerTile)
            {
                // Early-out if we've reached the limit for this label
                return;
            }

            tileLabels[label.Text]++;

            // Get or create a prefab instance from the object pool
            var pooledObject = _objectPool.GetOrCreate();
            var labelObject = pooledObject.Value;

            // Set the label's text string and parent tile
            labelObject.Initialize(label.Text, mapTile);
            labelObject.name = label.Text;

            // Hook this up to the parent and set its position, rotation, and scale
            labelObject.transform.SetParent(parent.transform, false);
            labelObject.transform.localScale = GetLocalScale(labelObject);
            var (position, rotation) = GetLocalPositionAndRotation(feature, label);
            var labelObjectTransform = labelObject.transform;
            labelObjectTransform.localPosition = position + ZOffset;
            labelObjectTransform.localRotation = rotation;
        }

        /// <summary>
        /// Called when the map radius changes so that we can update any
        /// labels whose scale is computed relative to the map radius.
        /// </summary>
        /// <param name="mapRadius">The new map radius.</param>
        private void OnMapViewRadiusChanged(double mapRadius)
        {
            foreach (var (_, pooledObject) in _pooledObjects)
            {
                var labelObject = pooledObject.Value;
                labelObject.transform.localScale = GetLocalScale(labelObject);
            }
        }

        /// <summary>
        /// Called when a label object is acquired from the object pool
        /// </summary>
        private void OnLabelAcquired(PooledObject<T> pooledObject)
        {
            var labelObject = pooledObject.Value;
            var labelGameObject = labelObject.gameObject;

            // Enable and un-hide this object (if it was pooled)
            UnityObjectUtils.EnableAndShow(labelGameObject);
            _pooledObjects.Add(labelGameObject, pooledObject);
        }

        /// <summary>
        /// Called when a label object is released back to the object pool
        /// </summary>
        private static void OnLabelReleased(T labelObject)
        {
            // Detach this label's GameObject from its parent,
            // disable it, and hide it in the hierarchy.
            UnityObjectUtils.DisableAndHide(labelObject.gameObject);
        }

        /// <summary>
        /// Determines whether the builder is configured to
        /// create labels for a given maptile feature type.
        /// </summary>
        private bool IsFeatureDisabled(IMapTileFeature feature) =>
            (_includedFeatures & GetFeatureType(feature)) == 0;

        /// <summary>
        /// Gets a FeatureType enum corresponding to a maptile feature type.
        /// </summary>
        private static FeatureType GetFeatureType(IMapTileFeature feature)
        {
            return feature switch
            {
                ILinearFeature => FeatureType.LinearFeatures,
                IStructureFeature => FeatureType.StructureFeatures,
                IAreaFeature => FeatureType.AreaFeatures,
                IPointFeature => FeatureType.PointFeatures,
                _ => throw new ArgumentOutOfRangeException(nameof(feature), feature, null)
            };
        }

        /// <summary>
        /// Gets a local scale for label instances.  If "scale
        /// with map radius" is set, this scale will be based on
        /// the <see cref="LightshipMapView"/>'s viewable map radius.
        /// </summary>
        private Vector3 GetLocalScale(LabelObject labelObject)
        {
            Vector3 localScale;
            var tileScale = 1.0f / (_lightshipMapView.MapScale * labelObject.ParentTile.Size);

            if (!_scaleWithMapRadius)
            {
                localScale = (float)tileScale * _originalScale;
                localScale.y = 1.0f;
                return localScale;
            }

            var radiusMeters = _lightshipMapView.MapRadius;
            var latitude = _lightshipMapView.MapCenter.Latitude;
            var radiusScene = _lightshipMapView.MetersToScene(radiusMeters, latitude);

            var scale = radiusScene * _scaleFactor;
            var clampedScale = MathEx.Clamp(scale, _minimumScale, _maximumScale);
            labelObject.SetScaleForInspector(scale, clampedScale);

            localScale = (float)(clampedScale * tileScale) * _originalScale;
            localScale.y = 1.0f;
            return localScale;
        }

        /// <summary>
        /// Computes the local position and rotation for a given feature's label.
        /// </summary>
        private (Vector3 Position, Quaternion Rotation)
            GetLocalPositionAndRotation(IMapTileFeature feature, ILabelInfo label)
        {
            return feature switch
            {
                ILinearFeature linearFeature => GetLocalPositionAndRotation(linearFeature),
                IStructureFeature or IAreaFeature or IPointFeature =>
                    (base.GetObjectPosition(feature), base.GetObjectRotation(feature)),
                _ => (new Vector3(label.PosX, 0, label.PosY), Quaternion.identity)
            };
        }

        /// <summary>
        /// Computes the local position and rotation for a linear feature
        /// (such as a road or a water feature such as a river or stream).
        /// </summary>
        private (Vector3 Position, Quaternion Rotation)
            GetLocalPositionAndRotation(ILinearFeature feature)
        {
            Vector3 position;
            var rotation = Quaternion.identity;

            int currentStripStart = 0;
            int longestStripStart = 0;
            int longestStripLength = 0;

            // Find the longest line strip in the current feature
            foreach (var lineStripLength in feature.LineStrips)
            {
                if (lineStripLength > longestStripLength)
                {
                    longestStripLength = lineStripLength;
                    longestStripStart = currentStripStart;
                }

                currentStripStart += lineStripLength;
            }

            int midpointIndex0;
            int midpointIndex1;

            if (longestStripLength % 2 == 0)
            {
                // If the longest line strip's length is even, then its midpoint
                // is between two points in the feature's Points array.  Indices
                // of these two points are saved and used to calculate the line
                // strip's tangent if the label is being aligned with its direction.

                midpointIndex0 = longestStripStart + longestStripLength / 2 - 1;
                midpointIndex1 = midpointIndex0 + 1;

                var point0 = feature.Points[midpointIndex0];
                var point1 = feature.Points[midpointIndex1];

                position = (point0 + point1) / 2.0f;
            }
            else
            {
                // If the longest line strip's length is odd, then its midpoint
                // is a single point in the feature's Points array.  Indices of
                // the points before and after this middle point are saved and
                // used to/ calculate the line strip's tangent if the label is
                // being aligned with its direction.

                var midIndex = longestStripStart + longestStripLength / 2;
                Assert.That(longestStripLength >= 3);

                midpointIndex0 = midIndex - 1;
                midpointIndex1 = midIndex + 1;

                position = feature.Points[midIndex];
            }

            // If selected, align labels with the tangent
            // at the longest line strip's middle point.
            if (_alignWithLinearFeatures)
            {
                var point0 = feature.Points[midpointIndex0];
                var point1 = feature.Points[midpointIndex1];

                var tangent = (point0 - point1).normalized;
                var dot = Vector3.Dot(Vector3.right, tangent);

                rotation = dot >= 0
                    ? Quaternion.FromToRotation(Vector3.right, tangent)
                    : Quaternion.FromToRotation(Vector3.right, -tangent);
            }

            return (position, rotation);
        }
    }
}
