// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Standard.LinearFeatures
{
    internal static class LinearFeatureBuilderUtils
    {
        private static void InsertIndices(int[] indices, int vertexCount, ref int indicesIndex)
        {
            var currentVertexCount = vertexCount - 4;
            indices[indicesIndex++] = currentVertexCount;
            indices[indicesIndex++] = currentVertexCount + 2;
            indices[indicesIndex++] = currentVertexCount + 1;

            indices[indicesIndex++] = currentVertexCount + 1;
            indices[indicesIndex++] = currentVertexCount + 2;
            indices[indicesIndex++] = currentVertexCount + 3;
        }

        private static void InsertSmoothingVertices(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3[] vertices,
            int[] indices,
            ref int vertexIndex,
            ref int indicesIndex,
            float thickness,
            float targetCos,
            int depth,
            float smoothingFactor
        )
        {
            while (true)
            {
                // Using http://graphics.cs.ucdavis.edu/education/CAGDNotes/Chaikins-Algorithm/Chaikins-Algorithm.html

                var p1ReplacementA = smoothingFactor * p0 + (1.0f - smoothingFactor) * p1;
                var p1ReplacementB = (1.0f - smoothingFactor) * p1 + smoothingFactor * p2;

                var insertedTangent0 = (p1ReplacementA - p0).normalized;
                var insertedTangent1 = (p1ReplacementB - p1ReplacementA).normalized;
                var insertedTangent2 = (p2 - p1ReplacementB).normalized;

                if (depth > 0 && Vector3.Dot(insertedTangent0, insertedTangent1) < targetCos)
                {
                    InsertSmoothingVertices(
                        p0,
                        p1ReplacementA,
                        p1ReplacementB,
                        vertices,
                        indices,
                        ref vertexIndex,
                        ref indicesIndex,
                        thickness,
                        targetCos,
                        depth - 1,
                        smoothingFactor
                    );
                }
                else
                {
                    var insertedBiNormal =
                        Vector3.Cross((insertedTangent0 + insertedTangent1) * 0.5f, Vector3.up) * 0.5f;

                    vertices[vertexIndex++] = p1ReplacementA + insertedBiNormal * thickness;
                    vertices[vertexIndex++] = p1ReplacementA - insertedBiNormal * thickness;

                    InsertIndices(indices, vertexIndex, ref indicesIndex);
                }

                if (depth > 0 && Vector3.Dot(insertedTangent1, insertedTangent2) < targetCos)
                {
                    p0 = p1ReplacementA;
                    p1 = p1ReplacementB;
                    depth -= 1;
                    continue;
                }

                {
                    var insertedBiNormal =
                        Vector3.Cross((insertedTangent1 + insertedTangent2) * 0.5f, Vector3.up) * 0.5f;

                    vertices[vertexIndex++] = p1ReplacementB + insertedBiNormal * thickness;
                    vertices[vertexIndex++] = p1ReplacementB - insertedBiNormal * thickness;

                    InsertIndices(indices, vertexIndex, ref indicesIndex);
                }

                break;
            }
        }

        public static void CreateMiteredWidePolyline(
            Vector3[] vertices,
            int[] indices,
            int startVertexIndex,
            int startIndicesIndex,
            Vector3[] points,
            int pointOffset,
            int strip,
            float thickness,
            out int endVertexIndex,
            out int endIndicesIndex,
            int endCapPointCount,
            float bendThreshold,
            float smoothFactor
        )
        {
            endVertexIndex = startVertexIndex;
            endIndicesIndex = startIndicesIndex;

            if (strip < 2)
            {
                return;
            }

            var tangent0 = (points[1 + pointOffset] - points[pointOffset]).normalized;
            var biNormal = Vector3.Cross(tangent0, Vector3.up);

            // Hold this as the base index for the front cap
            var baseCapVertexIndex = endVertexIndex;

            // Add front cap
            for (var i = 0; i < endCapPointCount; ++i)
            {
                var angle = Mathf.PI * (i + 1) / (endCapPointCount + 1);
                vertices[endVertexIndex++] =
                    points[pointOffset]
                    - (Mathf.Sin(angle) * tangent0 + Mathf.Cos(angle) * biNormal) * (thickness * 0.5f);

                indices[endIndicesIndex++] = baseCapVertexIndex + i;
                indices[endIndicesIndex++] = baseCapVertexIndex + endCapPointCount;

                indices[endIndicesIndex++] =
                    i == 0 ? baseCapVertexIndex + endCapPointCount + 1 : baseCapVertexIndex + i - 1;
            }

            for (var i = 0; i < strip - 1; ++i)
            {
                tangent0 = (points[i + 1 + pointOffset] - points[i + pointOffset]).normalized;
                biNormal = Vector3.Cross(tangent0, Vector3.up) * 0.5f;

                if (i == 0)
                {
                    vertices[endVertexIndex++] = points[i + pointOffset] + biNormal * thickness;
                    vertices[endVertexIndex++] = points[i + pointOffset] - biNormal * thickness;
                }

                if (i < strip - 2)
                {
                    var tangent1 = (points[i + 2 + pointOffset] - points[i + 1 + pointOffset]).normalized;

                    // If angle is too steep we iteratively insert new segments
                    if (Vector3.Dot(tangent0, tangent1) < bendThreshold)
                    {
                        InsertSmoothingVertices(
                            points[i + pointOffset],
                            points[i + 1 + pointOffset],
                            points[i + 2 + pointOffset],
                            vertices,
                            indices,
                            ref endVertexIndex,
                            ref endIndicesIndex,
                            thickness,
                            bendThreshold,
                            3,
                            smoothFactor
                        );
                        continue;
                    }

                    biNormal += Vector3.Cross(tangent1, Vector3.up) * 0.5f;
                    biNormal *= 0.5f;
                }

                vertices[endVertexIndex++] = points[i + 1 + pointOffset] + biNormal * thickness;
                vertices[endVertexIndex++] = points[i + 1 + pointOffset] - biNormal * thickness;

                InsertIndices(indices, endVertexIndex, ref endIndicesIndex);
            }

            tangent0 = (points[pointOffset + strip - 1] - points[pointOffset + strip - 2]).normalized;
            biNormal = Vector3.Cross(tangent0, Vector3.up);

            // Again, hold this value as base for the end cap
            baseCapVertexIndex = endVertexIndex;

            // Add end cap
            for (var i = 0; i < endCapPointCount; ++i)
            {
                var angle = Mathf.PI * (i + 1) / (endCapPointCount + 1);
                vertices[endVertexIndex++] =
                    points[pointOffset + strip - 1]
                    + (Mathf.Sin(angle) * tangent0 + Mathf.Cos(angle) * biNormal) * (thickness * 0.5f);

                indices[endIndicesIndex++] = baseCapVertexIndex + i;
                indices[endIndicesIndex++] = baseCapVertexIndex - 1;

                indices[endIndicesIndex++] = i == 0 ? baseCapVertexIndex - 2 : baseCapVertexIndex + i - 1;
            }
        }

        public static void CalculateSmoothingVerts(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            ref int vertCount,
            ref int indCount,
            float targetCos,
            int depth,
            float smoothFactor
        )
        {
            while (true)
            {
                // Using http://graphics.cs.ucdavis.edu/education/CAGDNotes/Chaikins-Algorithm/Chaikins-Algorithm.html

                var p1ReplacementA = smoothFactor * p0 + (1.0f - smoothFactor) * p1;
                var p1ReplacementB = (1.0f - smoothFactor) * p1 + smoothFactor * p2;

                var insertedTangent0 = (p1ReplacementA - p0).normalized;
                var insertedTangent1 = (p1ReplacementB - p1ReplacementA).normalized;
                var insertedTangent2 = (p2 - p1ReplacementB).normalized;

                if (depth > 0 && Vector3.Dot(insertedTangent0, insertedTangent1) < targetCos)
                {
                    CalculateSmoothingVerts(
                        p0,
                        p1ReplacementA,
                        p1ReplacementB,
                        ref vertCount,
                        ref indCount,
                        targetCos,
                        depth - 1,
                        smoothFactor
                    );
                }
                else
                {
                    vertCount += 2;
                    indCount += 6;
                }

                if (depth > 0 && Vector3.Dot(insertedTangent1, insertedTangent2) < targetCos)
                {
                    p0 = p1ReplacementA;
                    p1 = p1ReplacementB;
                    depth -= 1;
                    continue;
                }

                vertCount += 2;
                indCount += 6;

                break;
            }
        }
    }
}
