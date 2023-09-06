// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.MapLayers;
using UnityEditor;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.MapLayers
{
    /// <summary>
    /// <see cref="MenuItem"/>s and constants used
    /// in creating <see cref="MapLayer"/> prefabs.
    /// </summary>
    internal static class MenuItems
    {
        private const string Submenu = "Map Layer Prefabs/";
        private const string Menu = Root + Submenu;
        private const string Root = MenuConstants.AssetsMenuCreateAssetRoot;

        #region MenuItems

        [MenuItem(Menu + "Empty Map Layer")]
        public static void NewEmptyMapLayer()
        {
            CreatePrefabAction.Invoke<CreateEmptyMapLayer>();
        }

        [MenuItem(Menu + "Line Rendering Layer")]
        public static void NewLineRenderingLayer()
        {
            CreatePrefabAction.Invoke<CreateLineRenderingMapLayer>();
        }

        [MenuItem(Menu + "Polygon Rendering Layer")]
        public static void NewPolygonRenderingLayer()
        {
            CreatePrefabAction.Invoke<CreatePolygonRenderingMapLayer>();
        }

        [MenuItem(Menu + "GameObject Placement Layer")]
        public static void NewObjectPlacementLayer()
        {
            CreatePrefabAction.Invoke<CreateGameObjectPlacementMapLayer>();
        }

        [MenuItem(Menu + "GameObject Instance Layer")]
        public static void NewObjectInstanceLayer()
        {
            CreatePrefabAction.Invoke<CreateGameObjectInstanceMapLayer>();
        }

        #endregion
    }
}
