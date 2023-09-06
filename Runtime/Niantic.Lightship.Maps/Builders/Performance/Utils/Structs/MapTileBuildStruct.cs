// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Structs
{
    /// <summary>
    /// A helper struct containing NativeContainers
    /// needed to build a given tile's features.
    /// </summary>
    [BurstCompile]
    internal struct MapTileBuildStruct : IDisposable
    {
        /// <summary>
        /// Whether this struct has been properly
        /// constructed and has not yet been disposed.
        /// </summary>
        public bool IsCreated { get; private set; }

        /// <summary>
        /// A <see cref="Mesh.MeshDataArray"/> of size 1 whose only
        /// element contains the <see cref="Mesh.MeshData"/> representing
        /// the single combined mesh of all of a tile's features.
        /// </summary>
        /// <remarks>Must be Disposed separately from this struct, usually via
        /// <see cref="Mesh.ApplyAndDisposeWritableMeshData(Mesh.MeshDataArray, Mesh, MeshUpdateFlags)"/>
        /// </remarks>
        public Mesh.MeshDataArray CombinedFeatureMeshes { get; }

        private NativeList<Vertex> VerticesList { get; }
        private NativeArray<int> _vertexSubarraySizes;

        private NativeList<int> IndicesList { get; }
        private NativeArray<int> _indexSubarraySizes;

        private Mesh.MeshDataArray _featureMeshes;

        public MapTileBuildStruct(
            NativeList<Vertex> verticesList,
            NativeArray<int> vertexSubarraySizes,
            NativeList<int> indicesList,
            NativeArray<int> indexSubarraySizes,
            Mesh.MeshDataArray featureMeshes,
            Mesh.MeshDataArray combinedFeatureMeshes)
        {
            VerticesList = verticesList;
            _vertexSubarraySizes = vertexSubarraySizes;
            IndicesList = indicesList;
            _indexSubarraySizes = indexSubarraySizes;
            IsCreated = true;
            _featureMeshes = featureMeshes;
            CombinedFeatureMeshes = combinedFeatureMeshes;
        }

        /// <summary>
        /// Disposes this struct's Native Collections
        /// </summary>
        /// <remarks><see cref="CombinedFeatureMeshes"/> must be Disposed separately</remarks>
        public void Dispose()
        {
            if (!IsCreated)
            {
                return;
            }

            if (VerticesList.IsCreated)
            {
                VerticesList.Dispose();
            }

            if (_vertexSubarraySizes.IsCreated)
            {
                _vertexSubarraySizes.Dispose();
            }

            if (IndicesList.IsCreated)
            {
                IndicesList.Dispose();
            }

            if (_indexSubarraySizes.IsCreated)
            {
                _indexSubarraySizes.Dispose();
            }

            _featureMeshes.Dispose();

            IsCreated = false;
        }
    }
}
