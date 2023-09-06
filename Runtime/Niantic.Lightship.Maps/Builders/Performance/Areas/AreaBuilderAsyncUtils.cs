// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections;

namespace Niantic.Lightship.Maps.Builders.Performance.Areas
{
    internal static class AreaBuilderAsyncUtils
    {
        /// <summary>
        /// Converts a collection of each <see cref="IMapTile"/>'s
        /// <see cref="IAreaFeature"/>s into a Job-friendly form.
        /// </summary>
        /// <param name="this">The list of <see cref="IAreaFeature"/>s by tile</param>
        /// <returns>A managed <see cref="Array"/> of a <see cref="NativeArray{T}"/>
        /// of each tile's <see cref="NativeAreaFeature"/>.</returns>
        public static NativeArray<NativeAreaFeature>[] ToNative(
            this IReadOnlyList<IReadOnlyList<IAreaFeature>> @this)
        {
            var nativeFeaturesByTile = new NativeArray<NativeAreaFeature>[@this.Count];

            for (int i = 0; i < nativeFeaturesByTile.Length; i++)
            {
                var tileFeatures = @this[i];
                var count = tileFeatures.Count;

                var nativeFeatures = new NativeArray<NativeAreaFeature>(count, Allocator.TempJob);
                nativeFeaturesByTile[i] = nativeFeatures;

                for (int j = 0; j < count; j++)
                {
                    nativeFeatures[j] = tileFeatures[j].ToNative();
                }
            }

            return nativeFeaturesByTile;
        }
    }
}
