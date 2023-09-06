// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.Areas
{
    /// <summary>
    /// A builder for <see cref="AreaFeature"/>s
    /// </summary>
    [PublicAPI]
    public class AreaFeatureBuilder : MeshBuilderStandard
    {
        /// <inheritdoc />
        public override void Build(IMapTile mapTile, MeshFilter meshFilter)
        {
            var combineInstances =
                Features.Count == 0
                    ? GetCombineInstancesForLayer(mapTile)
                    : GetCombineInstancesForLayerAndFeatures(mapTile);

            if (combineInstances.Count == 0)
            {
                return;
            }

            var combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances.ToArray(), true, false);

            foreach (var inst in combineInstances)
            {
                DestroyImmediate(inst.mesh);
            }

            SetMeshForFilter(combinedMesh, meshFilter);
        }

        private static Mesh MakeMesh(IAreaFeature feature)
        {
            if (feature.Points.Length == 0)
            {
                return null;
            }

            var pointCount = feature.Points.Length;
            var normals = new Vector3[pointCount];
            var uvs = new Vector2[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                var point = feature.Points[i];
                normals[i] = Vector3.up;
                uvs[i] = new Vector2(point.x, point.z);
            }

            // Since this is just a 2D mesh, we can ignore the exterior edges for now
            var newMesh = new Mesh
            {
                vertices = feature.Points,
                normals = normals,
                uv = uvs,
                triangles = feature.Indices
            };

            return newMesh;
        }

        private List<CombineInstance> GetCombineInstancesForLayer(IMapTile mapTile)
        {
            var combineInstances = new List<CombineInstance>();
            var features = mapTile.GetTileData(Layer);
            AddMeshesToList(features, combineInstances);
            return combineInstances;
        }

        private List<CombineInstance> GetCombineInstancesForLayerAndFeatures(IMapTile mapTile)
        {
            var combineInstances = new List<CombineInstance>();

            foreach (var featureKind in Features)
            {
                var features = mapTile.GetTileData(Layer, featureKind);
                AddMeshesToList(features, combineInstances);
            }

            return combineInstances;
        }

        private static void AddMeshesToList(
            IReadOnlyList<IMapTileFeature> features,
            List<CombineInstance> combineInstances)
        {
            foreach (var feature in features)
            {
                if (feature is IAreaFeature areaFeature)
                {
                    var mesh = MakeMesh(areaFeature);
                    combineInstances.Add(new CombineInstance { mesh = mesh });
                }
            }
        }
    }
}
