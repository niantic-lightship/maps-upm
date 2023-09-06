// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Attributes;
using Niantic.Lightship.Maps.Coordinates;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.Exceptions;
using Niantic.Lightship.Maps.Internal;
using Niantic.Lightship.Maps.Jobs;
using Niantic.Lightship.Maps.MapLayers;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Themes;
using Niantic.Lightship.Maps.Utilities;
using Niantic.Platform.Debugging;
using UnityEngine;

namespace Niantic.Lightship.Maps
{
    using MapTileObjectDictionary = Dictionary<ulong, PooledObject<MapTileObject>>;

    /// <summary>
    /// This class represents a single viewable area of the map, and is the
    /// primary API that games will use to interact with the Lightship Maps
    /// SDK.  Most scenes will have a single <see cref="LightshipMapView"/>
    /// instance, although the Maps SDK supports any number of active map
    /// views in a given scene at a time.
    /// </summary>
    [PublicAPI]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public partial class LightshipMapView :
        MonoBehaviour, ILightshipMapView, IBuilderJobMonitorManager
    {
        public const int DefaultExecutionOrder = -10;

        #region Serialized fields

        [SerializeField]
        private LightshipMapManager _lightshipMapManager;

        [Tooltip("If set, the map will always be positioned with its center at the scene's origin")]
        [SerializeField]
        private bool _centerMapAtOrigin;

        [Tooltip(
            "The maximum distance the map is allowed to move away from the origin (in " +
            "meters). If the map's center moves further than this limit, all maptiles " +
            "and objects placed on the map will be moved back to the scene's origin.")]
        [SerializeField]
        private float _maximumMapOffset = 1000.0f;

        [Tooltip("This field can be used to specify the maximum number of maptiles " +
            "that will be scheduled together in a batch.  In most cases, this value " +
            "should be kept at its default.  Setting this to a value below the default " +
            "may help mitigate the impact on performance that could occur when a large " +
            "number of maptiles are returned at the same time, if profiling showed " +
            "large spikes in frame time when this happens.")]
        [SerializeField]
        private int _tileBatchCount = 32;

        [Tooltip("The theme to use when rendering this map view.")]
        [SerializeField]
        private MapTheme _mapTheme;

        [Tooltip("The map's viewable radius, in scene units.  This field is optional if the " +
            "viewable area's radius is passed in along with its center location at runtime. ")]
        [SerializeField]
        private double _viewableRadius = 500f;

        [Tooltip("If set, the map view will be initialized at a default starting location.")]
        [SerializeField]
        private bool _startAtDefaultLocation;

        [Tooltip("The default starting location, if 'Start At Default Location' is set.")]
        [SerializeField]
        [DisabledIfFalse(nameof(_startAtDefaultLocation))]
        private SerializableLatLng _defaultLocation;

        [Tooltip("A list of prefabs or GameObjects containing MapLayer components " +
            "that are used while constructing the map view.  These are typically " +
            "used to place objects or to render geometry that may cross maptile " +
            "boundaries to cover larger sections of the map.  For more information " +
            "about the MapLayer system, please refer to the online documentation.")]
        [SerializeField]
        private List<MapLayer> _mapLayers = new();

        #endregion
        #region Private fields

        private readonly ConcurrentQueue<IMapTile> _mapTilesToAdd = new();
        private readonly ConcurrentQueue<IMapTile> _mapTilesToRemove = new();
        private static ConcurrentDictionary<(Guid, IMapTileObject), JobMonitor> _monitorsByBuilderAndTile = new();

        private readonly Dictionary<ulong, IMapTile> _activeMapTiles = new();
        private readonly MapTileObjectDictionary _activeMapTileObjects = new();
        private ObjectPool<MapTileObject> _mapTileObjectPool;
        private MapTileObject _mapTileObjectToClone;

        private Transform _inactiveTilesParent;
        private Transform _activeTilesParent;
        private Transform _mapLayersParent;

        private MapUnitConverter _unitConverter;
        private float _maximumMapOffsetSquared;
        private MapTheme _currentTheme;
        private IMapView _mapView;
        private Task _update;

        private static ChannelLogger Log { get; } = new(nameof(LightshipMapView));

        #endregion
        #region Public events and properties

        /// <inheritdoc />
        public event Action<IMapTile, IMapTileObject> MapTileAdded;

        /// <inheritdoc />
        public event Action<IMapTile, IMapTileObject> MapTileRemoved;

        /// <inheritdoc />
        public event Action<double> MapRadiusChanged;

        /// <inheritdoc />
        public event Action<LatLng> MapCenterChanged;

        /// <inheritdoc />
        public event Action<LatLng> MapOriginChanged;

        /// <inheritdoc />
        public LatLng MapOrigin => _unitConverter.MapOrigin;

        /// <inheritdoc />
        public LatLng MapCenter { get; private set; }

        /// <inheritdoc />
        public double MapRadius { get; private set; }

        /// <inheritdoc />
        public double MapScale => _unitConverter.MapScale;

        /// <inheritdoc />
        public IReadOnlyDictionary<ulong, IMapTile> ActiveMapTiles => _activeMapTiles;

        /// <inheritdoc />
        public bool IsMapCenteredAtOrigin => _centerMapAtOrigin;

        #endregion
        #region Methods called by Unity

        private void Awake()
        {
            if (!_lightshipMapManager.IsInitialized)
            {
                // Throw if our LightshipMapManager failed to initialize
                throw new LightshipMapManagerNotInitializedException();
            }

            // Clear any child objects
            DetachAndHideChildObjects();

            Assert.IsNull(_mapLayersParent);
            _mapLayersParent = CreateTransformGroup("MapLayers");
            _mapLayers.ForEach(layer => layer.Initialize(this, _mapLayersParent));

            Assert.IsNull(_activeTilesParent);
            _activeTilesParent = CreateTransformGroup("ActiveTiles");

            Assert.IsNull(_inactiveTilesParent);
            _inactiveTilesParent = CreateTransformGroup("InactiveTiles");
            _inactiveTilesParent.gameObject.SetActive(false);

            // Set our initial theme
            SetMapTheme(_mapTheme);

            // Create a MapTileObject instance that all
            // other MapTileObjects will be instantiated from.
            var objectToClone = new GameObject(nameof(MapTileObject));
            _mapTileObjectToClone = objectToClone.AddComponent<MapTileObject>();
            UnityObjectUtils.DisableAndHide(_mapTileObjectToClone.gameObject);

            // Initialize the ObjectPool used for MapTileObjects
            _mapTileObjectPool = new ObjectPool<MapTileObject>(
                _mapTileObjectToClone,
                onCreate: mto => UnityObjectUtils.EnableAndShow(mto.gameObject),
                onRelease: mto => mto.Release());

            Log.Info("Initializing map view");
            _mapView = _lightshipMapManager.CreateMapView();
            _mapView.MapTileAdded += OnMapTileAdded;
            _mapView.MapTileRemoved += OnMapTileRemoved;

            MapRadius = _viewableRadius;
            _maximumMapOffsetSquared = _maximumMapOffset * _maximumMapOffset;

            if (_startAtDefaultLocation)
            {
                // Initialize the unit converter and set our starting position
                _unitConverter = InitializeUnitConverter(_defaultLocation);
                SetViewableArea(_unitConverter.MapOrigin, _viewableRadius);
            }

            Transform CreateTransformGroup(string groupName)
            {
                var newGameObject = new GameObject(groupName);
                var newTransform = newGameObject.transform;
                newGameObject.layer = gameObject.layer;
                newTransform.SetParent(transform);
                return newTransform;
            }
        }

        private void OnDestroy()
        {
            if (_mapView != null)
            {
                _mapView.MapTileAdded -= OnMapTileAdded;
                _mapView.MapTileRemoved -= OnMapTileRemoved;
            }

            Destroy(_mapTileObjectToClone);
        }

        private void Update()
        {
            // First, release any removed maptiles
            while (_mapTilesToRemove.TryDequeue(out var tileToRemove))
            {
                ReleaseMapTile(tileToRemove);
            }

            // Next, build any added maptiles
            if (_mapTilesToAdd.IsEmpty)
            {
                return;
            }

            var tileBatch = new List<IMapTile>();

            while (tileBatch.Count < _tileBatchCount && _mapTilesToAdd.TryDequeue(out var mapTile))
            {
                if (!_mapView.ActiveMapTiles.ContainsKey(mapTile.Id))
                {
                    // Don't add a MapTile that doesn't exist in the current IMapView.
                    // This can happen if the MapTile went into and out of view quickly
                    // enough that it was already gone by the time we dequeued it here.
                    continue;
                }

                tileBatch.Add(mapTile);
            }

            AddMapTiles(tileBatch);
        }

        #endregion
        #region Methods used to set the map's viewable area

        /// <inheritdoc />
        public void OffsetMapCenter(Vector3 offset)
        {
            var latLng = _unitConverter.SceneToLatLng(-transform.position - offset);
            var radius = MapRadius;
            SetViewableAreaInternal(latLng, radius);
        }

        /// <inheritdoc />
        public void SetMapCenter(in LatLng latLng)
        {
            var radius = MapRadius;
            SetViewableAreaInternal(latLng, radius);
        }

        /// <inheritdoc />
        public void SetMapCenter(Vector3 center)
        {
            var latLng = _unitConverter.SceneToLatLng(center);
            var radius = MapRadius;
            SetViewableAreaInternal(latLng, radius);
        }

        /// <inheritdoc />
        public void SetMapRadius(double mapRadius)
        {
            var radius = _unitConverter.SceneToMeters(mapRadius, MapCenter.Latitude);
            SetViewableAreaInternal(MapCenter, radius);
        }

        /// <inheritdoc />
        public void SetViewableArea(in LatLng latLng, double mapRadius)
        {
            var radius = _unitConverter.SceneToMeters(mapRadius, latLng.Latitude);
            SetViewableAreaInternal(latLng, radius);
        }

        /// <inheritdoc />
        public void SetViewableArea(Vector3 center, double mapRadius)
        {
            var latLng = _unitConverter.SceneToLatLng(center);
            var radius = _unitConverter.SceneToMeters(mapRadius, latLng.Latitude);
            SetViewableAreaInternal(latLng, radius);
        }

        private void SetViewableAreaInternal(in LatLng latLng, double mapRadius)
        {
            _unitConverter ??= InitializeUnitConverter(latLng);

            bool mapCenterChanged = MapCenter != latLng;
            bool mapRadiusChanged = !MathEx.AlmostEqual(MapRadius, mapRadius);

            MapCenter = latLng;
            MapRadius = mapRadius;

            _mapView.SetViewableArea(MapCenter, MapRadius);

            if (mapCenterChanged)
            {
                // Raise an event if the map's center changed
                MapCenterChanged?.Invoke(MapCenter);
            }

            if (mapRadiusChanged)
            {
                // Raise an event if the map's radius changed
                MapRadiusChanged?.Invoke(MapRadius);
            }

            // Compute the world space position of the new map
            // center.  If it's further than our maximum allowed
            // offset, trigger a full map reposition.  This can
            // be expensive, so if the map is set to be centered
            // at the origin, just set our transform's position.

            var position = _unitConverter.LatLngToScene(MapCenter);
            var distanceSquared = position.MagnitudeSquared;

            if (distanceSquared > _maximumMapOffsetSquared)
            {
                Log.Info($"Repositioning the map ({distanceSquared} squared meters from the origin)");
                _unitConverter.MapOrigin = MapCenter;
            }
            else if (_centerMapAtOrigin)
            {
                transform.position = -(Vector3)position;
            }
        }

        private MapUnitConverter InitializeUnitConverter(in LatLng location)
        {
            var unitConverter = new MapUnitConverter(location);
            unitConverter.MapOriginChanged += OnMapOriginChanged;
            return unitConverter;
        }

        private void OnMapOriginChanged()
        {
            Log.Info($"Map origin changed: {_unitConverter.MapOrigin}, {_unitConverter.MapScale}");

            // Reset our transform's position
            transform.position = Vector3.zero;

            // Trigger an update of all mapTile origins.
            foreach (var mapTile in _activeMapTiles.Values)
            {
                var wm = new WebMercator12(mapTile.Origin);
                var pooledObject = _activeMapTileObjects[mapTile.Id];
                var mapTileObject = pooledObject.Value;
                mapTileObject.transform.localScale = (float)(MapScale * mapTile.Size) * Vector3.one;
                mapTileObject.transform.localPosition = (Vector3)_unitConverter.WebMercator12ToScene(wm);
            }

            // Reposition all of our MapLayers.  Each layer is responsible
            // for repositioning all of the objects it owns as necessary.
            _mapLayers.ForEach(mapLayer => mapLayer.OnMapOriginChanged());

            // Raise an event so that objects placed on the map that aren't
            // managed by any of our MapLayers can be repositioned.
            MapOriginChanged?.Invoke(MapCenter);
        }

        #endregion
        #region Methods to convert between scene and LatLng coordinates

        /// <inheritdoc />
        public LatLng SceneToLatLng(Vector3 scenePosition)
        {
            var position = scenePosition + transform.position;
            return _unitConverter.SceneToLatLng(position);
        }

        /// <inheritdoc />
        public Vector3 LatLngToScene(in LatLng location)
        {
            return (Vector3)_unitConverter.LatLngToScene(location) + transform.position;
        }

        /// <inheritdoc />
        public Vector3 WebMercator12ToScene(in WebMercator12 webMercator12)
        {
            return (Vector3)_unitConverter.WebMercator12ToScene(webMercator12) + transform.position;
        }

        /// <inheritdoc />
        public double MetersToScene(double meters, double latitude)
        {
            return _unitConverter.MetersToScene(meters, latitude);
        }

        /// <inheritdoc />
        public double SceneToMeters(double sceneUnits, double latitude)
        {
            return _unitConverter.SceneToMeters(sceneUnits, latitude);
        }

        #endregion
        #region Methods used to update the map's theme

        /// <inheritdoc />
        public void SetMapTheme(MapTheme theme)
        {
            if (_currentTheme != null)
            {
                // Destroy the previous theme, if it exists
                Destroy(_currentTheme.gameObject);
            }

            // Instantiate the theme and add it to the hierarchy
            _currentTheme = Instantiate(theme, transform);
            _currentTheme.transform.SetAsFirstSibling();
            _currentTheme.name = "MapTheme";

            // Initializes the new theme's builders and sets the skybox
            _currentTheme.InitializeTheme(this);

            // Perform a full map reload
            ReloadMap();
        }

        #endregion
        #region Methods handling MapTile lifecycles

        /// <summary>
        /// Queues a map tile to be added to our scene.
        /// </summary>
        /// <param name="mapTile"></param>
        private void OnMapTileAdded(IMapTile mapTile)
        {
            _mapTilesToAdd.Enqueue(mapTile);
        }

        /// <summary>
        /// Queues a map tile to be removed from our scene.
        /// </summary>
        /// <param name="mapTile"></param>
        private void OnMapTileRemoved(IMapTile mapTile)
        {
            _mapTilesToRemove.Enqueue(mapTile);
        }

        /// <summary>
        /// Creates or reuses existing game objects for
        /// the mapTiles and invokes the individual builders.
        /// </summary>
        private void AddMapTiles(IReadOnlyList<IMapTile> mapTiles)
        {
            var tileCount = mapTiles.Count;
            var tilePairs = new TilePair[tileCount];

            for (var i = 0; i < tileCount; i++)
            {
                var mapTile = mapTiles[i];
                var (added, mapTileObject) = AddMapTileHelper(mapTile);
                if (!added)
                {
                    // Already have a tile for this id, don't rewrite / overwrite.
                    continue;
                }

                tilePairs[i] = new TilePair(mapTileObject, mapTile, tile =>
                {
                    tile.MapTileObject.SetLayerOnAllChildren(gameObject.layer);
                    MapTileAdded?.Invoke(tile.Tile, tile.MapTileObject);
                });
            }

            MapTileObject.Build(tilePairs, this);
        }

        /// <summary>
        /// Helper method which assigns a <see cref="MapTileObject"/> to a queued <see cref="IMapTile"/>.
        /// </summary>
        /// <param name="mapTile">The current <see cref="IMapTile"/></param>
        /// <returns>Whether or not the <see cref="IMapTile"/> is already active and, if not,
        /// also a <see cref="MapTileObject"/> from a <see cref="ObjectPool{T}"/></returns>
        private (bool, MapTileObject) AddMapTileHelper(IMapTile mapTile)
        {
            if (_activeMapTiles.ContainsKey(mapTile.Id))
            {
                // Already have a tile for this id, don't rewrite / overwrite.
                return (false, null);
            }

            var pooledMapTileObject = _mapTileObjectPool.GetOrCreate();
            var mapTileObject = pooledMapTileObject.Value;
            mapTileObject.AddToScene(mapTile, this, _activeTilesParent.transform, _currentTheme);

            _activeMapTiles[mapTile.Id] = mapTile;
            _activeMapTileObjects[mapTile.Id] = pooledMapTileObject;

            return (true, mapTileObject);
        }

        /// <summary>
        /// Returns a mapTile to the pool.
        /// </summary>
        private void ReleaseMapTile(IMapTile mapTile)
        {
            if (!_activeMapTiles.ContainsKey(mapTile.Id))
            {
                // Already deleted
                return;
            }

            var mapTileObject = _activeMapTileObjects[mapTile.Id];
            MapTileRemoved?.Invoke(mapTile, mapTileObject.Value);

            _activeMapTiles.Remove(mapTile.Id);
            _activeMapTileObjects.Remove(mapTile.Id);

            mapTileObject.Value.transform.SetParent(_inactiveTilesParent);
            mapTileObject.Dispose();
        }

        private void ReloadMap()
        {
            // Release all active MapTileObjects
            foreach (var maptile in _activeMapTileObjects)
            {
                var mapTileObject = maptile.Value;
                mapTileObject.Value.transform.SetParent(_inactiveTilesParent);
                mapTileObject.Dispose();
            }

            _activeMapTileObjects.Clear();

            // Move all of our maptiles into a temporary list
            var mapTiles = new List<IMapTile>(_activeMapTiles.Values);
            _activeMapTiles.Clear();

            // Re-add all maptiles to the queue, which simulates a full reload
            mapTiles.ForEach(mapTile => _mapTilesToAdd.Enqueue(mapTile));
        }

        #endregion
        #region IBuilderJobMonitorManager Methods

        /// <inheritdoc />
        bool IBuilderJobMonitorManager.TryRegisterMonitor(
            Guid builderId, IMapTileObject tile, out JobMonitor monitor)
        {
            if (_monitorsByBuilderAndTile.TryGetValue((builderId, tile), out monitor))
            {
                return false;
            }

            monitor = gameObject.AddComponent<JobMonitor>();
            monitor.hideFlags = HideFlags.HideInInspector;
            return _monitorsByBuilderAndTile.TryAdd((builderId, tile), monitor);
        }

        /// <inheritdoc />
        bool IBuilderJobMonitorManager.TryUnregisterMonitor(
            Guid builderId, IMapTileObject tile, bool destroyOnUnregister)
        {
            bool found = _monitorsByBuilderAndTile.TryRemove((builderId, tile), out var monitor);

            if (found && destroyOnUnregister)
            {
                Destroy(monitor);
            }

            return found;
        }

        /// <inheritdoc />
        bool IBuilderJobMonitorManager.TryGetMonitor(
            Guid builderId, IMapTileObject tile, out JobMonitor monitor)
        {
            return _monitorsByBuilderAndTile.TryGetValue((builderId, tile), out monitor);
        }

        #endregion

        /// <summary>
        /// If the <see cref="LightshipMapView"/> instance has any child objects
        /// attached to it, this method is called at startup to detach them
        /// and hide them from the hierarchy.  It can be useful for organizational
        /// purposes to attach objects or prefabs (like <see cref="MapLayer"/>s)
        /// to the <see cref="LightshipMapView"/> that references them, but they
        /// shouldn't inherit the map's or any of its parents' transforms.
        /// </summary>
        private void DetachAndHideChildObjects()
        {
            while (gameObject.transform.childCount > 0)
            {
                var child = gameObject.transform.GetChild(0).gameObject;
                Log.Info($"Detaching child object '{child.name}'");
                UnityObjectUtils.DisableAndHide(child);
            }
        }

        /// <inheritdoc />
        public void RefreshMap()
        {
            var currCenter = MapCenter;
            var currRadius = MapRadius;
            ReloadMap();

            _mapView = _lightshipMapManager.CreateMapView();
            _mapView.MapTileAdded += OnMapTileAdded;
            _mapView.MapTileRemoved += OnMapTileRemoved;
            _mapView.SetViewableArea(currCenter, currRadius);
        }
    }
}
