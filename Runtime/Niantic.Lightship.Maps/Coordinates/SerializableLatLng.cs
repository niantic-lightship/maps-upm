// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Coordinates;
using UnityEngine;

namespace Niantic.Lightship.Maps.Coordinates
{
    /// <summary>
    /// A serializable version of <see cref="LatLng"/>.  The
    /// <see cref="LatLng"/> type is a <c>readonly struct</c>,
    /// which can't easily be serialized as fields in Unity
    /// Objects or modified in the Editor's Inspector.  The
    /// <see cref="SerializableLatLng"/> type is an Editor-
    /// friendly version of the <c>readonly</c> value type,
    /// with methods to easily convert between the two.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public partial class SerializableLatLng
    {
        [SerializeField]
        private double _latitude;

        [SerializeField]
        private double _longitude;

        /// <inheritdoc cref="LatLng.Latitude" />
        public double Latitude => _latitude;

        /// <inheritdoc cref="LatLng.Longitude" />
        public double Longitude => _longitude;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="latitude">Latitude, in degrees</param>
        /// <param name="longitude">Longitude, in degrees</param>
        public SerializableLatLng(double latitude, double longitude)
        {
            _latitude = LatLng.ClampLatitude(latitude);
            _longitude = LatLng.WrapLongitude(longitude);
        }

        /// <summary>
        /// Implicit conversion from a <see cref="SerializableLatLng"/>
        /// to a <see cref="LatLng"/>.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The lat/lng value as a <see cref="LatLng"/></returns>
        public static implicit operator LatLng(SerializableLatLng value)
            => new(value._latitude, value._longitude);

        /// <summary>
        /// Implicit conversion from a <see cref="LatLng"/>
        /// to a <see cref="SerializableLatLng"/>.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The lat/lng value as a <see cref="SerializableLatLng"/></returns>
        public static implicit operator SerializableLatLng(in LatLng value)
            => new(value.Latitude, value.Longitude);
    }
}
