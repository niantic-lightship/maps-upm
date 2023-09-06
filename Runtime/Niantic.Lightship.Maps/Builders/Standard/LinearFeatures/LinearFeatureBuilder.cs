// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;
using UnityEngine.Assertions;

namespace Niantic.Lightship.Maps.Builders.Standard.LinearFeatures
{
    /// <summary>
    /// A builder for <see cref="ILinearFeature"/>s
    /// </summary>
    [PublicAPI]
    public partial class LinearFeatureBuilder : MeshBuilderStandard
    {
        // Knobs to adjust smooth linear feature building
        [Header("Smooth Linear Feature Knobs")]
        [Tooltip("Linear feature end cap vertex count.")]
        [Range(1, 8)]
        [SerializeField]
        private int _endCapPointCount = 4;

        // 1 is straight line, 0.85 = 31.8 degree deviation ... if bigger then insert smoothing points
        [Tooltip("Linear Feature - insert extra points when bend is more than cos(x degree)")]
        [Range(0.7f, 0.9f)]
        [SerializeField]
        private float _bendThreshold = 0.85f;

        // Sharpness of inserted curve (0.25 - fairly smooth, 0.15 - sharper turns) see algorithm below
        [Tooltip("Linear feature smoothness factor. (0.1 - sharp -> 0.25 smooth turn)")]
        [Range(0.1f, 0.25f)]
        [SerializeField]
        private float _smoothFactor = 0.15f;

        private readonly List<LinearFeatureSet> _allFeatures = new();

        #region Linear feature size values

        [SerializeField]
        [HideInInspector]
        private LinearFeatureSize _linearFeatureSize;

        [SerializeField]
        [HideInInspector]
        private float _customLinearFeatureMin;

        [SerializeField]
        [HideInInspector]
        private float _customLinearFeatureMax;

        private float _linearFeatureWidthMin;
        private float _linearFeatureWidthMax;
        private float _linearFeatureWidth;

        private double _linearFeatureWidthBaseSize;

        #endregion

        private static ChannelLogger Log { get; } = new(nameof(LinearFeatureBuilder));

        /// <inheritdoc />
        public override void Build(IMapTile mapTile, MeshFilter meshFilter)
        {
            SetFeaturesFromTile(mapTile);
            CalculateLinearFeatureWidth(mapTile.ZoomLevel, mapTile.Size);
            Parallel.ForEach(_allFeatures, AppraiseLinearFeature);

            var currVertIndex = 0;
            var currIndexIndex = 0;

            foreach (var featureSet in _allFeatures)
            {
                featureSet.VertStartIndex = currVertIndex;
                featureSet.IndicesStartIndex = currIndexIndex;
                currVertIndex += featureSet.NeededVerts;
                currIndexIndex += featureSet.NeededIndices;
            }

            var featureMeshData = new LinearFeatureMeshData(currVertIndex, currIndexIndex);

            Parallel.ForEach(_allFeatures, featureSet => ProcessLinearFeature(featureSet, featureMeshData));

            var mesh = MeshBuilderUtils.BuildSplitStreamMesh(
                "LinearFeatureMain",
                featureMeshData.Vertices,
                featureMeshData.Indices);

            SetMeshForFilter(mesh, meshFilter);
        }

        /// <inheritdoc />
        public override void Initialize(ILightshipMapView lightshipMapView)
        {
            base.Initialize(lightshipMapView);
            _linearFeatureWidthBaseSize = Math.Pow(2, WebMercator12.ZOOM_LEVEL - MaxLOD);
        }

        private void SetFeaturesFromTile(IMapTile tile)
        {
            _allFeatures.Clear();

            foreach (var featureKind in Features)
            {
                foreach (var feature in tile.GetTileData(Layer, featureKind))
                {
                    if (feature is ILinearFeature linearFeature)
                    {
                        _allFeatures.Add(new LinearFeatureSet(linearFeature));
                    }
                }
            }
        }

        private void ProcessLinearFeature(LinearFeatureSet featureSet, LinearFeatureMeshData linearFeatureMeshDatum)
        {
            var pointOffset = 0;
            var lastEndVerticesIndex = featureSet.VertStartIndex;
            var lastEndIndicesIndex = featureSet.IndicesStartIndex;

            foreach (var strip in featureSet.LinearFeature.LineStrips)
            {
                var startVerticesIndex = lastEndVerticesIndex;
                var startIndicesIndex = lastEndIndicesIndex;

                LinearFeatureBuilderUtils.CreateMiteredWidePolyline(
                    linearFeatureMeshDatum.Vertices,
                    linearFeatureMeshDatum.Indices,
                    startVerticesIndex,
                    startIndicesIndex,
                    featureSet.LinearFeature.Points,
                    pointOffset,
                    strip,
                    _linearFeatureWidth,
                    out lastEndVerticesIndex,
                    out lastEndIndicesIndex,
                    _endCapPointCount,
                    _bendThreshold,
                    _smoothFactor);

                pointOffset += strip;
            }

            Assert.AreEqual(lastEndVerticesIndex - featureSet.VertStartIndex, featureSet.NeededVerts);
            Assert.AreEqual(lastEndIndicesIndex - featureSet.IndicesStartIndex, featureSet.NeededIndices);
        }

        private void AppraiseLinearFeature(LinearFeatureSet featureSet)
        {
            var offset = 0;
            foreach (var strip in featureSet.LinearFeature.LineStrips)
            {
                if (strip < 2)
                {
                    continue;
                }

                // Add vertex and index counts for end caps
                featureSet.NeededVerts += _endCapPointCount * 2; // Two end caps
                featureSet.NeededIndices += _endCapPointCount * 2 * 3; // Three indices per vert per end cap

                // Count smoothing verts and indices with mocked methods
                for (var i = 0; i < strip - 1; ++i)
                {
                    if (i == 0)
                    {
                        // Two starting vertices for first strip
                        featureSet.NeededVerts += 2;
                    }

                    if (i < strip - 2)
                    {
                        var point0 = featureSet.LinearFeature.Points[offset + i];
                        var point1 = featureSet.LinearFeature.Points[offset + i + 1];
                        var point2 = featureSet.LinearFeature.Points[offset + i + 2];

                        var tangent0 = (point1 - point0).normalized;
                        var tangent1 = (point2 - point1).normalized;

                        if (Vector3.Dot(tangent0, tangent1) < _bendThreshold)
                        {
                            LinearFeatureBuilderUtils.CalculateSmoothingVerts(
                                point0,
                                point1,
                                point2,
                                ref featureSet.NeededVerts,
                                ref featureSet.NeededIndices,
                                _bendThreshold,
                                3,
                                _smoothFactor);
                            continue;
                        }
                    }

                    featureSet.NeededVerts += 2;
                    featureSet.NeededIndices += 6;
                }

                offset += strip;
            }
        }

        private float CalculateLinearFeatureWidth(int zoomLevel, double size)
        {
            UpdateLinearFeatureSizes(_linearFeatureSize);

            var linearFeatureWidth = (float)(_linearFeatureWidthMax * _linearFeatureWidthBaseSize / size);
            linearFeatureWidth = (float)MathEx.Clamp(linearFeatureWidth, _linearFeatureWidthMin, _linearFeatureWidthMax);

            return linearFeatureWidth;
        }

        private void UpdateLinearFeatureSizes(LinearFeatureSize linearFeatureSize)
        {
            switch (linearFeatureSize)
            {
                case LinearFeatureSize.Small:
                    _linearFeatureWidthMin = LinearFeatureSizeSettings.SmallLinearFeatureMin;
                    _linearFeatureWidthMax = LinearFeatureSizeSettings.SmallLinearFeatureMax;
                    break;

                case LinearFeatureSize.Medium:
                    _linearFeatureWidthMin = LinearFeatureSizeSettings.MedLinearFeatureMin;
                    _linearFeatureWidthMax = LinearFeatureSizeSettings.MedLinearFeatureMax;
                    break;

                case LinearFeatureSize.Large:
                    _linearFeatureWidthMin = LinearFeatureSizeSettings.LargeLinearFeatureMin;
                    _linearFeatureWidthMax = LinearFeatureSizeSettings.LargeLinearFeatureMax;
                    break;

                case LinearFeatureSize.Custom:
                    _linearFeatureWidthMin = _customLinearFeatureMin;
                    _linearFeatureWidthMax = _customLinearFeatureMax;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(linearFeatureSize), linearFeatureSize, null);
            }
        }
    }
}
