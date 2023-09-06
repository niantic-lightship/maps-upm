// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.AssetImporter
{
    // Commented out for package publication
    // Uncomment if needed for package development
    //[CreateAssetMenu(fileName = "ImporterConfig", menuName = "Lightship Maps/Importer Config")]
    internal class LightshipMapImporterConfig : ScriptableObject
    {
        [Tooltip("Root path for Lightship Maps asset imports, relative to Project root")]
        [SerializeField]
        private string _defaultImportPath;

        [SerializeField]
        private GameObject _lightshipMapPrefab;

        [SerializeField]
        private ImporterDirectory _directoryStructure;

        public string DefaultImportPath => _defaultImportPath.Replace('/', Path.DirectorySeparatorChar);
        public GameObject LightshipMapPrefab => _lightshipMapPrefab;

        public void CreateDirectories()
        {
            _directoryStructure.CreateDirectories(DefaultImportPath);
        }
    }
}
