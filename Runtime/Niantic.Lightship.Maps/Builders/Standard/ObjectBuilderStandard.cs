// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Linq;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard
{
    /// <inheritdoc cref="IObjectBuilderStandard" />
    [PublicAPI]
    public abstract class ObjectBuilderStandard : ObjectBuilderBase, IObjectBuilderStandard
    {
        /// <inheritdoc />
        public virtual void Build(IMapTile mapTile, GameObject parent)
        {
            if (Features.IsEmpty())
            {
                // If no features are specified, build all
                // features that match the builder's Layer.
                BuildForLayer(mapTile, parent);
            }
            else
            {
                // Build only the features specified
                BuildForLayerAndFeatures(mapTile, parent);
            }

            // Disable the parent GameObject if there weren't
            // any child objects added to the current maptile.
            parent.SetActive(parent.transform.childCount > 0);
        }

        /// <summary>
        /// Builds all features in the builder's <see cref="LayerKind"/>
        /// </summary>
        /// <param name="mapTile">The maptile being built</param>
        /// <param name="parent">This builder's parent GameObject</param>
        private void BuildForLayer(IMapTile mapTile, GameObject parent)
        {
            var features = mapTile.GetTileData(Layer);

            foreach (var feature in features)
            {
                BuildFeature(mapTile, parent, feature);
            }
        }

        /// <summary>
        /// Builds each of the selected <see cref="FeatureKind"/>s
        /// in the builder's <see cref="LayerKind"/>.
        /// </summary>
        /// <param name="mapTile">The maptile being built</param>
        /// <param name="parent">This builder's parent GameObject</param>
        private void BuildForLayerAndFeatures(IMapTile mapTile, GameObject parent)
        {
            foreach (var featureKind in Features)
            {
                var features = mapTile.GetTileData(Layer, featureKind);

                foreach (var feature in features)
                {
                    BuildFeature(mapTile, parent, feature);
                }
            }
        }

        /// <summary>
        /// Gets an instantiated object's local position.  This method should be
        /// overridden when customizing the position of placed object instances.
        /// </summary>
        /// <param name="feature">The maptile feature being built</param>
        /// <returns>The local position applied to placed objects</returns>
        protected virtual Vector3 GetObjectPosition(IMapTileFeature feature)
        {
            return feature switch
            {
                IAreaFeature areaFeature => MeshBuilderUtils.CalculateCentroid(areaFeature.Points),
                IPointFeature pointFeature => MeshBuilderUtils.CalculateCentroid(pointFeature.Points),
                IStructureFeature structureFeature => MeshBuilderUtils.CalculateCentroid(structureFeature.Points),
                ILinearFeature linearFeature => linearFeature.Points[0],
                _ => throw new ArgumentOutOfRangeException(nameof(feature))
            };
        }

        /// <summary>
        /// Gets an instantiated object's local rotation.  This method should be
        /// overridden when customizing the orientation of placed object instances.
        /// </summary>
        /// <param name="feature">The maptile feature being built</param>
        /// <returns>A local rotation that will be applied to placed objects</returns>
        protected virtual Quaternion GetObjectRotation(IMapTileFeature feature) => Quaternion.identity;

        /// <summary>
        /// Gets an instantiated object's local scale.  This method should be
        /// overridden when customizing the scale of placed object instances.
        /// </summary>
        /// <param name="feature">The maptile feature being built</param>
        /// <returns>A local scale that will be applied to placed objects</returns>
        protected virtual Vector3 GetObjectScale(IMapTileFeature feature) => Vector3.one;
    }
}
