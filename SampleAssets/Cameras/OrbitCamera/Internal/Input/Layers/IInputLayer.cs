// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Layers
{
    /// <summary>
    /// InputLayers correspond to input "scenes". For instance,
    /// there should be one layer that handles all the input
    /// you would want to process while on the world map.
    /// </summary>
    internal interface IInputLayer
    {
        /// <summary>
        /// Called every frame with all the input events
        /// that this layer is eligible to handle.
        /// </summary>
        void ProcessEvents(List<InputEvent> events);
    }
}
