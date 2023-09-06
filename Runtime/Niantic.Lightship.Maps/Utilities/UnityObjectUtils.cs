// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.ObjectPools;
using UnityEngine;

namespace Niantic.Lightship.Maps.Utilities
{
    /// <summary>
    /// Utility class used to enable or disable <see cref="GameObject"/>s
    /// and hide or un-hide them in the scene's hierarchy.
    /// </summary>
    internal static class UnityObjectUtils
    {
        /// <summary>
        /// Detaches the <see cref="GameObject"/> from its parent,
        /// sets it to inactive, and hides it in the hierarchy.
        /// This may be called when an object is returned to an
        /// <see cref="ObjectPool{T}"/>, for example.
        /// </summary>
        /// <param name="gameObject">The object to disable</param>
        public static void DisableAndHide(GameObject gameObject)
        {
            // Detach this child object from its parent
            gameObject.transform.SetParent(null, false);

            // Disable and hide this object in the hierarchy
            gameObject.hideFlags |= HideFlags.HideInHierarchy;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Un-hides a <see cref="GameObject"/> in the hierarchy
        /// (if it was previously hidden), and sets it to active.
        /// </summary>
        /// <param name="gameObject">The object to enable</param>
        public static void EnableAndShow(GameObject gameObject)
        {
            // Enable and un-hide this object (if it was pooled)
            gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
            gameObject.SetActive(true);
        }
    }
}
