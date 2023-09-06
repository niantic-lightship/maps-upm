// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEditor;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.LightshipMaps
{
    /// <summary>
    /// <see cref="MenuItem"/>s and constants used in
    /// creating <see cref="LightshipMapView"/> prefabs.
    /// </summary>
    internal static class MenuItems
    {
        private const string Submenu = "Lightship Map Prefabs/";
        private const string Menu = Root + Submenu;
        private const string Root = MenuConstants.AssetsMenuCreateAssetRoot;

        #region MenuItems

        [MenuItem(Menu + "LightshipMap Prefab")]
        public static void NewLightshipMap()
        {
            CreatePrefabAction.Invoke<CreateLightshipMap>();
        }

        [MenuItem(Menu + "LightshipMapManager Prefab")]
        public static void NewLightshipMapManager()
        {
            CreatePrefabAction.Invoke<CreateLightshipMapManager>();
        }

        [MenuItem(Menu + "LightshipMapView Prefab")]
        public static void NewLightshipMapView()
        {
            CreatePrefabAction.Invoke<CreateLightshipMapView>();
        }

        #endregion
    }
}
