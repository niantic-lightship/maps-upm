// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if ENABLE_LEGACY_INPUT_MANAGER

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityInput = UnityEngine.Input;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras
{
    /// <summary>
    /// Cross-platform class that unifies Unity's mouse and Touch input APIs.
    ///
    /// When run in the Unity Editor, the class will convert mouse input changes
    /// into touch input.  When run natively on a mobile device, the class will
    /// simply surface Input.GetTouch, Input.touchCount, etc.
    /// </summary>
    internal static class PlatformAgnosticInput
    {
        /// <summary>
        /// (Basically) a state machine used to track mouse drags, persistent touches, etc.
        /// </summary>
        private class MouseEventBuffer
        {
            public void Update(TouchPhase touchPhase, Vector2 mousePosition)
            {
                if (_lastFrame == Time.frameCount)
                {
                    // Already updated this frame
                    return;
                }

                _lastFrame = Time.frameCount;
                _touchPhase = touchPhase;

                switch (touchPhase)
                {
                    case TouchPhase.Began:
                        _currentDelta = Vector2.zero;
                        _priorPosition = mousePosition;
                        break;

                    default:
                        // Movement.
                        _currentDelta = mousePosition - _priorPosition;
                        _priorPosition = mousePosition;

                        if (touchPhase == TouchPhase.Moved)
                        {
                            _touchPhase =
                                _currentDelta == Vector2.zero
                                    ? TouchPhase.Stationary
                                    : TouchPhase.Moved;
                        }

                        break;
                }
            }

            public Touch GetTouch()
            {
                var touch = new Touch
                {
                    fingerId = 1,
                    phase = _touchPhase,
                    position = _priorPosition,
                    deltaPosition = _currentDelta
                };

                return touch;
            }

            private TouchPhase _touchPhase;
            private Vector2 _priorPosition;
            private Vector2 _currentDelta;

            private int _lastFrame;
        }

        // ReSharper disable once InconsistentNaming
        private static readonly MouseEventBuffer _mouseEventBuffer = new();

        /// <summary>
        /// The number of touches.
        /// </summary>
        public static int TouchCount
        {
            get
            {
                if (Application.isMobilePlatform)
                {
                    return UnityInput.touchCount;
                }

                const KeyCode m0 = KeyCode.Mouse0;
                return UnityInput.GetKey(m0) || UnityInput.GetKeyDown(m0) || UnityInput.GetKeyUp(m0) ? 1 : 0;
            }
        }

        /// <summary>
        /// Call to obtain the status of a finger touching the screen.
        /// </summary>
        /// <param name="index">The touch input on the device screen.  If
        /// touchCount is greater than zero, this parameter sets which screen
        /// touch to check. Use zero to obtain the first screen touch.</param>
        /// <returns>Touch details as a struct.</returns>
        public static Touch GetTouch(int index)
        {
            return Application.isMobilePlatform ? UnityInput.GetTouch(index) : TouchFromMouse();
        }

        /// <summary>
        /// Determines if a specific touch is over any UI raycast targets.  Useful
        /// for discounting screen touches before processing them as gestures.
        /// </summary>
        /// <param name="position">The current mouse or touch position</param>
        /// <returns>True if the position is over a UI object</returns>
        public static bool IsOverUIObject(Vector2 position)
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = position
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        private static Touch TouchFromMouse()
        {
            const KeyCode m0 = KeyCode.Mouse0;
            var touch = new Touch();

            // Send different state changes depending on the Mouse Click state...
            if (UnityInput.GetKeyDown(m0))
            {
                _mouseEventBuffer.Update(TouchPhase.Began, UnityInput.mousePosition);
                touch = _mouseEventBuffer.GetTouch();
            }
            else if (UnityInput.GetKeyUp(m0))
            {
                _mouseEventBuffer.Update(TouchPhase.Ended, UnityInput.mousePosition);
                touch = _mouseEventBuffer.GetTouch();
            }
            else if (UnityInput.GetKey(m0))
            {
                _mouseEventBuffer.Update(TouchPhase.Moved, UnityInput.mousePosition);
                touch = _mouseEventBuffer.GetTouch();
            }
            else
            {
                touch.phase = TouchPhase.Canceled;
            }

            return touch;
        }
    }
}
#endif
