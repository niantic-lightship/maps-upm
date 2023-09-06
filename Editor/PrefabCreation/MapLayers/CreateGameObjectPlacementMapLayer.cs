// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.MapLayers;
using Niantic.Lightship.Maps.MapLayers.Components;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.MapLayers
{
    /// <summary>
    /// Creates a <see cref="MapLayer"/> prefab with a
    /// <see cref="LayerGameObjectPlacement"/> component.
    /// </summary>
    internal class CreateGameObjectPlacementMapLayer :
        CreateMapLayerBase<LayerGameObjectPlacement>
    {
    }
}
