// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.CustomMapLayers.StateBoundaries
{
    internal class StateBoundariesInternal : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown _stateDropdown;

        [SerializeField]
        private StateBoundaryCollection _stateBoundaryCollection;

        [SerializeField]
        private StateBoundarySelector _stateBoundarySelector;

        private void Start()
        {
            foreach (var state in _stateBoundaryCollection.States)
            {
                _stateDropdown.options.Add(new TMP_Dropdown.OptionData(state.Name));
            }
        }

        public void OnSelectStateDropdownValueChanged(int value)
        {
            var selectedState = _stateBoundaryCollection.States[value];
            _stateBoundarySelector.OnStateSelected(selectedState);
        }
    }
}
