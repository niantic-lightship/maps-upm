// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Structs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using IntReference = Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.UnsafeReference<int>;

namespace Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Jobs
{
    [BurstCompile]
    internal readonly struct AppraiseMeshJob : IJob
    {
        private readonly LinearFeatureSet _featureSet;
        private readonly IntReference _vertexCount;
        private readonly IntReference _indexCount;

        [ReadOnly] private readonly float _smoothFactor;
        [ReadOnly] private readonly int _endCapPointCount;
        [ReadOnly] private readonly float _bendThreshold;

        public void Execute()
        {
            AppraiseLinearFeature();
            LinearFeatureBuilderUtils.PrepareFeatureSet(_featureSet, _vertexCount, _indexCount);
        }

        public AppraiseMeshJob(
            LinearFeatureSet featureSet,
            IntReference vertexCount,
            IntReference indexCount,
            float smoothFactor,
            int endCapPointCount,
            float bendThreshold)
        {
            _featureSet = featureSet;
            _smoothFactor = smoothFactor;
            _endCapPointCount = endCapPointCount;
            _bendThreshold = bendThreshold;
            _vertexCount = vertexCount;
            _indexCount = indexCount;
        }

        private void AppraiseLinearFeature()
        {
            var linearFeature = _featureSet.LinearFeature;

            var offset = 0;
            foreach (var strip in linearFeature.LineStrips)
            {
                if (strip < 2)
                {
                    continue;
                }

                // Add vertex and index counts for end caps
                var featureSetNeededVerts = _featureSet.NeededVerts;
                featureSetNeededVerts.Value += _endCapPointCount * 2; //Two end caps
                var featureSetNeededIndices = _featureSet.NeededIndices;
                featureSetNeededIndices.Value += _endCapPointCount * 2 * 3; //3 indices per vert per end cap

                // Count smoothing verts and indices with mocked methods
                for (var i = 0; i < strip - 1; ++i)
                {
                    if (i == 0)
                    {
                        featureSetNeededVerts.Value += 2; //2 starting verts for first strip
                    }

                    if (i < strip - 2)
                    {
                        var point0 = linearFeature.Points[offset + i];
                        var point1 = linearFeature.Points[offset + i + 1];
                        var point2 = linearFeature.Points[offset + i + 2];

                        var tangent0 = math.normalize(point1 - point0);
                        var tangent1 = math.normalize(point2 - point1);

                        if (math.dot(tangent0, tangent1) < _bendThreshold)
                        {
                            CalculateSmoothingVerts(
                                linearFeature.Points[i + offset],
                                linearFeature.Points[i + 1 + offset],
                                linearFeature.Points[i + 2 + offset],
                                featureSetNeededVerts,
                                featureSetNeededIndices,
                                _bendThreshold,
                                3);
                            continue;
                        }
                    }

                    featureSetNeededVerts.Value += 2;
                    featureSetNeededIndices.Value += 6;
                }

                offset += strip;
            }
        }

        private void CalculateSmoothingVerts(
            float3 p0,
            float3 p1,
            float3 p2,
            IntReference vertCount,
            IntReference indCount,
            float targetCos,
            int depth)
        {
            // using http://graphics.cs.ucdavis.edu/education/CAGDNotes/Chaikins-Algorithm/Chaikins-Algorithm.html

            var p1ReplacementA = _smoothFactor * p0 + (1.0f - _smoothFactor) * p1;
            var p1ReplacementB = (1.0f - _smoothFactor) * p1 + _smoothFactor * p2;

            var insertedTangent0 = math.normalize(p1ReplacementA - p0);
            var insertedTangent1 = math.normalize(p1ReplacementB - p1ReplacementA);
            var insertedTangent2 = math.normalize(p2 - p1ReplacementB);

            if (depth > 0 && math.dot(insertedTangent0, insertedTangent1) < targetCos)
            {
                CalculateSmoothingVerts(p0, p1ReplacementA, p1ReplacementB, vertCount, indCount, targetCos, depth - 1);
            }
            else
            {
                vertCount.Value += 2;
                indCount.Value += 6;
            }

            if (depth > 0 && math.dot(insertedTangent1, insertedTangent2) < targetCos)
            {
                CalculateSmoothingVerts(p1ReplacementA, p1ReplacementB, p2, vertCount, indCount, targetCos, depth - 1);
            }
            else
            {
                vertCount.Value += 2;
                indCount.Value += 6;
            }
        }
    }
}
