// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Core.Geometry;
using Niantic.Lightship.Maps.Linq;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.Structures
{
    /// <summary>
    /// A builder for <see cref="StructureFeature"/>s
    /// </summary>
    [PublicAPI]
    public class StructureFeatureBuilder : MeshBuilderStandard
    {
        [Tooltip("The minimum height of generated building meshes.")]
        [SerializeField]
        private float _minHeight;

        [Tooltip("The maximum height of generated building meshes.")]
        [SerializeField]
        private float _maxHeight = 0.2f;

        /// <inheritdoc />
        public override void Build(IMapTile mapTile, MeshFilter meshFilter)
        {
            var combineInstances = Features.IsEmpty()
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

        private void AddMeshesToList(
            IReadOnlyList<IMapTileFeature> features,
            List<CombineInstance> combineInstances)
        {
            foreach (var feature in features)
            {
                if (feature is IStructureFeature structureFeature)
                {
                    AddMeshesToList(structureFeature, combineInstances);
                }
            }
        }

        private void AddMeshesToList(IStructureFeature structureData, List<CombineInstance> combineInstances)
        {
            var height = Mathf.Clamp(structureData.Height, _minHeight, _maxHeight);

            var pointCount = structureData.Points.Length;
            var normals = new Vector3[pointCount];
            var uvs = new Vector2[pointCount];

            for (int i = 0; i < pointCount; i++)
            {
                normals[i] = Vector3.up;
                uvs[i] = new Vector2(0.25f, 0.25f);
            }

            var topMesh = new Mesh
            {
                vertices = MoveUp(structureData.Points, height),
                uv = uvs,
                normals = normals,
                triangles = structureData.Indices
            };

            var wallMesh = GenerateWallMesh(structureData.ExteriorEdges, height);

            combineInstances.Add(new CombineInstance { mesh = topMesh });
            combineInstances.Add(new CombineInstance { mesh = wallMesh });
        }

        private static Vector3[] MoveUp(Vector3[] points, float height)
        {
            var delta = Vector3.up * height;
            var outPoints = new Vector3[points.Length];

            for (int i = 0; i < points.Length; ++i)
            {
                outPoints[i] = points[i] + delta;
            }

            return outPoints;
        }

        private static Mesh GenerateWallMesh(LineSegment[] exteriorEdges, float height)
        {
            var wallCount = exteriorEdges.Length;
            var vertices = new Vector3[wallCount * 4];
            var normals = new Vector3[vertices.Length];
            var triangles = new int[wallCount * 6];
            var uvs = new Vector2[vertices.Length];
            var heightOffset = Vector3.up * height;

            int vertexIndex = 0;
            int triIndex = 0;

            foreach (var exteriorEdge in exteriorEdges)
            {
                var edge = exteriorEdge.VertexB - exteriorEdge.VertexA;
                var normal = Vector3.Cross(Vector3.up, edge).normalized;

                // Vertices
                var vertexIndex0 = vertexIndex++;
                var vertexIndex1 = vertexIndex++;
                var vertexIndex2 = vertexIndex++;
                var vertexIndex3 = vertexIndex++;

                vertices[vertexIndex0] = exteriorEdge.VertexA;
                vertices[vertexIndex1] = exteriorEdge.VertexB;
                vertices[vertexIndex2] = exteriorEdge.VertexA + heightOffset;
                vertices[vertexIndex3] = exteriorEdge.VertexB + heightOffset;

                // Normals
                normals[vertexIndex0] = normal;
                normals[vertexIndex1] = normal;
                normals[vertexIndex2] = normal;
                normals[vertexIndex3] = normal;

                // Triangles
                triangles[triIndex++] = vertexIndex2;
                triangles[triIndex++] = vertexIndex1;
                triangles[triIndex++] = vertexIndex0;

                triangles[triIndex++] = vertexIndex2;
                triangles[triIndex++] = vertexIndex3;
                triangles[triIndex++] = vertexIndex1;
            }

            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(0.75f, 0.75f);
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                normals = normals,
                triangles = triangles
            };

            return mesh;
        }
    }
}
