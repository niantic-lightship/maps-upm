// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.Core.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.Internal
{
    /// <summary>
    /// Provides the ability to easily convert between different coordinate systems
    /// such as scene world positions, latitude/longitude, web mercator, and meters.
    /// </summary>
    internal class MapUnitConverter
    {
        public event Action MapOriginChanged = delegate { };

        private readonly double _mapScaleMetersMultiplier;
        private WebMercator12 _mapOriginWm;
        private LatLng _mapOrigin;

        /// <param name="mapOrigin">
        /// The latitude/longitude that we should use for the origin (0, 0, 0) in our scene.
        /// Note that you can change this at any time by setting it via the MapOrigin property
        /// However, doing this could be an expensive operation depending on the observers of
        /// the MapOriginChanged event.  See external documentation for details.
        /// </param>
        /// <param name="mapScaleMetersMultiplier">
        /// Default: 1.0
        /// This value will determine the scale of the scene as a multiple of meters.
        /// 1 meter = 1 unit is the Unity standard, so in most cases you can leave this parameter
        /// unspecified, however you might want to change it depending on the game you are making
        /// </param>
        public MapUnitConverter(in LatLng mapOrigin, double mapScaleMetersMultiplier = 1.0d)
        {
            _mapScaleMetersMultiplier = mapScaleMetersMultiplier;
            SetMapOrigin(mapOrigin);
        }

        public double MapScale { get; private set; }

        public LatLng MapOrigin
        {
            get { return _mapOrigin; }
            set
            {
                bool almostEqual =
                    MathEx.AlmostEqual(_mapOrigin.Latitude, value.Latitude)
                    || MathEx.AlmostEqual(_mapOrigin.Longitude, value.Longitude);

                if (!almostEqual)
                {
                    SetMapOrigin(value);
                    MapOriginChanged();
                }
            }
        }

        private void SetMapOrigin(in LatLng mapOrigin)
        {
            MapScale = _mapScaleMetersMultiplier * WebMercator12.GetMetersPerUnit(mapOrigin.Latitude);
            _mapOrigin = mapOrigin;
            _mapOriginWm = mapOrigin.ToWebMercator12();
        }

        public Vector3D WebMercator12ToScene(in WebMercator12 wmPosition)
        {
            var displacement = wmPosition.ToVector3D() - _mapOriginWm.ToVector3D();
            return displacement * MapScale;
        }

        public WebMercator12 SceneToWebMercator12(in Vector3 scenePos)
        {
            var north = WebMercator12.ClampLatitudeCoord(scenePos.z / MapScale + _mapOriginWm.North);
            var east = WebMercator12.WrapLongitudeCoord(scenePos.x / MapScale + _mapOriginWm.East);
            var elevation = scenePos.y / MapScale + _mapOriginWm.Elevation;

            return new WebMercator12(east, elevation, north);
        }

        public Vector3D LatLngToScene(in LatLng latLng)
        {
            return WebMercator12ToScene(latLng.ToWebMercator12());
        }

        public LatLng SceneToLatLng(in Vector3 position)
        {
            return SceneToWebMercator12(position).ToLatLng();
        }

        public double SceneToWebMercator12(double sceneUnits)
        {
            return sceneUnits / MapScale;
        }

        /// <summary>
        /// Convert a distance in meters in the real world to Unity scene units.
        /// Note that latitude is required here because our map uses Web Mercator
        /// projection, which causes distances to stretch as you approach the poles.
        /// </summary>
        /// <param name="meters">The distance, in meters, to convert.</param>
        /// <param name="latitude">The latitude at the location at which the conversion applies.
        /// A reasonable value for this would be the latitude at the map origin.</param>
        /// <returns>The distance in Unity scene units.</returns>
        public double MetersToScene(double meters, double latitude)
        {
            return WebMercator12ToScene(MetersToWebMercator12(meters, latitude));
        }

        /// <summary>
        /// Convert a distance in Unity scene units to meters in the real world.
        /// Note that latitude is required here because our map uses Web Mercator
        /// projection, which causes distances to stretch as you approach the poles.
        /// </summary>
        /// <param name="sceneUnits">The distance, in Unity scene units, to convert.</param>
        /// <param name="latitude">The latitude at the location at which the conversion applies.
        /// A reasonable value for this would be the latitude at the map origin.</param>
        /// <returns>The distance in meters.</returns>
        public double SceneToMeters(double sceneUnits, double latitude)
        {
            return WebMercator12ToMeters(SceneToWebMercator12(sceneUnits), latitude);
        }

        public double WebMercator12ToScene(double wmUnits)
        {
            return wmUnits * MapScale;
        }

        public static double MetersToWebMercator12(double meters, double latitude)
        {
            return meters / WebMercator12.GetMetersPerUnit(latitude);
        }

        public static double WebMercator12ToMeters(double wmUnits, double latitude)
        {
            return wmUnits * WebMercator12.GetMetersPerUnit(latitude);
        }
    }
}
