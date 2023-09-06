// Copyright 2019 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Gestures;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Layers;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input.Sources;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input
{
    internal class InputService
    {
        private readonly List<IInputSource> _sources = new();
        private readonly List<IInputLayer> _layers = new();
        private readonly List<InputEvent> _tempEventList = new();
        private uint _disableCount;

        private InputService()
        {
            _sources.Add(new UnityTouchInput());
            _sources.Add(new UnityMouseInput());
        }

        public InputService(IScreenInputGesture gesture) : this()
        {
            var inputLayer = new ScreenSpaceInputLayer();
            inputLayer.Register(gesture);
            _layers.Add(inputLayer);
        }

        public void Update()
        {
            foreach (var source in _sources)
            {
                source.CollectInput();
            }

            ProcessInput();
        }

        private static bool IsEventAvailable(InputEvent e)
            => e is { ConsumedBy: null };

        private void ProcessInput()
        {
            foreach (var layer in _layers)
            {
                if (layer == null)
                {
                    continue;
                }

                foreach (var source in _sources)
                {
                    _tempEventList.Clear();

                    foreach (var ev in source.Events)
                    {
                        if (IsEventAvailable(ev))
                        {
                            _tempEventList.Add(ev);
                        }
                    }

                    if (_tempEventList.Count > 0)
                    {
                        layer.ProcessEvents(_tempEventList);

                        // Only accept events from a single layer per
                        // frame (e.g. let touch override mouse).
                        return;
                    }
                }
            }
        }
    }
}
