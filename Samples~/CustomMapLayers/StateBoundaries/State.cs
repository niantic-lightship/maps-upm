// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Niantic.Lightship.Maps.Coordinates;
using Niantic.Lightship.Maps.Core.Coordinates;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.CustomMapLayers.StateBoundaries
{
    [Serializable]
    [DebuggerDisplay("{_name}")]
    internal class State
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private string _stateId;

        [SerializeField]
        private int _stateNumber;

        [SerializeField]
        private double _latitudeMin;

        [SerializeField]
        private double _latitudeMax;

        [SerializeField]
        private double _longitudeMin;

        [SerializeField]
        private double _longitudeMax;

        [SerializeField]
        private SerializableLatLng _center;

        [SerializeField]
        private List<Boundary> _boundaries = new();

        public string Name => _name;
        public string StateId => _stateId;
        public int StateNumber => _stateNumber;
        public LatLng Center => _center;
        public List<Boundary> Boundaries => _boundaries;

        public double LatitudeMin => _latitudeMin;
        public double LatitudeMax => _latitudeMax;
        public double LongitudeMin => _longitudeMin;
        public double LongitudeMax => _longitudeMax;
    }
}
