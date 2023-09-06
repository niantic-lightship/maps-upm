// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.LightshipMaps
{
    /// <summary>
    /// Creates a new prefab with a <see cref="LightshipMapManager"/>
    /// component as well as a <see cref="LightshipMapView"/>, which
    /// is useful as a single file in scenes with only one map view.
    /// </summary>
    internal class CreateLightshipMap :
        CreatePrefabBase<LightshipMapManager, LightshipMapView>
    {
        /// <inheritdoc />
        protected override void InitializeComponents(
            GameObject rootObject,
            LightshipMapManager mapManager,
            LightshipMapView mapView)
        {
            base.InitializeComponents(rootObject, mapManager, mapView);

            // Hook our LightshipMapManager up to the map view
            ILightshipMapViewWritable mapViewWritable = mapView;
            mapViewWritable.LightshipMapManager = mapManager;
        }
    }
}
