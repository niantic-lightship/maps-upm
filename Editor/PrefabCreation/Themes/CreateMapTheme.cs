// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Themes;
using Niantic.Lightship.Maps.Themes.Editor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.Themes
{
    /// <summary>
    /// Creates a new <see cref="MapTheme"/> prefab.
    /// </summary>
    internal class CreateMapTheme : CreatePrefabBase<MapTheme>
    {
        /// <inheritdoc />
        protected override void InitializeComponents(GameObject rootObject, MapTheme component)
        {
            base.InitializeComponents(rootObject, component);
            IMapThemeWritable mapTheme = component;
            mapTheme.ThemeName = rootObject.name;
        }
    }
}
