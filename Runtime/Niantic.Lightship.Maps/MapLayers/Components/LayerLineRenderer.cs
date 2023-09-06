// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.MapLayers.Components.BaseTypes;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapLayers.Components
{
    using InstanceDictionary = Dictionary<GameObject, LatLng>;
    using LineRendererDictionary = Dictionary<GameObject, LineRenderer>;

    /// <summary>
    /// This <see cref="MapLayerComponent"/> is used to draw lines on the map
    /// </summary>
    [PublicAPI]
    public class LayerLineRenderer : MapLayerComponent
    {
        [Tooltip("The layer component's name (for display purposes only).")]
        [SerializeField]
        private string _name = "Line";

        [Tooltip("The line's width when rendered.")]
        [SerializeField]
        private float _lineWidth;

        [Tooltip("An offset along the up vector to apply to lines rendered with this component.")]
        [SerializeField]
        private float _heightOffset;

        [Tooltip("Material that will be applied to lines rendered with this component.")]
        [SerializeField]
        private Material _material;

        private ObjectPool<GameObject> _objectPool;
        private readonly InstanceDictionary _instances = new();
        private readonly LineRendererDictionary _lineRenderers = new();

        /// <summary>
        /// Draws a loop defined by a list of <see cref="LatLng"/> coordinates.
        /// </summary>
        /// <param name="outline">The points along the loop, in order</param>
        /// <param name="instanceName">An optional name for the GameObject</param>
        /// <returns>A <see cref="GameObject"/> representing this line instance</returns>
        public PooledObject<GameObject> DrawLoop(IReadOnlyList<LatLng> outline, string instanceName = null)
        {
            // The line renderer is configured such that it assumes
            // a non-closed loop, so if the first and last vertices
            // are the same, then skip the first one in the list.
            int outlineIndex = outline[0] == outline[^1] ? 1 : 0;

            int pointCount = outline.Count - outlineIndex;
            var vertices = new Vector3[pointCount];

            // Get or create an instance from the object pool
            var pooledObject = _objectPool.GetOrCreate();
            var instance = pooledObject.Value;
            var lineRenderer = _lineRenderers[instance];

            instance.name = instanceName ?? _name;
            PositionInstance(instance, LightshipMapView.MapOrigin);

            lineRenderer.positionCount = pointCount;
            lineRenderer.widthMultiplier = (float)LightshipMapView.MapRadius;

            for (int i = 0; i < vertices.Length; i++)
            {
                // Convert LatLng to world space position
                var latLng = outline[outlineIndex++];
                var point = LightshipMapView.LatLngToScene(latLng);
                var local = ParentMapLayer.transform.InverseTransformPoint(point);
                lineRenderer.SetPosition(i, local);
            }

            return pooledObject;
        }

        private void PositionInstance(GameObject instance, in LatLng latLng)
        {
            // Hook this up to the parent and set its position and scale
            var position = LightshipMapView.LatLngToScene(latLng) + _heightOffset * Vector3.up;
            var localPos = ParentMapLayer.transform.InverseTransformPoint(position);
            instance.transform.SetParent(ParentMapLayer.transform, false);
            instance.transform.localScale = Vector3.one;
            instance.transform.localPosition = localPos;
        }

        /// <inheritdoc />
        public override void Initialize(LightshipMapView lightshipMapView, GameObject parent)
        {
            base.Initialize(lightshipMapView, parent);
            lightshipMapView.MapRadiusChanged += OnMapRadiusChanged;

            var lineRenderer = CreateLineRenderer();
            _objectPool = new ObjectPool<GameObject>(
                lineRenderer,
                onAcquire: OnObjectPoolAcquire,
                onRelease: OnObjectPoolRelease);
        }

        /// <inheritdoc />
        public override void OnMapOriginChanged()
        {
            foreach (var instance in _instances)
            {
                // Recalculate the object's position from its LatLng if the
                // parent LightshipMapView was repositioned to the origin.
                PositionInstance(instance.Key, instance.Value);
            }
        }

        private void OnObjectPoolAcquire(PooledObject<GameObject> pooledObject)
        {
            var instance = pooledObject.Value;
            var lineRenderer = instance.GetComponent<LineRenderer>();

            // Keep track of the map's origin when this instance
            // was placed so that it can be repositioned with an
            // offset if the map's origin ever changes.
            _lineRenderers.Add(instance, lineRenderer);
            _instances.Add(instance, LightshipMapView.MapOrigin);

            // Enable and un-hide this object (if it was pooled)
            UnityObjectUtils.EnableAndShow(instance);
        }

        private void OnObjectPoolRelease(GameObject instance)
        {
            _instances.Remove(instance);
            _lineRenderers.Remove(instance);

            // Detach this child object from its parent,
            // disable it, and hide it in the hierarchy.
            UnityObjectUtils.DisableAndHide(instance);
        }

        private void OnDestroy()
        {
            LightshipMapView.MapRadiusChanged -= OnMapRadiusChanged;
        }

        private void OnMapRadiusChanged(double mapRadius)
        {
            foreach (var instance in _lineRenderers)
            {
                instance.Value.widthMultiplier = (float)mapRadius;
            }
        }

        private GameObject CreateLineRenderer()
        {
            var go = new GameObject(_name);
            UnityObjectUtils.DisableAndHide(go);

            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.material = _material;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.widthCurve = AnimationCurve.Constant(1, 1, _lineWidth);
            lineRenderer.widthMultiplier = (float)LightshipMapView.MapRadius;

            return go;
        }
    }
}
