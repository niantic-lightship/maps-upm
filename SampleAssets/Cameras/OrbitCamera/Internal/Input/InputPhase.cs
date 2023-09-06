// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input
{
    /// <summary>
    /// Input event lifecycle descriptor
    /// </summary>
    internal enum InputPhase
    {
        Began,
        Held,
        Ended,
        Hovered,
        Canceled
    }
}
