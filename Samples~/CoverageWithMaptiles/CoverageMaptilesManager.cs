// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Niantic.ARDK;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.VirtualStudio.VpsCoverage;
using Niantic.ARDK.VPSCoverage;
using Niantic.Lightship.Maps.MapLayers.Components;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Samples.CoverageWithMaptiles.ExtensionMethods;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.Maps.Samples.CoverageWithMaptiles
{
    internal class CoverageMaptilesManager : MonoBehaviour
    {
        [SerializeField]
        private RuntimeEnvironment _environment = RuntimeEnvironment.Default;

        [SerializeField]
        private VpsCoverageResponses _mockResponses;

        [SerializeField]
        private Transform _infoPanel;

        [SerializeField]
        private Location _location;

        [SerializeField]
        private int _queryRadius = 200;

        [SerializeField]
        private LightshipMapView _mapView;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private LayerPolygonRenderer _areaPolygonRenderer;

        [SerializeField]
        private LayerLineRenderer _areaOutlineRenderer;

        [SerializeField]
        private MarkerLayer _markerSpawner;

        private readonly List<PooledObject<GameObject>> _pooledAreaObjects = new();
        private readonly List<PooledObject<TargetMarker>> _pooledMarkerObjects = new();

        private ICoverageClient _coverageClient;
        private TargetMarker _selectedMarker;

        private static ChannelLogger Log { get; } = new("VpsCoverage");

        private void Start()
        {
            _coverageClient = CoverageClientFactory.Create(_environment, _mockResponses);
        }

        private async Task ProcessAreasResultAsync(CoverageAreasResult areasResult)
        {
            if (areasResult.Status != ResponseStatus.Success)
            {
                Log.Warning($"CoverageAreas request failed with status: {areasResult.Status}");
                return;
            }

            // Release all pooled area and marker objects
            _pooledAreaObjects.ForEach(p => p.Dispose());
            _pooledMarkerObjects.ForEach(p => p.Dispose());
            _pooledAreaObjects.Clear();
            _pooledMarkerObjects.Clear();

            AddAreasToMap(areasResult.Areas);

            var targetIds = new List<string>();
            foreach (var area in areasResult.Areas)
            {
                foreach (var targetIdentifier in area.LocalizationTargetIdentifiers)
                {
                    targetIds.Add(targetIdentifier);
                }
            }

            var targetsResult = await _coverageClient.RequestLocalizationTargetsAsync(targetIds.ToArray());
            ProcessTargetsResult(targetsResult);
        }

        private void ProcessTargetsResult(LocalizationTargetsResult targetsResult)
        {
            if (targetsResult.Status != ResponseStatus.Success)
            {
                Log.Warning($"LocalizationTarget request failed with status: {targetsResult.Status}");
                return;
            }

            AddTargetsToMap(targetsResult.ActivationTargets);
        }

        public void RequestAreaAroundMapCenter()
        {
            _ = RequestAreaAroundMapCenterAsync()
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            Log.Error($"Exception thrown by {nameof(RequestAreaAroundMapCenter)}: {t.Exception}");
                        }
                    },
                    TaskScheduler.Default
                );
        }

        private async Task RequestAreaAroundMapCenterAsync()
        {
            var mapCenter = _mapView.MapCenter.ToArdkLatLng();
            var areasResult = await _coverageClient.RequestCoverageAreasAsync(mapCenter, _queryRadius);
            await ProcessAreasResultAsync(areasResult);
        }

        private void DisplayTargetDetails(LocalizationTarget target)
        {
            var image = _infoPanel.Find("Image");
            target.DownloadImage(
                (int)(_camera.pixelWidth * 0.4f),
                (int)(_camera.pixelHeight * 0.15f),
                downLoadedImage => image.GetComponent<RawImage>().texture = downLoadedImage
            );

            var title = _infoPanel.Find("Info/Title");
            title.GetComponent<Text>().text = target.Name;

            var distance = _infoPanel.Find("Info/Distance");
            double distanceInM = target.Center.Distance(_location.GetLastLocation().ToArdkLatLng());
            distance.GetComponent<Text>().text = "Distance: " + distanceInM.ToString("N0") + " m";
        }

        private void AddAreasToMap(CoverageArea[] areas)
        {
            foreach (var area in areas)
            {
                var shapeLatLng = area.Shape.ToMapsLatLng();
                var areaName = $"Area_{area.LocalizationTargetIdentifiers[0]}";

                _pooledAreaObjects.Add(_areaPolygonRenderer.DrawPolygon(shapeLatLng, areaName));
                _pooledAreaObjects.Add(_areaOutlineRenderer.DrawLoop(shapeLatLng, areaName));
            }
        }

        private void AddTargetsToMap(IReadOnlyDictionary<string, LocalizationTarget> targets)
        {
            foreach (var target in targets)
            {
                var targetLatLng = target.Value.Center.ToMapsLatLng();
                var markerName = $"Target_{target.Key}";

                var marker = _markerSpawner.PlaceInstance(targetLatLng, markerName);
                marker.Value.Initialize(target.Value);
                _pooledMarkerObjects.Add(marker);
            }
        }

        private void Update()
        {
            if (PlatformAgnosticInput.touchCount <= 0)
            {
                return;
            }

            var touch = PlatformAgnosticInput.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                var touchRay = _camera.ScreenPointToRay(touch.position);

                // Raycast into the scene and see if we hit a map feature
                if (!Physics.Raycast(touchRay, out var hitInfo))
                {
                    return;
                }

                var hitMarker = hitInfo.collider.GetComponent<TargetMarker>();

                if (hitMarker != null)
                {
                    hitMarker.SetIsSelected(true);
                    DisplayTargetDetails(hitMarker.Target);

                    if (_selectedMarker != null)
                    {
                        _selectedMarker.SetIsSelected(false);
                    }

                    _selectedMarker = hitMarker;
                }
            }
        }
    }
}
