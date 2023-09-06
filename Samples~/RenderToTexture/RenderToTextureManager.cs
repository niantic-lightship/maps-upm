// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.SampleAssets.Cameras.OrthographicCamera;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace Niantic.Lightship.Maps.Samples.RenderToTexture
{
    internal class RenderToTextureManager : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _toggleMapButtonText;

        [SerializeField]
        private OrthographicCameraController _cameraController;

        [SerializeField]
        private Animator _renderTextureAnimator;

        private bool _isMapActive;
        private readonly Random _random = new();
        private static readonly int AnimationIndex = Animator.StringToHash("AnimationIndex");
        private static readonly int IsMapVisible = Animator.StringToHash("IsMapVisible");

        /// <summary>
        /// This method is called when our toggle button is clicked to hide or
        /// show the map texture and enable or disable the map camera controller.
        /// </summary>
        public void ToggleMap()
        {
            // Toggle the map on or off
            _isMapActive = !_isMapActive;

            // Set the toggle button's text to "hide" or "show"
            _toggleMapButtonText.text = _isMapActive ? "Hide Map" : "Show Map";

            // Enable or disable the camera controller
            _cameraController.enabled = _isMapActive;

            // Pick a random hide or show animation
            var animationCount = _isMapActive ? 1 : 4;
            var index = _random.Next(0, animationCount);
            _renderTextureAnimator.SetInteger(AnimationIndex, index);
            _renderTextureAnimator.SetBool(IsMapVisible, _isMapActive);
        }
    }
}
