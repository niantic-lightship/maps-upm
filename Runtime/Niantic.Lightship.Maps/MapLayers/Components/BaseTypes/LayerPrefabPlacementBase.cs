// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Attributes;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Niantic.Lightship.Maps.MapLayers.Components.BaseTypes
{
    /// <summary>
    /// Base class for all <see cref="MapLayerComponent"/>s
    /// that instantiate objects from a given prefab.
    /// </summary>
    /// <typeparam name="T">The prefab's type</typeparam>
    [PublicAPI]
    public abstract class LayerPrefabPlacementBase<T> : MapLayerComponent where T : Object
    {
        [Tooltip("The layer component's name (for display purposes only).")]
        [SerializeField]
        private string _name = "Prefab";

        [Tooltip("A prefab or GameObject that this component " +
            "will instantiate when placing new objects.")]
        [SerializeField]
        private T _prefab;

        [Tooltip("An offset along the up vector to apply " +
            "to objects placed with this component.")]
        [SerializeField]
        private float _heightOffset;

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
        [DisabledIfFalse(nameof(_scaleWithMapRadius))]
        private double _maximumScale = double.PositiveInfinity;

        private Vector3 _offset;
        private Vector3 _prefabScale;
        private Quaternion _prefabRotation;

        private readonly Dictionary<T, (LatLng Position, Quaternion Rotation)> _instances = new();
        private ObjectPool<T> _objectPool;

        /// <summary>
        /// Classes derived from <see cref="LayerPrefabPlacementBase{T}"/> should
        /// implement this method by returning the <see cref="Transform"/> of an
        /// object placed by that derived class.
        /// </summary>
        /// <param name="instance">An object whose Transform is returned</param>
        /// <returns>The Transform belonging to placed object instance</returns>
        protected abstract Transform GetTransform(T instance);

        /// <summary>
        /// Classes derived from <see cref="LayerPrefabPlacementBase{T}"/> should
        /// implement this method by returning the <see cref="GameObject"/> of an
        /// object placed by that derived class.
        /// </summary>
        /// <param name="instance">An object whose GameObject is returned</param>
        /// <returns>The GameObject belonging to placed object instance</returns>
        protected abstract GameObject GetGameObject(T instance);

        /// <summary>
        /// Places an instance of this component's prefab
        /// at a given <see cref="LatLng"/> coordinate.
        /// </summary>
        /// <param name="position">The location to place the instance</param>
        /// <param name="instanceName">An optional name to assign to the instance</param>
        /// <returns>An instance placed at the desired <see cref="LatLng"/></returns>
        public PooledObject<T> PlaceInstance(in LatLng position, string instanceName = null)
            => PlaceInstance(position, Quaternion.identity, instanceName);

        /// <summary>
        /// Places an instance of this component's prefab
        /// at a given <see cref="LatLng"/> coordinate.
        /// </summary>
        /// <param name="position">The location to place the instance</param>
        /// <param name="rotation">A local rotation applied to the placed instance</param>
        /// <param name="instanceName">An optional name to assign to the instance</param>
        /// <returns>An instance placed at the desired <see cref="LatLng"/></returns>
        public virtual PooledObject<T> PlaceInstance(
            in LatLng position,
            in Quaternion rotation,
            string instanceName = null)
        {
            // Get or create a prefab instance from the object pool
            var pooledObject = _objectPool.GetOrCreate();
            var instance = pooledObject.Value;
            _instances.Add(instance, (position, rotation));

            instance.name = instanceName ?? _name;
            PositionInstance(instance, position, rotation);

            return pooledObject;
        }

        /// <summary>
        /// Places an instance of this component's prefab at
        /// a given point in the Unity scene's world space.
        /// </summary>
        /// <param name="position">The location to place the instance</param>
        /// <param name="instanceName">An optional name to assign to the instance</param>
        /// <returns>An instance placed at the desired position</returns>
        public PooledObject<T> PlaceInstance(in Vector3 position, string instanceName = null)
            => PlaceInstance(position, Quaternion.identity, instanceName);

        /// <summary>
        /// Places an instance of this component's prefab at
        /// a given point in the Unity scene's world space.
        /// </summary>
        /// <param name="position">The location to place the instance</param>
        /// <param name="rotation">A local rotation applied to the placed instance</param>
        /// <param name="instanceName">An optional name to assign to the instance</param>
        /// <returns>An instance placed at the desired position</returns>
        public PooledObject<T> PlaceInstance(in Vector3 position, in Quaternion rotation, string instanceName = null)
            => PlaceInstance(LightshipMapView.SceneToLatLng(position), rotation, instanceName);

        /// <summary>
        /// Positions and orients a placed object instance
        /// </summary>
        /// <param name="instance">The object being positioned</param>
        /// <param name="location">The object's location</param>
        /// <param name="rotation">The object's local rotation</param>
        private void PositionInstance(T instance, in LatLng location, in Quaternion rotation)
        {
            // Hook this up to the parent and set its transform
            var instanceTransform = GetTransform(instance);
            instanceTransform.SetParent(ParentMapLayer.transform, false);
            instanceTransform.localScale = GetObjectScale(LightshipMapView.MapRadius);
            instanceTransform.localRotation = GetObjectRotation(rotation);
            instanceTransform.position = GetObjectPosition(location);
        }

        /// <inheritdoc />
        public override void Initialize(LightshipMapView lightshipMapView, GameObject parent)
        {
            base.Initialize(lightshipMapView, parent);
            _objectPool = new ObjectPool<T>(_prefab, onAcquire: OnObjectPoolAcquire, onRelease: OnObjectPoolRelease);

            var prefabTransform = GetTransform(_prefab);
            _prefabRotation = prefabTransform.localRotation;
            _prefabScale = prefabTransform.localScale;
            _offset = _heightOffset * Vector3.up;

            if (_scaleWithMapRadius)
            {
                lightshipMapView.MapRadiusChanged += OnLightshipMapRadiusChanged;
            }
        }

        private void OnDestroy()
        {
            LightshipMapView.MapRadiusChanged -= OnLightshipMapRadiusChanged;
        }

        /// <summary>
        /// Called when an object is acquired from the <see cref="ObjectPool{T}"/>.
        /// This object may be newly instantiated or an instance reused from the pool.
        /// </summary>
        /// <param name="pooledObject">The pooled object handle</param>
        protected virtual void OnObjectPoolAcquire(PooledObject<T> pooledObject)
        {
            // Enable and un-hide this object (if it was pooled)
            var instanceGameObject = GetGameObject(pooledObject.Value);
            UnityObjectUtils.EnableAndShow(instanceGameObject);
        }

        /// <summary>
        /// Called when an object is released into the <see cref="ObjectPool{T}"/>
        /// </summary>
        /// <param name="instance">The instance being released</param>
        protected virtual void OnObjectPoolRelease(T instance)
        {
            _instances.Remove(instance);

            // Detach this child object from its parent,
            // disable it, and hide it in the hierarchy.
            var instanceGameObject = GetGameObject(instance);
            UnityObjectUtils.DisableAndHide(instanceGameObject);
        }

        /// <summary>
        /// This method is called when the <see cref="LightshipMapView"/>'s
        /// radius changes.  This method should be overridden if any
        /// action needs to be taken (such as scaling object instances)
        /// </summary>
        /// <param name="mapRadius">The new map radius</param>
        private void OnLightshipMapRadiusChanged(double mapRadius)
        {
            if (_scaleWithMapRadius)
            {
                foreach (var (instance, _) in _instances)
                {
                    var instanceTransform = GetTransform(instance);
                    instanceTransform.localScale = GetObjectScale(mapRadius);
                }
            }
        }

        /// <inheritdoc />
        public override void OnMapOriginChanged()
        {
            foreach (var (instance, value) in _instances)
            {
                // Recalculate the object's position from its LatLng if
                // the parent LightshipMapView was repositioned to the origin.
                PositionInstance(instance, value.Position, value.Rotation);
            }
        }

        /// <summary>
        /// Gets a local scale for object instances.  If "scale
        /// with map radius" is set, this scale will be based on
        /// the <see cref="LightshipMapView"/>'s viewable map radius.
        /// </summary>
        /// <param name="mapRadius">The map's radius</param>
        /// <returns>A local scale that will be applied to placed objects</returns>
        protected virtual Vector3 GetObjectScale(double mapRadius)
        {
            if (!_scaleWithMapRadius)
            {
                return _prefabScale;
            }

            var scale = mapRadius * _relativeScale;
            var clampedScale = MathEx.Clamp(scale, _minimumScale, _maximumScale);
            return (float)clampedScale * _prefabScale;
        }

        /// <summary>
        /// Gets an instantiated object's local rotation.  This method should be
        /// overridden when customizing the orientation of placed object instances.
        /// </summary>
        /// <param name="rotation">The local rotation associated with this object</param>
        /// <returns>A new local rotation that will be applied to this object</returns>
        protected virtual Quaternion GetObjectRotation(in Quaternion rotation)
            => _prefabRotation * rotation;

        /// <summary>
        /// Gets an instantiated object's position in the Unity
        /// scene's world space.  This method should be overridden
        /// when customizing the position of placed object instances.
        /// </summary>
        /// <param name="location">The location associated with this object</param>
        /// <returns>A world space position applied to this object</returns>
        protected virtual Vector3 GetObjectPosition(in LatLng location)
            => LightshipMapView.LatLngToScene(location) + _offset;
    }
}
