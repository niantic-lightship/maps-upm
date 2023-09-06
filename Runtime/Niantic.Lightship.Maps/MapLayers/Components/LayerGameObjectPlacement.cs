// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.MapLayers.Components.BaseTypes;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapLayers.Components
{
    /// <summary>
    /// This <see cref="MapLayerComponent"/> instantiates
    /// <see cref="GameObject"/>s from a given prefab.
    /// </summary>
    [PublicAPI]
    public class LayerGameObjectPlacement : LayerPrefabPlacementBase<GameObject>
    {
        /// <inheritdoc />
        protected override Transform GetTransform(GameObject instance) => instance.transform;

        /// <inheritdoc />
        protected override GameObject GetGameObject(GameObject instance) => instance;
    }
}
