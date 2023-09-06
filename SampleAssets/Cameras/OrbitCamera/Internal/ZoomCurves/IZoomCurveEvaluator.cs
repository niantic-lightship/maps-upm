// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.ZoomCurves
{
    internal interface IZoomCurveEvaluator
    {
        float GetAngleFromDistance(float distanceMeters);
        float GetElevationFromDistance(float distance);
        float GetDistanceFromZoomFraction(float zoomFraction);
    }
}
