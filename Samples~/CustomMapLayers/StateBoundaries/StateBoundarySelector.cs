// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Niantic.Lightship.Maps.Samples.CustomMapLayers.StateBoundaries
{
    [Serializable]
    internal class StateSelectedEvent : UnityEvent<State> { }

    internal class StateBoundarySelector : MonoBehaviour
    {
        [SerializeField]
        private StateSelectedEvent _stateSelected = new();

        internal void OnStateSelected(State state)
        {
            _stateSelected.Invoke(state);
        }
    }
}
