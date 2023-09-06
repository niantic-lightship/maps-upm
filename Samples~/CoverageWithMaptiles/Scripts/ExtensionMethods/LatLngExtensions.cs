// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using ArdkLatLng = Niantic.ARDK.LocationService.LatLng;
using MapsLatLng = Niantic.Lightship.Maps.Core.Coordinates.LatLng;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Niantic.Lightship.Maps.Samples.CoverageWithMaptiles.ExtensionMethods
{
    /// <summary>
    /// Extension methods for converting the ARDK's <see cref="ArdkLatLng"/>
    /// type to the Maps SDK's <see cref="MapsLatLng"/> type, and vice-versa.
    /// </summary>
    internal static class LatLngExtensions
    {
        /// <summary>
        /// Converts a Maps SDK <see cref="MapsLatLng"/> to an ARDK <see cref="ArdkLatLng"/>
        /// </summary>
        public static ArdkLatLng ToArdkLatLng(this MapsLatLng mapsLatLng) =>
            new(mapsLatLng.Latitude, mapsLatLng.Longitude);

        /// <summary>
        /// Converts an ARDK <see cref="ArdkLatLng"/> to a Maps SDK <see cref="MapsLatLng"/>
        /// </summary>
        public static MapsLatLng ToMapsLatLng(this ArdkLatLng ardkLatLng) =>
            new(ardkLatLng.Latitude, ardkLatLng.Longitude);

        /// <summary>
        /// Converts an array of Maps SDK <see cref="MapsLatLng"/>s
        /// to an array of ARDK <see cref="ArdkLatLng"/>s
        /// </summary>
        public static ArdkLatLng[] ToArdkLatLng(this MapsLatLng[] mapsLatLng)
        {
            var results = new ArdkLatLng[mapsLatLng.Length];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = mapsLatLng[i].ToArdkLatLng();
            }

            return results;
        }

        /// <summary>
        /// Converts an array of ARDK <see cref="ArdkLatLng"/>s
        /// to an array of Maps SDK <see cref="MapsLatLng"/>s
        /// </summary>
        public static MapsLatLng[] ToMapsLatLng(this ArdkLatLng[] ardkLatLng)
        {
            var results = new MapsLatLng[ardkLatLng.Length];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = ardkLatLng[i].ToMapsLatLng();
            }

            return results;
        }
    }
}
