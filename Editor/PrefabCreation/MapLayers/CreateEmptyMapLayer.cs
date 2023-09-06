// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.MapLayers;
using Niantic.Lightship.Maps.MapLayers.Components.BaseTypes;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.MapLayers
{
    /// <summary>
    /// Creates an empty <see cref="MapLayer"/> prefab,
    /// with no <see cref="MapLayerComponent"/>s attached.
    /// </summary>
    internal class CreateEmptyMapLayer : CreatePrefabBase<MapLayer>
    {
        /// <inheritdoc />
        protected override void InitializeComponents(
            GameObject rootObject, MapLayer mapLayer)
        {
            base.InitializeComponents(rootObject, mapLayer);
            mapLayer.LayerName = rootObject.name;
            mapLayer.name = rootObject.name;
        }
    }
}
