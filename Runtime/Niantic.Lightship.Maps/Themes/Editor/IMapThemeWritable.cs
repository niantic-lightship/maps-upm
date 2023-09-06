// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;

namespace Niantic.Lightship.Maps.Themes.Editor
{
    /// <summary>
    /// This internal, Editor-only interface exposes serialized fields
    /// and other methods that are meant to be used to modify internal
    /// state of <see cref="MapTheme"/>s programmatically.
    /// </summary>
    internal interface IMapThemeWritable
    {
        /// <summary>
        /// The theme's name
        /// </summary>
        string ThemeName { set; }
    }
}

#endif
