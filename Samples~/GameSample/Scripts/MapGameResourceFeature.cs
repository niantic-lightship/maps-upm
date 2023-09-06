// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    /// <summary>
    /// Resource feature is a component on resources spawned on that map at particular map features
    /// It generates resources over time which can be harvested by the user
    /// </summary>
    internal class MapGameResourceFeature : MonoBehaviour
    {
        [SerializeField]
        private MapGameState.ResourceType _resourceType;

        [SerializeField]
        private int _maxUnits;
        public MapGameState.ResourceType ResourceType => _resourceType;

        private int _currentUnits;
        private float _resourceIncreaseTime;

        public bool ResourcesAvailable => _currentUnits > 0;

        public int GainResources()
        {
            int currentUnits = _currentUnits;
            _currentUnits = 0;
            return currentUnits;
        }

        private void Update()
        {
            if (Time.time > _resourceIncreaseTime + 2.0f)
            {
                _resourceIncreaseTime = Time.time;
                _currentUnits = Mathf.Min(_currentUnits + 1, _maxUnits);
            }
        }
    }
}
