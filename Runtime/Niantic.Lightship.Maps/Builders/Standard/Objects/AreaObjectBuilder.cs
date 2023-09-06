// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.Objects
{
    using PooledObjectDictionary = Dictionary<GameObject, PooledObject<GameObject>>;

    /// <summary>
    /// An <see cref="ObjectBuilderStandard"/> used to place object
    /// instances from <see cref="IAreaFeature"/> features.
    /// </summary>
    [PublicAPI]
    public class AreaObjectBuilder : ObjectBuilderStandard
    {
        [Tooltip("A prefab or GameObject that this builder " +
            "will instantiate when placing new objects.")]
        [SerializeField]
        private GameObject _prefab;

        private static ChannelLogger Log { get; } = new(nameof(AreaObjectBuilder));

        private ILightshipMapView _lightshipMapView;
        private ObjectPool<GameObject> _objectPool;
        private readonly PooledObjectDictionary _pooledObjects = new();

        /// <inheritdoc />
        public override void Initialize(ILightshipMapView lightshipMapView)
        {
            base.Initialize(lightshipMapView);
            _lightshipMapView = lightshipMapView;
            _objectPool = new ObjectPool<GameObject>(_prefab, onAcquire: OnObjectAcquired, onRelease: OnObjectReleased);
        }

        private void OnObjectAcquired(PooledObject<GameObject> pooledObject)
        {
            var featureInstance = pooledObject.Value;
            _pooledObjects.Add(featureInstance, pooledObject);

            // Enable and un-hide this object (if it was pooled)
            UnityObjectUtils.EnableAndShow(featureInstance);
        }

        private static void OnObjectReleased(GameObject poolGameObject)
        {
            // Detach this child object from its parent,
            // disable it, and hide it in the hierarchy.
            UnityObjectUtils.DisableAndHide(poolGameObject);
        }

        /// <inheritdoc />
        protected override void BuildFeature(IMapTile mapTile, GameObject parent, IMapTileFeature feature)
        {
            if (feature is IAreaFeature areaFeature)
            {
                // Get or create a prefab instance from the object pool
                var pooledObject = _objectPool.GetOrCreate();
                var featureInstance = pooledObject.Value;

                // Get the object's position, rotation, and scale
                var position = GetObjectPosition(areaFeature);
                var rotation = GetObjectRotation(areaFeature);
                var localScale = GetObjectScale(areaFeature);

                // Adjust the object's local scale relative to our parent maptile
                var scaleFactor = 1.0f / (_lightshipMapView.MapScale * mapTile.Size);
                var tileScale = (float)scaleFactor * _prefab.transform.localScale;
                localScale.Scale(tileScale);

                // Hook this up to the parent and set its local transform
                featureInstance.transform.SetParent(parent.transform, false);
                featureInstance.transform.localPosition = position + ZOffset;
                featureInstance.transform.localRotation = rotation;
                featureInstance.transform.localScale = localScale;
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

                // Return the GameObject to the pool
                pooledObject.Dispose();
            }
        }

        /// <inheritdoc />
        protected override Vector3 GetObjectPosition(IMapTileFeature feature)
        {
            if (feature is not IAreaFeature areaFeature)
            {
                // This method should only be called for area features,
                // so if we somehow got a different feature type, log
                // an error and return a default value of Vector3.zero.

                var type = feature.GetType().Name;
                Log.Error($"Feature '{type}' is not an area feature!");
                return Vector3.zero;
            }

            // Return the area feature's centroid for our object's position
            return MeshBuilderUtils.CalculateCentroid(areaFeature.Points);
        }
    }
}
