// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.MapLayers;
using Niantic.Lightship.Maps.MapLayers.Components.BaseTypes;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.MapLayers
{
    /// <summary>
    /// Base class used to create new <see cref="MapLayer"/> prefabs
    /// </summary>
    /// <typeparam name="T">A type derived from
    /// <see cref="MapLayerComponent"/> that specifies
    /// a MapLayer component to add to the new prefab.</typeparam>
    internal class CreateMapLayerBase<T> : CreatePrefabBase<MapLayer, T>
        where T : MapLayerComponent
    {
        /// <inheritdoc />
        protected override void InitializeComponents(
            GameObject rootObject, MapLayer mapLayer, T component)
        {
            base.InitializeComponents(rootObject, mapLayer, component);
            mapLayer.LayerName = rootObject.name;
            mapLayer.name = rootObject.name;
        }
    }
}
