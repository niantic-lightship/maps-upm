// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal
{
    [Serializable]
    internal class GestureSettings
    {
        [SerializeField]
        private float _defaultZoom = 0.75f;

        [SerializeField]
        private float _mouseScrollZoomSpeed = 1.5f;

        [SerializeField]
        private float _touchPinchZoomSpeed = 2.0f;

        [SerializeField]
        private bool _doubleTapZoomEnabled;

        [SerializeField]
        private float _doubleTapZoomSpeed = 6.0f;

        [SerializeField]
        private float _doubleTapMaxTime = 0.5f;

        public float DefaultZoom => _defaultZoom;
        public float MouseScrollZoomSpeed => _mouseScrollZoomSpeed;
        public float TouchPinchZoomSpeed => _touchPinchZoomSpeed;
        public bool DoubleTapZoomEnabled => _doubleTapZoomEnabled;
        public float DoubleTapZoomSpeed => _doubleTapZoomSpeed;
        public float DoubleTapMaxTime => _doubleTapMaxTime;
    }
}
