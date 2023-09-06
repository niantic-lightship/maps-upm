// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.MapLayers.Components;
using Niantic.Lightship.Maps.ObjectPools;
using Niantic.Lightship.Maps.Samples.CustomMapLayers.StateBoundaries;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.CustomMapLayers
{
    using PooledObjectList = List<PooledObject<GameObject>>;

    internal class CustomMapLayersManager : MonoBehaviour
    {
        [SerializeField]
        private LayerPolygonRenderer _visitedStatePolygonRenderer;

        [SerializeField]
        private LayerPolygonRenderer _selectedStatePolygonRenderer;

        [SerializeField]
        private LayerLineRenderer _selectedStateLineRenderer;

        private State _selectedState;
        private readonly HashSet<State> _visitedStates = new();
        private readonly PooledObjectList _selectedStatePolygons = new();
        private readonly PooledObjectList _selectedStateBorders = new();

        private static ChannelLogger Log { get; } = new(nameof(CustomMapLayers));

        public void OnStateSelected(State state)
        {
            if (_visitedStates.Contains(state))
            {
                return;
            }

            Log.Info($"Selected {state.Name}");
            var previousState = _selectedState;
            _visitedStates.Add(state);
            _selectedState = state;

            _selectedStatePolygons.ForEach(p => p.Dispose());
            _selectedStateBorders.ForEach(p => p.Dispose());

            _selectedStatePolygons.Clear();
            _selectedStateBorders.Clear();

            var stateName = $"Selected State ({state.StateId})";
            var borderName = $"Selected State Border ({state.StateId})";

            foreach (var boundary in state.Boundaries)
            {
                _selectedStatePolygons.Add(_selectedStatePolygonRenderer.DrawPolygon(boundary.Points, stateName));
                _selectedStateBorders.Add(_selectedStateLineRenderer.DrawLoop(boundary.Points, borderName));
            }

            if (previousState == null)
            {
                return;
            }

            var visitedName = $"Visited State ({previousState.StateId})";

            foreach (var boundary in previousState.Boundaries)
            {
                _visitedStatePolygonRenderer.DrawPolygon(boundary.Points, visitedName);
            }
        }
    }
}
