// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections;

namespace Niantic.Lightship.Maps.Builders.Performance.Structures
{
    internal static class StructureFeatureBuilderUtils
    {
        /// <summary>
        /// Converts a collection of each <see cref="IMapTile"/>'s
        /// <see cref="IStructureFeature"/>s into a Job-friendly form.
        /// </summary>
        /// <param name="this">The list of <see cref="IStructureFeature"/>s by tile</param>
        /// <returns>A managed <see cref="Array"/> of a <see cref="NativeArray{T}"/>
        /// of each tile's <see cref="NativeStructureFeature"/>s.</returns>
        public static NativeArray<NativeStructureFeature>[] ToNative(
            this IReadOnlyList<IReadOnlyList<IStructureFeature>> @this)
        {
            var nativeFeaturesByTile = new NativeArray<NativeStructureFeature>[@this.Count];

            for (int i = 0; i < nativeFeaturesByTile.Length; i++)
            {
                var tileFeatures = @this[i];
                var count = tileFeatures.Count;

                var nativeFeatures = new NativeArray<NativeStructureFeature>(count, Allocator.TempJob);
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
