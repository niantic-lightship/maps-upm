// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Niantic.Lightship.Maps.Samples.Common.Utilities
{
    /// <summary>
    /// Utility class for helper methods related to <see cref="Quaternion"/>s.
    /// </summary>
    [PublicAPI]
    public static class QuaternionEx
    {
        /// <summary>
        /// Instantiates a <see cref="Quaternion"/> with a random
        /// rotation around a given <see cref="Vector3"/> axis.
        /// </summary>
        /// <param name="upAxis">The axis to rotate around</param>
        /// <returns>A new Quaternion representing this rotation</returns>
        public static Quaternion RandomLookRotation(in Vector3 upAxis)
        {
            return Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), upAxis);
        }

        /// <summary>
        /// Instantiates a <see cref="Quaternion"/> with a random
        /// rotation around the <see cref="Vector3.up"/> axis.
        /// </summary>
        /// <returns>A new Quaternion representing this rotation</returns>
        public static Quaternion RandomLookRotation()
        {
            return RandomLookRotation(Vector3.up);
        }
    }
}
