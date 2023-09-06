// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.ARDK.VPSCoverage;
using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.Maps.Samples.CoverageWithMaptiles
{
    internal class TargetMarker : MonoBehaviour
    {
        [SerializeField]
        private RawImage _image;

        [SerializeField]
        private Color _selectedColor = Color.green;

        [SerializeField]
        private Color _unselectedColor = Color.red;

        public LocalizationTarget Target { get; private set; }

        public void SetIsSelected(bool isSelected)
        {
            _image.color = isSelected ? _selectedColor : _unselectedColor;
        }

        public void Initialize(LocalizationTarget target)
        {
            Target = target;
            SetIsSelected(false);
        }
    }
}
