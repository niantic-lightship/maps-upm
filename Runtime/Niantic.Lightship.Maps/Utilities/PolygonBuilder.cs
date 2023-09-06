// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.Lightship.Maps.Utilities
{
    /// <summary>
    /// Utility class for building polygon meshes.  Adapted from
    /// <a href="https://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml">
    /// here</a>
    /// </summary>
    internal static class PolygonBuilder
    {
        private const float Epsilon = 0.0000000001f;

        private static ChannelLogger Log { get; } = new(nameof(PolygonBuilder));

        /// <summary>
        /// Triangulates a polygon from a list of vertices
        /// </summary>
        /// <param name="polygon">The vertices that define
        /// this polygon's outer edges, in order</param>
        /// <returns>A list of triangles, as three-tuples of vertices</returns>
        public static List<Vector3> Triangulate(Vector3[] polygon)
        {
            var result = new List<Vector3>();
            int vertexCount = polygon.Length;

            if (vertexCount < 3)
            {
                Log.Error("There must be at least 3 vertices to build a polygon");
                return null;
            }

            var indices = new int[vertexCount];

            // If this polygon has a negative area, then our vertices
            // are wound clockwise, so their order should be reversed.

            if (0.0f < Area(polygon))
            {
                for (int v = 0; v < vertexCount; v++)
                {
                    indices[v] = v;
                }
            }
            else
            {
                for (int v = 0; v < vertexCount; v++)
                {
                    indices[v] = vertexCount - 1 - v;
                }
            }

            int nv = vertexCount;

            // Remove nv-2 Vertices, creating 1 triangle every time
            int count = 2 * nv; // error detection

            for (int v = nv - 1; nv > 2;)
            {
                // If we loop, it is probably a non-simple polygon
                if (0 >= count--)
                {
                    Log.Error("Invalid polygon!");
                    return null;
                }

                // Three consecutive vertices in current polygon, <u,v,w>
                int u = v;
                if (nv <= u)
                {
                    u = 0; // Previous
                }

                v = u + 1;
                if (nv <= v)
                {
                    v = 0; // New v
                }

                int w = v + 1;
                if (nv <= w)
                {
                    w = 0; // Next
                }

                if (Snip(polygon, u, v, w, nv, indices))
                {
                    int s;
                    int t;

                    // Get this triangle's vertex indices
                    int a = indices[u];
                    int b = indices[v];
                    int c = indices[w];

                    // Add the triangle to the result list
                    result.Add(polygon[a]);
                    result.Add(polygon[b]);
                    result.Add(polygon[c]);

                    // Remove v from remaining polygon
                    for (s = v, t = v + 1; t < nv; s++, t++)
                    {
                        indices[s] = indices[t];
                    }

                    nv--;

                    // Reset error detection counter
                    count = 2 * nv;
                }
            }

            return result;
        }

        private static float Area(IList<Vector3> polygon)
        {
            int n = polygon.Count;
            float area = 0.0f;

            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                area += polygon[p].x * polygon[q].z - polygon[q].x * polygon[p].z;
            }

            return area * 0.5f;
        }

        /// <summary>
        /// InsideTriangle decides if a point P is Inside of the triangle defined by A, B, C.
        /// </summary>
        private static bool IsPointInsideTriangle(
            float vertexAx,
            float vertexAy,
            float vertexBx,
            float vertexBy,
            float vertexCx,
            float vertexCy,
            float pointX,
            float pointY)
        {
            float ax = vertexCx - vertexBx;
            float ay = vertexCy - vertexBy;
            float bx = vertexAx - vertexCx;
            float by = vertexAy - vertexCy;
            float cx = vertexBx - vertexAx;
            float cy = vertexBy - vertexAy;
            float apx = pointX - vertexAx;
            float apy = pointY - vertexAy;
            float bpx = pointX - vertexBx;
            float bpy = pointY - vertexBy;
            float cpx = pointX - vertexCx;
            float cpy = pointY - vertexCy;

            float aCrossBp = ax * bpy - ay * bpx;
            float cCrossAp = cx * apy - cy * apx;
            float bCrossCp = bx * cpy - by * cpx;

            return aCrossBp >= 0.0f && bCrossCp >= 0.0f && cCrossAp >= 0.0f;
        }

        private static bool Snip(IReadOnlyList<Vector3> polygon, int u, int v, int w, int n, int[] indices)
        {
            int p;

            var a = polygon[indices[u]];
            var b = polygon[indices[v]];
            var c = polygon[indices[w]];

            float ax = a.x;
            float ay = a.z;

            float bx = b.x;
            float by = b.z;

            float cx = c.x;
            float cy = c.z;

            if (Epsilon > (bx - ax) * (cy - ay) - (by - ay) * (cx - ax))
            {
                return false;
            }

            for (p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w)
                {
                    continue;
                }

                var point = polygon[indices[p]];
                float px = point.x;
                float py = point.z;

                if (IsPointInsideTriangle(ax, ay, bx, by, cx, cy, px, py))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
