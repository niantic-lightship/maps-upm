// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders
{
    /// <inheritdoc cref="IObjectBuilder" />
    [PublicAPI]
    public abstract class ObjectBuilderBase : FeatureBuilderBase, IObjectBuilder
    {
        /// <inheritdoc />
        public GameObject CreateParent(IMapTileObject mapTileObject)
        {
            var parentGameObject = new GameObject($"{BuilderName} Objects");
            parentGameObject.transform.SetParent(mapTileObject.Transform);
            parentGameObject.transform.localPosition = Vector3.zero;
            parentGameObject.transform.localScale = Vector3.one;
            parentGameObject.SetActive(false);

            return parentGameObject;
        }

        /// <inheritdoc />
        public abstract void Release(GameObject parent);

        /// <summary>
        /// Called by the <c>Build</c> method for each <see cref="IMapTileFeature"/> that
        /// matches the builder's <see cref="LayerKind"/> and <see cref="FeatureKind"/>s.
        /// </summary>
        /// <param name="mapTile">The <see cref="IMapTile"/> containing features to build.</param>
        /// <param name="parent">The <see cref="GameObject"/> instantiated by
        /// <see cref="CreateParent"/> that this builder's objects will be parented to.</param>
        /// <param name="feature">The <see cref="IMapTileFeature"/> to build.</param>
        protected abstract void BuildFeature(IMapTile mapTile, GameObject parent, IMapTileFeature feature);
    }
}
