// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Themes
{
    internal static class SkyboxUtils
    {
        public static Material DefaultSkybox { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetDefaultSkybox() => DefaultSkybox = RenderSettings.skybox;
    }
}
