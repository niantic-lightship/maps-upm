// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Core.Features;

namespace Niantic.Lightship.Maps.Builders.Standard.LinearFeatures
{
    internal class LinearFeatureSet
    {
        public LinearFeatureSet(ILinearFeature linearFeature)
        {
            LinearFeature = linearFeature;
        }

        public int NeededVerts;
        public int VertStartIndex;
        public int NeededIndices;
        public int IndicesStartIndex;
        public readonly ILinearFeature LinearFeature;
    }
}
