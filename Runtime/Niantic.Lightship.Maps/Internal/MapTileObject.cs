// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Builders;
using Niantic.Lightship.Maps.Builders.Performance;
using Niantic.Lightship.Maps.Builders.Standard;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.ExtensionMethods;
using Niantic.Lightship.Maps.Themes;
using UnityEngine;

namespace Niantic.Lightship.Maps.Internal
{
    /// <inheritdoc cref="IMapTileObject" />
    internal partial class MapTileObject : MonoBehaviour, IMapTileObject
    {
        private readonly Dictionary<Guid, MeshFilter> _meshesByBuilder = new();
        private readonly Dictionary<Guid, GameObject> _objectBuilderParents = new();
        private MapTheme _theme;

        public IReadOnlyList<FeatureBuilderBase> Builders => _theme.Builders;

        /// <inheritdoc />
        public Transform Transform => transform;

        /// <summary>
        /// Called when a <see cref="IMapTileObject"/> is added to a scene.
        /// </summary>
        /// <param name="mapTile">The <see cref="IMapTile"/> containing
        /// this <see cref="IMapTileObject"/>'s feature data.</param>
        /// <param name="lightshipMapView">The current tile's <see cref="ILightshipMapView"/>.</param>
        /// <param name="parent">The parent <see cref="GameObject"/> for all active tiles.</param>
        /// <param name="theme">The active <see cref="MapTheme"/></param>
        public void AddToScene(IMapTile mapTile, ILightshipMapView lightshipMapView, Transform parent, MapTheme theme)
        {
            name = $"Tile ({mapTile.TileCoordinateString})";

            var wm = new WebMercator12(mapTile.Origin);
            var position = lightshipMapView.WebMercator12ToScene(wm);
            var scale = (float)(lightshipMapView.MapScale * mapTile.Size) * Vector3.one;

            var tileTransform = transform;
            tileTransform.SetParent(parent, true);
            tileTransform.localScale = scale;
            tileTransform.position = position;

            SetActiveTheme(theme);
        }

        /// <summary>
        /// Recursively sets a layer value for this <see cref="MapTileObject"/>
        /// and all of its child objects in the hierarchy.
        /// </summary>
        /// <param name="layer">The layer value to set</param>
        public void SetLayerOnAllChildren(int layer)
        {
            SetLayerOnChildren(gameObject, layer);
        }

        /// <summary>
        /// Called by <see cref="SetLayerOnAllChildren"/> to recursively
        /// set a layer value on all child objects in the hierarchy.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/>
        /// whose layer should be set</param>
        /// <param name="layer">The layer value</param>
        private static void SetLayerOnChildren(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                SetLayerOnChildren(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Called when a maptile is removed from the scene.  This method cleans up
        /// meshes or other resources created by the <see cref="Build"/> method.
        /// </summary>
        public void Release()
        {
            foreach (var builder in Builders)
            {
                switch (builder)
                {
                    case IMeshBuilderAsync meshBuilderAsync:
                        meshBuilderAsync.Release(_meshesByBuilder[builder.Id]);
                        break;

                    case IObjectBuilderAsync objectBuilderAsync:
                        objectBuilderAsync.Release(_objectBuilderParents[builder.Id]);
                        break;

                    case IMeshBuilderStandard meshBuilder:
                        meshBuilder.Release(_meshesByBuilder[builder.Id]);
                        break;

                    case IObjectBuilderStandard objectBuilder:
                        objectBuilder.Release(_objectBuilderParents[builder.Id]);
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the active theme, performing cleanup and initialization if necessary.
        /// </summary>
        /// <param name="theme">The new <see cref="MapTheme"/></param>
        private void SetActiveTheme(MapTheme theme)
        {
            if (_theme == theme)
            {
                // Early-out if the theme hasn't changed
                return;
            }

            if (_theme.IsReferenceNotNull())
            {
                // Clean up objects created from the previous theme's builders
                DestroyBuilderObjects();
            }

            _theme = theme;

            InitializeBuilderObjects();
        }

        /// <summary>
        /// Initialize objects used by the current theme's builders
        /// </summary>
        private void InitializeBuilderObjects()
        {
            foreach (var builder in Builders)
            {
                switch (builder)
                {
                    case IMeshBuilderAsync meshBuilderAsync:
                        _meshesByBuilder.Add(builder.Id, meshBuilderAsync.CreateMeshComponents(this));
                        break;

                    case IMeshBuilderStandard meshBuilder:
                        _meshesByBuilder.Add(builder.Id, meshBuilder.CreateMeshComponents(this));
                        break;

                    case IObjectBuilderAsync objectBuilderAsync:
                        _objectBuilderParents.Add(builder.Id, objectBuilderAsync.CreateParent(this));
                        break;

                    case IObjectBuilderStandard objectBuilder:
                        _objectBuilderParents.Add(builder.Id, objectBuilder.CreateParent(this));
                        break;
                }
            }
        }

        /// <summary>
        /// Destroy objects created by the current theme's builders
        /// </summary>
        private void DestroyBuilderObjects()
        {
            Release();

            foreach (var meshFilter in _meshesByBuilder)
            {
                Destroy(meshFilter.Value);
            }

            foreach (var parentObject in _objectBuilderParents)
            {
                Destroy(parentObject.Value);
            }

            _meshesByBuilder.Clear();
            _objectBuilderParents.Clear();
        }
    }
}
