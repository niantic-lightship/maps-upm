// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.AssetImporter
{
    [Serializable]
    internal class ImporterDirectory
    {
        [SerializeField]
        private string _directoryName;

        [SerializeField]
        private string[] _subDirectories;

        public void CreateDirectories(string pathRoot)
        {
            var directoryToCreate = Path.Combine(pathRoot, _directoryName);

            if (!Directory.Exists(directoryToCreate))
            {
                Directory.CreateDirectory(directoryToCreate);
            }

            foreach (var subdirectory in _subDirectories)
            {
                if (string.IsNullOrEmpty(subdirectory))
                {
                    continue;
                }

                var subdirectoryPath = Path.Combine(directoryToCreate, subdirectory);

                if (!Directory.Exists(subdirectoryPath))
                {
                    Directory.CreateDirectory(subdirectoryPath);
                }
            }
        }
    }
}
