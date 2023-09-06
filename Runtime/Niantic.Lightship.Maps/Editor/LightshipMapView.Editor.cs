// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.Editor;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps
{
    /// <inheritdoc cref="ILightshipMapViewWritable" />
    public partial class LightshipMapView : ILightshipMapViewWritable
    {
        /// <inheritdoc />
        LightshipMapManager ILightshipMapViewWritable.LightshipMapManager
        {
            set => _lightshipMapManager = value;
        }
    }
}

#endif
