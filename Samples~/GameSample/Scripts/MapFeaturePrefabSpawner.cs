// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders.Standard.Objects;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Samples.Common.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    /// <summary>
    /// Custom maptile feature builder that spawns a prefab for features
    /// found on the map.  Spawned prefabs are attached directly to active
    /// map tiles.  Only supports area map features, so not paths or roads.
    /// </summary>
    [PublicAPI]
    public class MapFeaturePrefabSpawner : AreaObjectBuilder
    {
        [SerializeField]
        private MapGameState.ResourceType _resourceType;

        /// <inheritdoc />
        public override void Build(IMapTile mapTile, GameObject parent)
        {
            if (MapGameState.Instance.IsResourceProductionEnabled(_resourceType))
            {
                // Only spawn objects if this type of resource production is enabled
                base.Build(mapTile, parent);
            }
        }

        /// <inheritdoc />
        protected override Quaternion GetObjectRotation(IMapTileFeature feature)
        {
            // Assign each spawned object a random rotation around the up Vector
            return QuaternionEx.RandomLookRotation();
        }
    }
}
