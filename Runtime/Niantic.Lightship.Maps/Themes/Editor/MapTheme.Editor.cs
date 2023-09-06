// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Themes.Editor;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Niantic.Lightship.Maps.Themes
{
    /// <inheritdoc cref="IMapThemeWritable" />
    public partial class MapTheme : IMapThemeWritable
    {
        #region IMapThemeWritable

        /// <inheritdoc />
        string IMapThemeWritable.ThemeName { set => _themeName = value; }

        #endregion

        private void OnValidate()
        {
            if (_showWarning && _skybox != null && !_skybox.HasProperty(Tint))
            {
                const string title = "WARNING!";
                const string message = "Skybox color cannot be applied to this material.";
                const string okButtonLabel = "OK";
                const string cancelButtonLabel = "Do Not Show Again";

                _showWarning = EditorUtility.DisplayDialog(
                    title,
                    message,
                    okButtonLabel,
                    cancelButtonLabel);
            }
        }
    }
}

#endif
