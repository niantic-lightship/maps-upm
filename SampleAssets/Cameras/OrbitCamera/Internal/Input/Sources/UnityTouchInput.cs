// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Sources
{
    internal class UnityTouchInput : IInputSource
    {
        /// <inheritdoc />
        public List<InputEvent> Events { get; } = new();

        /// <inheritdoc />
        public void CollectInput()
        {
            Events.Clear();

            float reciprocalWidth = 1.0f / Screen.width;
            float reciprocalHeight = 1.0f / Screen.height;

            for (var i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                // Find prev touch of this finger id.
                // If it exists, create delta position.
                var touch = UnityEngine.Input.GetTouch(i);

                var position = new Vector3(
                    touch.position.x * reciprocalWidth,
                    touch.position.y * reciprocalHeight);

                float time = Time.unscaledTime;
                var phase = ToInputPhase(touch.phase);
                var transformData = new TransformData(touch.fingerId, position);
                var inputEvent = new InputEvent(this, phase, time, transformData);

                Events.Add(inputEvent);
            }
        }

        private static InputPhase ToInputPhase(TouchPhase phase)
        {
            return phase switch
            {
                TouchPhase.Began => InputPhase.Began,
                TouchPhase.Canceled => InputPhase.Canceled,
                TouchPhase.Ended => InputPhase.Ended,
                TouchPhase.Moved => InputPhase.Held,
                TouchPhase.Stationary => InputPhase.Held,
                _ => throw new NotImplementedException($"Unexpected UnityEngine.TouchPhase {phase}")
            };
        }
    }
}
