// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;

namespace Niantic.Lightship.Maps.Editor
{
    /// <summary>
    /// This internal, Editor-only interface exposes serialized fields
    /// and other methods that are meant to be used to modify internal
    /// state of <see cref="LightshipMapView"/>s programmatically.
    /// </summary>
    internal interface ILightshipMapViewWritable
    {
        /// <summary>
        /// The reference to this map view's <see cref="LightshipMapManager"/>
        /// </summary>
        LightshipMapManager LightshipMapManager { set; }
    }
}

#endif
