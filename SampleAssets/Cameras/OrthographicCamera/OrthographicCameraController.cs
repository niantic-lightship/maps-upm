// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Platform.Debugging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityInput = UnityEngine.Input;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrthographicCamera
{
    /// <summary>
    /// A simple top-down camera controller. The camera supports panning with touch
    /// or mouse drags and zooming in and out using pinch gestures or the mouse wheel.
    /// </summary>
    public class OrthographicCameraController : MonoBehaviour
    {
        [SerializeField]
        private float _mouseScrollSpeed = 0.1f;

        [SerializeField]
        private float _pinchScrollSpeed = 0.002f;

        [SerializeField]
        private float _minimumMapRadius = 10.0f;

        [HideInInspector]
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private LightshipMapView _mapView;

        private bool _isPinchPhase;
        private bool _isPanPhase;
        private float _lastPinchDistance;
        private Vector3 _lastWorldPosition;
        private float _mapRadius;

        private void Start()
        {
            Assert.That(_camera.orthographic);
            Assert.That(_mapView.IsMapCenteredAtOrigin);
            _mapRadius = (float)_mapView.MapRadius;
            _camera.orthographicSize = _mapRadius;
        }

        private void Update()
        {
            // Mouse scroll wheel moved
            if (UnityInput.mouseScrollDelta.y != 0)
            {
                var mousePosition = new Vector2(UnityInput.mousePosition.x, UnityInput.mousePosition.y);

                // Don't zoom if the mouse pointer is over a UI object
                if (!PlatformAgnosticInput.IsOverUIObject(mousePosition))
                {
                    var sizeDelta = UnityInput.mouseScrollDelta.y * _mouseScrollSpeed * _mapRadius;
                    var newMapRadius = Math.Max(_mapRadius - sizeDelta, _minimumMapRadius);

                    _mapView.SetMapRadius(newMapRadius);
                    _camera.orthographicSize = newMapRadius;
                    _mapRadius = newMapRadius;
                }
            }

            // UI element was pressed, so ignore all touch input this frame
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return;
            }

            // Pinch logic
            if (UnityInput.touchCount == 2)
            {
                Vector2 touch0;
                Vector2 touch1;

                if (_isPinchPhase == false)
                {
                    // Pinch started so reset pan position
                    ResetPanTouch();

                    touch0 = UnityInput.GetTouch(0).position;
                    touch1 = UnityInput.GetTouch(1).position;
                    _lastPinchDistance = Vector2.Distance(touch0, touch1);

                    _isPinchPhase = true;
                }
                else
                {
                    touch0 = UnityInput.GetTouch(0).position;
                    touch1 = UnityInput.GetTouch(1).position;
                    float distance = Vector2.Distance(touch0, touch1);

                    var sizeDelta = (distance - _lastPinchDistance) * _pinchScrollSpeed * _mapRadius;
                    var newMapRadius = Math.Max(_mapRadius - sizeDelta, _minimumMapRadius);

                    _mapView.SetMapRadius(newMapRadius);
                    _camera.orthographicSize = newMapRadius;
                    _mapRadius = newMapRadius;

                    _lastPinchDistance = distance;
                }
            }
            // No pinch
            else
            {
                // Pinch so reset pan position
                if (_isPinchPhase && _isPanPhase && PlatformAgnosticInput.TouchCount == 1)
                {
                    ResetPanTouch();
                }

                _isPinchPhase = false;
            }

            // Pan camera by swiping
            if (PlatformAgnosticInput.TouchCount >= 1)
            {
                if (_isPanPhase == false)
                {
                    _isPanPhase = true;
                    ResetPanTouch();
                }
                else
                {
                    Vector3 currentInputPos = PlatformAgnosticInput.GetTouch(0).position;
                    currentInputPos.z = _camera.nearClipPlane;
                    var currentWorldPosition = _camera.ScreenToWorldPoint(currentInputPos);
                    currentWorldPosition.y = 0.0f;

                    var offset = currentWorldPosition - _lastWorldPosition;
                    _mapView.OffsetMapCenter(offset);
                    _lastWorldPosition = currentWorldPosition;
                }
            }
            else
            {
                _isPanPhase = false;
            }
        }

        private void ResetPanTouch()
        {
            Vector3 currentInputPos = PlatformAgnosticInput.GetTouch(0).position;
            var currentWorldPosition = _camera.ScreenToWorldPoint(currentInputPos);
            currentWorldPosition.y = 0.0f;

            _lastWorldPosition = currentWorldPosition;
        }
    }
}
