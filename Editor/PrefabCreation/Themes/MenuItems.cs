// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Themes;
using UnityEditor;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.Themes
{
    /// <summary>
    /// <see cref="MenuItem"/>s and constants used
    /// in creating <see cref="MapTheme"/> prefabs.
    /// </summary>
    internal static class MenuItems
    {
        private const string Menu = MenuConstants.AssetsMenuCreateAssetRoot;

        #region MenuItems

        [MenuItem(Menu + "Map Theme Prefab", priority = 10)]
        public static void NewMapTheme()
        {
            CreatePrefabAction.Invoke<CreateMapTheme>();
        }

        #endregion
    }
}
