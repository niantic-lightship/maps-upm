// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation
{
    /// <summary>
    /// Base class used to create prefabs with one
    /// <see cref="MonoBehaviour"/> component.
    /// </summary>
    /// <typeparam name="T">The added component's type.</typeparam>
    internal class CreatePrefabBase<T> : CreatePrefabAction
        where T : MonoBehaviour
    {
        /// <inheritdoc />
        protected override GameObject CreateRootObject(string fileName)
        {
            // Create a root GameObject and add
            // our MonoBehaviour as a component.
            var rootObject = new GameObject(fileName);
            var component = rootObject.AddComponent<T>();

            // Call InitializeComponents(), if overridden
            InitializeComponents(rootObject, component);

            // Return the new root object
            return rootObject;
        }

        /// <summary>
        /// This method can be overridden if any customization
        /// of the new <see cref="MonoBehaviour"/> is required.
        /// </summary>
        /// <param name="rootObject">The prefab's root GameObject</param>
        /// <param name="monoBehaviour">A new MonoBehaviour attached to the root</param>
        protected virtual void InitializeComponents(GameObject rootObject, T monoBehaviour)
        {
        }
    }

    /// <summary>
    /// Base class used to create prefabs with two
    /// <see cref="MonoBehaviour"/> components.
    /// </summary>
    /// <typeparam name="TU">The first added component's type.</typeparam>
    /// <typeparam name="TV">The second added component's type.</typeparam>
    internal class CreatePrefabBase<TU, TV> : CreatePrefabAction
        where TU : MonoBehaviour
        where TV : MonoBehaviour
    {
        /// <inheritdoc />
        protected override GameObject CreateRootObject(string fileName)
        {
            // Create a root GameObject and add
            // our MonoBehaviours as components.
            var rootObject = new GameObject(fileName);
            var component1 = rootObject.AddComponent<TU>();
            var component2 = rootObject.AddComponent<TV>();

            // Call InitializeComponents(), if overridden
            InitializeComponents(rootObject, component1, component2);

            // Return the new root object
            return rootObject;
        }

        /// <summary>
        /// This method can be overridden if any customization
        /// of the new <see cref="MonoBehaviour"/>s are required.
        /// </summary>
        /// <param name="rootObject">The prefab's root GameObject</param>
        /// <param name="monoBehaviour1">A new MonoBehaviour component</param>
        /// <param name="monoBehaviour2">A new MonoBehaviour component</param>
        protected virtual void InitializeComponents(
            GameObject rootObject,
            TU monoBehaviour1,
            TV monoBehaviour2)
        {
        }
    }
}
