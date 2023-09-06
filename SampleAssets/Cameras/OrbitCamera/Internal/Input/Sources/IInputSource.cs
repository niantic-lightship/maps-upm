// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Sources
{
    internal interface IInputSource
    {
        List<InputEvent> Events { get; }

        void CollectInput();
    }
}
