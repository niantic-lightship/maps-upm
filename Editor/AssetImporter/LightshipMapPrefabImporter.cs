// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using Niantic.Lightship.Maps.Linq;
using Niantic.Lightship.Maps.Themes;
using Niantic.Platform.Debugging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Niantic.Lightship.Maps.Editor.AssetImporter
{
    internal static class LightshipMapPrefabImporter
    {
        [MenuItem(MenuConstants.MainMenuRoot + "Add Lightship Map To Scene")]
        public static void ImportLightshipMapPrefab()
        {
            try
            {
                var foldersToSearch = new[]
                {
                    "Packages/com.niantic.lightship.maps", // Found here in published package
                    "Assets/" // Found here during package development
                };

                var importerConfigGuids = AssetDatabase.FindAssets("t:LightshipMapImporterConfig", foldersToSearch);

                // This should NEVER happen in the published package
                if (importerConfigGuids.IsEmpty())
                {
                    Log.Error("No LightshipMapImporterConfig instance found.  " +
                        "If you are a Lightship Developer and are seeing this error, " +
                        "please contact the Lightship Team.");
                    return;
                }

                var assetPath = AssetDatabase.GUIDToAssetPath(importerConfigGuids[0]);
                var importerConfig = AssetDatabase.LoadAssetAtPath<LightshipMapImporterConfig>(assetPath);
                var importPath = importerConfig.DefaultImportPath;
                importerConfig.CreateDirectories();

                // Copy and Unpack LightshipMapView prefab
                var lightshipMapPrefab =
                    PrefabUtility.InstantiatePrefab(importerConfig.LightshipMapPrefab) as GameObject;

                PrefabUtility.UnpackPrefabInstance(
                    lightshipMapPrefab,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);

                // Save unpacked prefab as new prefab in Assets
                var savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                    lightshipMapPrefab,
                    Path.Combine(importPath, "Prefabs", "LightshipMapView.prefab"),
                    InteractionMode.AutomatedAction);

                // Replace all the (relevant) references
                ReplaceReferencesAndDuplicateAssets(importPath, lightshipMapPrefab);
                PrefabUtility.ApplyPrefabInstance(lightshipMapPrefab, InteractionMode.AutomatedAction);

                AssetDatabase.SaveAssetIfDirty(savedPrefab);

                Selection.activeObject = lightshipMapPrefab;
                EditorGUIUtility.PingObject(savedPrefab);

                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void ReplaceReferencesAndDuplicateAssets(string importPath, GameObject prefab)
        {
            var serializedPrefab = new SerializedObject(prefab.GetComponent<LightshipMapView>());

            // Duplicate the default theme
            var themeProp = serializedPrefab.FindProperty("_mapTheme");
            var oldTheme = themeProp.objectReferenceValue as MapTheme;

            var newTheme = DuplicateAsset(oldTheme, Path.Combine(importPath, "Themes"));
            themeProp.objectReferenceValue = newTheme;

            serializedPrefab.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T DuplicateAsset<T>(T oldAsset, string newParentDir) where T : Object
        {
            var oldPath = AssetDatabase.GetAssetPath(oldAsset);
            var assetName = Path.GetFileName(oldPath);
            var newPath = Path.Combine(newParentDir, assetName);
            var asset = AssetDatabase.LoadAssetAtPath<T>(newPath);

            if (asset)
            {
                return asset;
            }

            AssetDatabase.CopyAsset(oldPath, newPath);
            return AssetDatabase.LoadAssetAtPath<T>(newPath);
        }
    }
}
