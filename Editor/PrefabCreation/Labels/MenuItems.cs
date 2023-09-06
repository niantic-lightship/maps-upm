// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.SampleAssets.Experimental.Labels;
using UnityEditor;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.Labels
{
    /// <summary>
    /// <see cref="MenuItem"/>s and constants used in
    /// creating <see cref="LabelObject"/> prefabs.
    /// </summary>
    internal static class MenuItems
    {
        private const string Submenu = "Labels/";
        private const string Menu = Root + Submenu;
        private const string Root = MenuConstants.AssetsMenuCreateAssetRoot;

        #region MenuItems

        [MenuItem(Menu + "TextMesh Label")]
        public static void NewLabel()
        {
            CreatePrefabAction.Invoke<CreateTextMeshLabel>();
        }

        #endregion
    }
}
