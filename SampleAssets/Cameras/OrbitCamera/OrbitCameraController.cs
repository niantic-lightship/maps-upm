// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Gestures;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.ZoomCurves;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera
{
    /// <summary>
    /// Camera controller for Niantic-standard map camera
    /// interactions, similar to the Pokemon GO camera.
    /// </summary>
    public class OrbitCameraController : MonoBehaviour
    {
        [SerializeField]
        private float _minimumZoomDistance = 23f;

        [SerializeField]
        private float _maximumZoomDistance = 99f;

        [SerializeField]
        private float _minimumPitchDegrees = 20.0f;

        [SerializeField]
        private float _maximumPitchDegrees = 60.0f;

        [SerializeField]
        private float _verticalFocusOffset = 10.0f;

        [SerializeField]
        private GestureSettings _gestureSettings;

        [HideInInspector]
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private GameObject _focusObject;

        private InputService _inputService;
        private CameraGestureTracker _gestureTracker;
        private IZoomCurveEvaluator _zoomCurveEvaluator;

        public void Awake()
        {
            _gestureTracker = new CameraGestureTracker(_camera, _focusObject, _gestureSettings);
            _inputService = new InputService(_gestureTracker);

            _zoomCurveEvaluator = new ZoomCurveEvaluator(
                _minimumZoomDistance,
                _maximumZoomDistance,
                _minimumPitchDegrees,
                _maximumPitchDegrees,
                _verticalFocusOffset);
        }

        public void Update()
        {
            _inputService.Update();
        }

        // Late update to ensure we use the latest avatar position
        private void LateUpdate()
        {
            float rotationAngleDegrees = _gestureTracker.RotationAngleDegrees;
            float rotationAngleRadians = Mathf.Deg2Rad * rotationAngleDegrees;
            float zoomFraction = _gestureTracker.ZoomFraction;

            float distance = _zoomCurveEvaluator.GetDistanceFromZoomFraction(zoomFraction);
            float elevMeters = _zoomCurveEvaluator.GetElevationFromDistance(distance);
            float pitchDegrees = _zoomCurveEvaluator.GetAngleFromDistance(distance);

            // Position the camera above the x-z plane,
            // according to our pitch and distance constraints.
            float x = -distance * Mathf.Sin(rotationAngleRadians);
            float z = -distance * Mathf.Cos(rotationAngleRadians);
            var offsetPos = new Vector3(x, elevMeters, z);

            _camera.transform.position = _focusObject.transform.position + offsetPos;
            _camera.transform.rotation = Quaternion.Euler(pitchDegrees, rotationAngleDegrees, 0.0f);
        }
    }
}
