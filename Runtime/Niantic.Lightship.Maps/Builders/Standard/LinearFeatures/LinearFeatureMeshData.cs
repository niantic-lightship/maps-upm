// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.LinearFeatures
{
    internal class LinearFeatureMeshData
    {
        public LinearFeatureMeshData(int vertCount, int indexCount)
        {
            Vertices = new Vector3[vertCount];
            Indices = new int[indexCount];
        }

        public readonly Vector3[] Vertices;
        public readonly int[] Indices;
    }
}
