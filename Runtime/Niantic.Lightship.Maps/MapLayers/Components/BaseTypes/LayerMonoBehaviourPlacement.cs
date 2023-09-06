// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Niantic.Lightship.Maps.MapLayers.Components.BaseTypes
{
    /// <summary>
    /// Base class for <see cref="MapLayerComponent"/>s that instantiate
    /// <see cref="MonoBehaviour"/>s from a given prefab.
    /// </summary>
    /// <typeparam name="T">The <see cref="MonoBehaviour"/>'s type</typeparam>
    [PublicAPI]
    public class LayerMonoBehaviourPlacement<T> : LayerPrefabPlacementBase<T> where T : MonoBehaviour
    {
        /// <inheritdoc />
        protected override Transform GetTransform(T instance) => instance.transform;

        /// <inheritdoc />
        protected override GameObject GetGameObject(T instance) => instance.gameObject;
    }
}
