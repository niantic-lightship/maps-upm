// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityObject = UnityEngine.Object;

namespace Niantic.Lightship.Maps.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="UnityObject"/>
    /// </summary>
    [PublicAPI]
    public static class ObjectExtensions
    {
        /// <summary>
        /// Checks if the reference to a type derived from <see cref="UnityObject"/>
        /// is null.  Unity overrides many of the equality and null coalescing and
        /// propagation operators to perform lifetime checks of the underlying Unity
        /// engine object, so this extension method is a workaround for cases where
        /// the desired behavior is just a simple null check.  For more information, see
        /// <see href="https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object">
        /// this page from JetBrains.</see>
        /// </summary>
        /// <param name="unityObject">The Unity Object to check for null</param>
        /// <returns>True if the reference is null</returns>
        [ContractAnnotation("null => true; notnull => false")]
        public static bool IsReferenceNull(this UnityObject unityObject)
        {
            return ReferenceEquals(unityObject, null);
        }

        /// <summary>
        /// Checks if the reference to a type derived from <see cref="UnityObject"/>
        /// is not null.  Unity overrides many of the equality and null coalescing and
        /// propagation operators to perform lifetime checks of the underlying Unity
        /// engine object, so this extension method is a workaround for cases where
        /// the desired behavior is just a simple null check.  For more information, see
        /// <see href="https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object">
        /// this page from JetBrains.</see>
        /// </summary>
        /// <param name="unityObject">The Unity Object to check for null</param>
        /// <returns>True if the reference is not null</returns>
        [ContractAnnotation("null => false; notnull => true")]
        public static bool IsReferenceNotNull(this UnityObject unityObject)
        {
            return !ReferenceEquals(unityObject, null);
        }
    }
}
