// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.CustomMapLayers.StateBoundaries
{
    internal class StateBoundaryCollection : ScriptableObject
    {
        [SerializeField]
        private List<State> _states = new();

        public List<State> States => _states;
    }
}
