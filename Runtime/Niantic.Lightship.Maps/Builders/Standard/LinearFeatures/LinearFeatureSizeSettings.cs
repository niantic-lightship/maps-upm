// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;

namespace Niantic.Lightship.Maps.Builders.Standard.LinearFeatures
{
    /// <summary>
    /// Pre-set sizes for linear features
    /// </summary>
    [PublicAPI]
    public enum LinearFeatureSize
    {
        /// <summary>
        /// User-specified custom values
        /// </summary>
        Custom,

        Small,
        Medium,
        Large
    }

    /// <summary>
    /// An asset containing pre-set values for
    /// several categories of linear feature sizes.
    /// </summary>
    [PublicAPI]
    public static class LinearFeatureSizeSettings
    {
         public const float SmallLinearFeatureMin = 0.01f;
         public const float SmallLinearFeatureMax = 0.02f;

         public const float MedLinearFeatureMin = 0.02f;
         public const float MedLinearFeatureMax = 0.04f;

         public const float LargeLinearFeatureMin = 0.01f;
         public const float LargeLinearFeatureMax = 0.07f;
    }
}
