// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    /// <summary>
    /// UI menu item to show buildings to be built, checks if there is enough resources to build this structure
    /// </summary>
    internal class MapGameBuildingMenuItem : MonoBehaviour
    {
        [SerializeField]
        private MapGameState.StructureType _structureType;

        [SerializeField]
        private TMP_Text _structureText;

        [SerializeField]
        private TMP_Text _resourceTypeText;

        [SerializeField]
        private TMP_Text _unitsText;

        [SerializeField]
        private Button _buildButton;

        private void OnEnable()
        {
            var cost = MapGameState.Instance.GetStructureCost(_structureType);
            int requiredAmount = cost.Amount;
            var resourceType = cost.Resource;

            _structureText.text = _structureType.ToString();
            _resourceTypeText.text = resourceType.ToString();
            int currentResourceCount = MapGameState.Instance.GetResource(resourceType);
            _unitsText.text = $"{currentResourceCount} / {requiredAmount}";
            _buildButton.interactable = currentResourceCount >= requiredAmount;
        }
    }
}
