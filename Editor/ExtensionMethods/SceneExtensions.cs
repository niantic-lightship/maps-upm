// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Niantic.Lightship.Maps.Editor.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="Scene"/>
    /// </summary>
    internal static class SceneExtensions
    {
        /// <summary>
        /// Enumerates all <see cref="GameObject"/>s in a
        /// given <see cref="Scene"/> that contain the
        /// specified <see cref="Component"/>.
        /// </summary>
        /// <param name="scene">The scene to enumerate</param>
        /// <param name="onlyActive">If true, only return active objects</param>
        /// <typeparam name="T">The Component type to search for</typeparam>
        /// <returns>Scene objects containing the specified Component</returns>
        public static IEnumerable<T> EnumerateGameObjectsWithComponent<T>(
            this Scene scene, bool onlyActive = false)
            where T : Component
        {
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                foreach (var child in rootObject.GetComponentsInChildren<T>(!onlyActive))
                {
                    yield return child;
                }
            }
        }
    }
}
