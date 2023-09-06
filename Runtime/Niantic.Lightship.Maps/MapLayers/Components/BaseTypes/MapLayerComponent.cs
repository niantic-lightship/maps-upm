// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapLayers.Components.BaseTypes
{
    /// <summary>
    /// The base class for components that can be added to a <see cref="MapLayer"/>
    /// </summary>
    [PublicAPI]
    public abstract class MapLayerComponent : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="LightshipMapView"/> to which this
        /// component's <see cref="MapLayer"/> belongs.
        /// </summary>
        protected LightshipMapView LightshipMapView;

        /// <summary>
        /// A <see cref="GameObject"/> for this component's <see cref="MapLayer"/>.
        /// All instances of objects created by this component will be parented to
        /// this <see cref="GameObject"/>.
        /// </summary>
        protected GameObject ParentMapLayer;

        /// <summary>
        /// Called from the <see cref="LightshipMapView"/> associated
        /// with this component's <see cref="MapLayer"/> at startup.
        /// </summary>
        /// <param name="lightshipMapView">The map associated with this component</param>
        /// <param name="parent">The <see cref="GameObject"/> created for instances of objects
        /// created by this component (which is assigned to <see cref="ParentMapLayer"/></param>
        public virtual void Initialize(LightshipMapView lightshipMapView, GameObject parent)
        {
            LightshipMapView = lightshipMapView;
            ParentMapLayer = parent;
        }

        /// <summary>
        /// Called from this component's <see cref="MapLayer"/> when its <see
        /// cref="LightshipMapView"/> has been repositioned to the scene's origin.
        /// </summary>
        public abstract void OnMapOriginChanged();
    }
}
