// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Niantic.Lightship.Maps.Utilities;
using Niantic.Platform.Debugging;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation
{
    /// <summary>
    /// Base class for derived types that can be used to create prefabs.
    /// </summary>
    internal abstract class CreatePrefabAction : EndNameEditAction
    {
        /// <summary>
        /// A <see cref="ChannelLogger"/> whose channel name
        /// matches the concrete type derived from this class.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected ChannelLogger Log { get; }

        /// <summary>
        /// This method is implemented by derived types, and
        /// is used to create a root <see cref="GameObject"/>
        /// with any necessary components and child objects
        /// attached that will be saved as a prefab.
        /// </summary>
        /// <param name="fileName">The name of the prefab to save.</param>
        /// <returns>The root <see cref="GameObject"/></returns>
        protected abstract GameObject CreateRootObject(string fileName);

        /// <summary>
        /// Constructor
        /// </summary>
        protected CreatePrefabAction()
        {
            Log = new ChannelLogger(GetType().Name);
        }

        /// <summary>
        /// Static helper method used to invoke the prefab
        /// creation methods implemented in derived types.
        /// </summary>
        /// <param name="defaultFileName">The default file
        /// name that will be used for the new prefab.
        /// Note: This parameter is optional, and if omitted,
        /// will be the name of the calling method (via the
        /// <see cref="CallerMemberNameAttribute"/> attribute).
        /// </param>
        /// <typeparam name="T">The concrete type that will
        /// be invoked to create a new prefab.</typeparam>
        public static void Invoke<T>(
            [CallerMemberName] string defaultFileName = null)
            where T : CreatePrefabAction
        {
            const int instanceId = 0;
            const string resourceFile = null;
            var icon = EditorConstants.PrefabIcon;
            var createAction = CreateInstance<T>();
            var fileName = $"{defaultFileName}.prefab";

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                instanceId, createAction, fileName, icon, resourceFile);
        }

        /// <summary>
        /// Delegate called by
        /// <see cref="ProjectWindowUtil.StartNameEditingIfProjectWindowExists"/>
        /// to create and save our new prefab.
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="prefabFilePath"></param>
        /// <param name="resourceFile"></param>
        public override void Action(int instanceId, string prefabFilePath, string resourceFile)
        {
            // Get the prefab file's name, which is often used to name root objects
            var fileName = Path.GetFileNameWithoutExtension(prefabFilePath);

            // Call the derived class's CreateRootObject method
            var rootObject = CreateRootObject(fileName);

            // Save our new root GameObject as a prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabFilePath, out var success);

            // Log whether the prefab was saved successfully
            var message = $"{(success ? "Created" : "Failed to create")} prefab '{prefabFilePath}'";
            var severity = success ? LogLevel.Info : LogLevel.Error;
            Log.LogMessage(severity, message);

            // Destroy the root object, since it's no longer needed
            DestroyImmediate(rootObject);

            if (success)
            {
                // Open the new prefab in the Inspector
                ProjectWindowUtil.ShowCreatedAsset(prefab);
                AssetDatabase.OpenAsset(prefab);
            }
        }
    }
}
