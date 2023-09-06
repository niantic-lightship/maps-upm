// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Structs;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.Extensions;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Collections;
using IntReference = Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.UnsafeReference<int>;

namespace Niantic.Lightship.Maps.Builders.Performance.LinearFeatures
{
    internal static class LinearFeatureBuilderUtils
    {
        /// <summary>
        /// Prepares a <see cref="LinearFeatureSet"/>, as a part
        /// of a sequence of <see cref="LinearFeatureSet"/>s.
        /// </summary>
        /// <param name="feature">The current feature being prepared</param>
        /// <param name="currVertIndex">A reference to a helper offset used
        /// to calculate <see cref="LinearFeatureSet.NeededVerts"/>.</param>
        /// <param name="currIndexIndex">>A reference to a helper offset used
        /// to calculate <see cref="LinearFeatureSet.NeededIndices"/>.</param>
        public static void PrepareFeatureSet(
            LinearFeatureSet feature,
            IntReference currVertIndex,
            IntReference currIndexIndex)
        {
            var featureVertStartIndex = feature.VertStartIndex;
            featureVertStartIndex.Value = currVertIndex.Value;
            var featureIndexStartIndex = feature.IndexStartIndex;
            featureIndexStartIndex.Value = currIndexIndex.Value;
            currVertIndex.Value += feature.NeededVerts.Value;
            currIndexIndex.Value += feature.NeededIndices.Value;
        }

        /// <summary>
        /// Converts a collection of each <see cref="IMapTile"/>'s
        /// <see cref="ILinearFeature"/>s into a Job-friendly form.
        /// </summary>
        /// <param name="this">The list of <see cref="ILinearFeature"/>s by tile</param>
        /// <returns>A managed <see cref="Array"/> of a <see cref="NativeArray{T}"/> of each
        /// tile's <see cref="NativeLinearFeature"/>, wrapped in a helper <see cref="LinearFeatureSet"/></returns>
        public static NativeArray<LinearFeatureSet>[] ToNative(this IReadOnlyList<IReadOnlyList<ILinearFeature>> @this)
        {
            var nativeFeaturesByTile = new NativeArray<LinearFeatureSet>[@this.Count];

            for (int i = 0; i < nativeFeaturesByTile.Length; i++)
            {
                var tileFeatures = @this[i];
                var count = tileFeatures.Count;

                var nativeFeatures = new NativeArray<LinearFeatureSet>(count, Allocator.TempJob);
                nativeFeaturesByTile[i] = nativeFeatures;
                for (int j = 0; j < count; j++)
                {
                    nativeFeatures[j] = new LinearFeatureSet(tileFeatures[j].ToNative());
                }
            }

            return nativeFeaturesByTile;
        }
    }
}
