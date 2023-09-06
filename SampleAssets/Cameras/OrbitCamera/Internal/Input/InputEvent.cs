// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Gestures;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Sources;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input
{
    [Serializable]
    internal sealed class InputEvent
    {
        /// <summary>
        /// What part of the touch's lifetime is this event occuring in
        /// </summary>
        public readonly InputPhase Phase;

        /// <summary>
        /// The unscaled seconds since startup that this input was created
        /// </summary>
        public readonly float Time;

        /// <summary>
        /// Contains positional data such as mouse/touch Position
        /// and DeltaPosition, if applicable to this event.
        /// </summary>
        public readonly TransformData Transform;

        /// <summary>
        /// The source that emitted this event
        /// </summary>
        public readonly IInputSource Source;

        /// <summary>
        /// Scrolling amount if this is an event that contains scrolling
        /// </summary>
        public readonly Vector2? ScrollDelta;

        /// <summary>
        /// Unless this input is consumed, this field will be null
        /// </summary>
        public IGesture ConsumedBy { get; private set; }

        public InputEvent(
            IInputSource source,
            InputPhase phase,
            float time,
            in TransformData transform,
            Vector2? scrollDelta = null)
        {
            Source = source;
            Phase = phase;
            Transform = transform;
            Time = time;
            ScrollDelta = scrollDelta;
            ConsumedBy = null;
        }

        public void Consume(IGesture consumer)
        {
            if (ConsumedBy != null)
            {
                throw new InvalidOperationException("InputEvent already consumed");
            }

            ConsumedBy = consumer;
        }
    }
}
