// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Coordinates;
using Niantic.Lightship.Maps.Core.Coordinates;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.CustomMapLayers.StateBoundaries
{
    [Serializable]
    internal class Boundary : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableLatLng> _points = new();

        public List<LatLng> Points { get; } = new();

        public void OnBeforeSerialize()
        {
            _points.Clear();

            foreach (SerializableLatLng latLng in Points)
            {
                _points.Add(latLng);
            }
        }

        public void OnAfterDeserialize()
        {
            Points.Clear();

            foreach (LatLng latLng in _points)
            {
                Points.Add(latLng);
            }
        }
    }
}
