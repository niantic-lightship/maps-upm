// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Gestures
{
    /// <summary>
    /// Base interface for gestures.
    /// </summary>
    internal interface IGesture
    {
        /// <summary>
        /// Called after all input for this frame has been passed to this gesture
        /// </summary>
        void PostProcessInput();
    }

    /// <summary>
    /// Input gesture handled in screen space
    /// </summary>
    internal interface IScreenInputGesture : IGesture
    {
        void ProcessEvent(InputEvent inputEvent);
    }
}
