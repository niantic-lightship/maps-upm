// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.Editor
{
    /// <summary>
    /// Static class for menu-specific constant values
    /// </summary>
    internal static class MenuConstants
    {
        private const string CreateAssetsSubmenu = "Assets/Create/";
        private const string LightshipSubmenu = "Lightship/";
        private const string MapsSDKSubmenu = "Maps SDK/";

        public const string MainMenuRoot = LightshipSubmenu + MapsSDKSubmenu;
        public const string CreateAssetMenuRoot = MapsSDKSubmenu;

        public const string AssetsMenuCreateAssetRoot = CreateAssetsSubmenu + CreateAssetMenuRoot;
    }
}
