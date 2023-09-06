// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor
{
    /// <summary>
    /// Static class for Editor-specific constant values
    /// </summary>
    internal static class EditorConstants
    {
        public static readonly Texture2D PrefabIcon = EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D;
    }
}
