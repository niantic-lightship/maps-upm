// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Sources
{
    internal class UnityMouseInput : IInputSource
    {
        private const uint MaxMouseButtons = 6;

        /// <inheritdoc />
        public List<InputEvent> Events { get; } = new();

        /// <inheritdoc />
        public void CollectInput()
        {
            float reciprocalWidth = 1.0f / Screen.width;
            float reciprocalHeight = 1.0f / Screen.height;

            var mousePositionNormalized = new Vector3(
                UnityInput.mousePosition.x * reciprocalWidth,
                UnityInput.mousePosition.y * reciprocalHeight);

            Events.Clear();

            for (var i = 0; i < MaxMouseButtons; i++)
            {
                var phase = CurrentMousePhase(i);

                // Skip sending hovered events for all the buttons
                if (i == 0 || phase != InputPhase.Hovered)
                {
                    float time = Time.unscaledTime;
                    var scroll = UnityInput.mouseScrollDelta;
                    var transform = new TransformData(i, mousePositionNormalized);
                    var inputEvent = new InputEvent(this, phase, time, transform, scroll);

                    Events.Add(inputEvent);
                }
            }
        }

        private static InputPhase CurrentMousePhase(int buttonId)
        {
            if (UnityInput.GetMouseButtonDown(buttonId))
            {
                return InputPhase.Began;
            }

            if (UnityInput.GetMouseButton(buttonId))
            {
                return InputPhase.Held;
            }

            return UnityInput.GetMouseButtonUp(buttonId)
                ? InputPhase.Ended
                : InputPhase.Hovered;
        }
    }
}
