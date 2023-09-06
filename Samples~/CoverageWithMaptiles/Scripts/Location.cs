// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if !UNITY_EDITOR
#define NOT_UNITY_EDITOR
#endif

using System;
using System.Diagnostics;
using System.Linq;
using Niantic.Lightship.Maps.Core.Coordinates;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.CoverageWithMaptiles
{
    internal class Location : MonoBehaviour
    {
        [SerializeField]
        private Transform _gpsMarker;

        [SerializeField]
        private float _gpsMarkerHeight;

        [SerializeField]
        private LightshipMapView _mapView;

        private readonly LatLng _spoofLocation = new(37.796263, -122.39396);

        private float[] _orientationSmoothing;
        private int _orientationSmoothingIndex;

        private bool _active;
        private LocationInfo _lastLocation;
        private LatLng _lastPosition;
        private float _lastCompassHeading;

        // Start is called before the first frame update
        private void Start()
        {
            Input.location.Start(10f, 1);
            Input.compass.enabled = true;

            _mapView.MapOriginChanged += OnMapViewOriginChanged;
            _gpsMarker.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            if (!Input.location.lastData.Equals(_lastLocation))
            {
                OnLocationUpdated();
            }

            if (!Input.compass.trueHeading.Equals(_lastCompassHeading))
            {
                OnCompassUpdated();
            }
        }

        private void OnMapViewOriginChanged(LatLng origin)
        {
            SetGpsMarkerPosition(_lastPosition);
        }

        public void LocateDevice()
        {
            SetLastPositionInEditor();
            SetLastPositionInGame();

            _gpsMarker.gameObject.SetActive(true);
            _mapView.SetMapCenter(_lastPosition);
            SetGpsMarkerPosition(_lastPosition);
        }

        [Conditional("UNITY_EDITOR")]
        private void SetLastPositionInEditor()
        {
            _lastPosition = _spoofLocation;
        }

        [Conditional("NOT_UNITY_EDITOR")]
        private void SetLastPositionInGame()
        {
            _lastPosition = new LatLng(Input.location.lastData.latitude, Input.location.lastData.longitude);
            _active = true;
        }

        private void OnCompassUpdated()
        {
            _lastCompassHeading = Input.compass.trueHeading;

            // init
            if (_orientationSmoothing == null)
            {
                _orientationSmoothing = Enumerable.Repeat(_lastCompassHeading, 50).ToArray();
                _gpsMarker.transform.rotation = Quaternion.Euler(0, _lastCompassHeading, 0);
                return;
            }

            // add new value
            _orientationSmoothing[_orientationSmoothingIndex] = _lastCompassHeading;
            _orientationSmoothingIndex = (_orientationSmoothingIndex + 1) % _orientationSmoothing.Length;

            // average over sliding window
            float sum = 0;
            foreach (var orientation in _orientationSmoothing)
            {
                sum += orientation;
            }

            // set orientation
            _gpsMarker.transform.rotation = Quaternion.Euler(0, sum / _orientationSmoothing.Length, 0);
        }

        private void OnLocationUpdated()
        {
            _lastLocation = Input.location.lastData;
            SetGpsMarkerPosition(new LatLng(_lastLocation.latitude, _lastLocation.longitude));
        }

        public LatLng GetLastLocation()
        {
            return new LatLng(_lastLocation.latitude, _lastLocation.longitude);
        }

        private void SetGpsMarkerPosition(in LatLng latLng)
        {
            _gpsMarker.transform.position = _mapView.LatLngToScene(latLng) + _gpsMarkerHeight * Vector3.up;
        }
    }
}
