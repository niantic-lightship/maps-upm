// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    /// <summary>
    /// Building that is placed on the map, buildings consume and generate resources at a particular rate
    /// This class also shows an example in Update of maintaining an objects position on the map without
    /// being a child of an IMapTile. It updates it's position based on LatLng to ensure it is in the right spot
    /// regardless of map or camera movements
    /// </summary>
    internal class MapGameBuilding : MonoBehaviour
    {
        [SerializeField]
        private MapGameState.ResourceType _resourceToConsume;

        [SerializeField]
        private MapGameState.ResourceType _resourceToCreate;

        [SerializeField]
        private float _resourceGenerationRate = 3.0f;

        [SerializeField]
        private FloatingText.FloatingText _floatingTextPrefab;

        private float _lastResourceGenerateTime;

        private void Update()
        {
            // consume and generate resources
            if (Time.time > _lastResourceGenerateTime + _resourceGenerationRate)
            {
                _lastResourceGenerateTime = Time.time;
                if (MapGameState.Instance.GetResource(_resourceToConsume) > 0)
                {
                    int amount = 1;
                    MapGameState.Instance.SpendResource(_resourceToConsume, amount);
                    MapGameState.Instance.AddResource(_resourceToCreate, amount);

                    // spawn an animated floating text to show resources being gained
                    var floatingTextPosition = transform.position + Vector3.up * 30.0f;
                    var floatText = Instantiate(_floatingTextPrefab, floatingTextPosition, Quaternion.identity);
                    floatText.SetText($"+{amount} {_resourceToCreate.ToString()}");
                }
            }
        }
    }
}
