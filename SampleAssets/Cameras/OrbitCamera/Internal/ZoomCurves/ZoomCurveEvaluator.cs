// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.ZoomCurves
{
    internal class ZoomCurveEvaluator : IZoomCurveEvaluator
    {
        private readonly float _minZoomMeters;
        private readonly float _minPitchDegrees;
        private readonly float _verticalOffsetMeters;
        private readonly float _pitchRange;
        private readonly float _zoomRange;

        public ZoomCurveEvaluator(
            float minZoomMeters,
            float maxZoomMeters,
            float minPitchDegrees,
            float maxPitchDegrees,
            float verticalOffsetMeters)
        {
            _minZoomMeters = minZoomMeters;
            _minPitchDegrees = minPitchDegrees;
            _verticalOffsetMeters = verticalOffsetMeters;
            _pitchRange = maxPitchDegrees - _minPitchDegrees;
            _zoomRange = maxZoomMeters - _minZoomMeters;
        }

        /// <inheritdoc />
        public float GetAngleFromDistance(float distanceMeters)
        {
            float distanceFrac = (distanceMeters - _minZoomMeters) / _zoomRange;
            float distanceFracSquared = distanceFrac * distanceFrac;
            return _minPitchDegrees + distanceFracSquared * _pitchRange;
        }

        /// <inheritdoc />
        public float GetElevationFromDistance(float distanceMeters)
        {
            float angleDegrees = GetAngleFromDistance(distanceMeters);
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            return _verticalOffsetMeters + Mathf.Tan(angleRadians) * distanceMeters;
        }

        /// <inheritdoc />
        public float GetDistanceFromZoomFraction(float zoomFraction)
        {
            float zoomFractionReciprocal = zoomFraction / 1.0f;
            return _zoomRange * zoomFractionReciprocal + _minZoomMeters;
        }
    }
}
