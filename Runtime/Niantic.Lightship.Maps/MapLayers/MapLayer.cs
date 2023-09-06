// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.MapLayers.Components.BaseTypes;
using Niantic.Lightship.Maps.Utilities;
using Niantic.Platform.Debugging;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapLayers
{
    /// <summary>
    /// This class represents a single MapLayer, which is a group of
    /// <see cref="GameObject"/>s attached to a <see cref="LightshipMapView"/>.
    /// These objects are instantiated and added to the map by one or
    /// more <see cref="MapLayerComponent"/>s associated with a given
    /// <see cref="MapLayer"/> instance.
    /// </summary>
    [PublicAPI]
    public class MapLayer : MonoBehaviour
    {
        [Tooltip("The layer's name (for display purposes only).")]
        [SerializeField]
        private string _layerName = "MapLayer";

        private readonly List<MapLayerComponent> _components = new();

        internal string LayerName
        {
            set => _layerName = value;
        }

        private static ChannelLogger Log { get; } = new(nameof(MapLayer));

        /// <summary>
        /// Called by the parent <see cref="LightshipMapView"/> at startup
        /// </summary>
        /// <param name="lightshipMapView">The map associated with this MapLayer</param>
        /// <param name="parent">The <see cref="Transform"/> of the parent <see
        /// cref="GameObject"/> that all <see cref="MapLayer"/>s are children of.</param>
        public void Initialize(LightshipMapView lightshipMapView, Transform parent)
        {
            Log.Verbose($"Initializing layer '{_layerName}'");
            var mapLayer = new GameObject(_layerName);
            mapLayer.transform.SetParent(parent);

            Assert.That(_components.Count == 0);
            _components.AddRange(gameObject.GetComponentsInChildren<MapLayerComponent>());

            foreach (var component in _components)
            {
                component.Initialize(lightshipMapView, mapLayer);
            }
        }

        /// <summary>
        /// Called by the parent <see cref="LightshipMapView"/> when
        /// it has been repositioned to the scene's origin.
        /// </summary>
        public void OnMapOriginChanged()
        {
            _components.ForEach(component => component.OnMapOriginChanged());
        }
    }
}
