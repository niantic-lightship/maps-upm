// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Configuration
{
    internal class MapsAuthConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Obtain an API key from your Lightship Account Dashboard and copy it in here.")]
        private string _lightshipApiKey = "";

        private const string ResourcesDir = "Maps SDK";

        public string LightshipAPIKey
        {
            get => _lightshipApiKey;
            internal set => _lightshipApiKey = value;
        }

        public static MapsAuthConfig[] Load()
        {
            return Resources.LoadAll<MapsAuthConfig>(ResourcesDir) ?? Array.Empty<MapsAuthConfig>();
        }

#if UNITY_EDITOR
        public static MapsAuthConfig Create()
        {
            var resourcesPath = Path.Combine("Assets", "Resources", ResourcesDir);
            var assetPath = Path.Combine(resourcesPath, "MapsAuthConfig.asset");

            Directory.CreateDirectory(resourcesPath);
            var authConfig = CreateInstance<MapsAuthConfig>();
            AssetDatabase.CreateAsset(authConfig, assetPath.Replace('\\', '/'));
            EditorUtility.SetDirty(authConfig);
            AssetDatabase.SaveAssetIfDirty(authConfig);
            AssetDatabase.Refresh();

            return authConfig;
        }
#endif
    }
}
