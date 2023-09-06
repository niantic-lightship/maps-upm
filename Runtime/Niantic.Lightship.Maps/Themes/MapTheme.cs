// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders;
using UnityEngine;

namespace Niantic.Lightship.Maps.Themes
{
    /// <summary>
    /// The <see cref="MapTheme"/> class contains all of
    /// the maptile feature builders and any other settings
    /// that define a given <see cref="LightshipMapView"/>'s
    /// visual style when rendered in-game.
    /// </summary>
    [PublicAPI]
    public partial class MapTheme : MonoBehaviour
    {
        [Tooltip("The theme's name (for display purposes only).")]
        [SerializeField]
        private string _themeName;

        [Tooltip("A list of maptile feature builders that " +
            "construct renderable geometry and place objects " +
            "or labels based on maptile feature data.")]
        [SerializeField]
        private List<FeatureBuilderBase> _builders = new();

        [Header("Set Skybox")]
        [SerializeField]
        private Material _skybox;

        [Tooltip("When true, default skybox is set if no skybox is assigned.")]
        [SerializeField]
        private bool _useDefaultIfUnassigned;

        [SerializeField]
        private Color _skyboxColor;

        private static readonly int Tint = Shader.PropertyToID("_Tint");
        private bool _showWarning = true;

        public string ThemeName => _themeName;
        internal IReadOnlyList<FeatureBuilderBase> Builders => _builders;

        internal void InitializeTheme(ILightshipMapView lightshipMapView)
        {
            InitializeBuilders(lightshipMapView);
            SetSkybox();
        }

        /// <summary>
        /// Initialize the current theme's builders.
        /// </summary>
        /// <param name="lightshipMapView">The <see cref="ILightshipMapView"/>
        /// to which this <see cref="MapTheme"/> is applied.</param>
        private void InitializeBuilders(ILightshipMapView lightshipMapView)
        {
            foreach (var builder in _builders)
            {
                builder.Initialize(lightshipMapView);
            }
        }

        private void SetSkybox()
        {
            // Instantiate a new material for our skybox
            var skybox = _skybox != null
                ? new Material(_skybox)
                : _useDefaultIfUnassigned
                    ? new Material(SkyboxUtils.DefaultSkybox)
                    : null;

            // Set the skybox material's color
            if (skybox != null && skybox.HasProperty(Tint))
            {
                skybox.SetColor(Tint, _skyboxColor);
            }

            RenderSettings.skybox = skybox;
        }
    }
}
