// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Platform.Debugging;
using UnityEngine;

namespace Niantic.Lightship.Maps.Themes
{
    /// <summary>
    /// This <see cref="MonoBehaviour"/> will rotate the
    /// skybox at the specified rate, which can give the
    /// appearance of clouds moving across the sky.
    /// </summary>
    [PublicAPI]
    public class RotateSkybox : MonoBehaviour
    {
        [Tooltip("The speed at which the skybox is rotated.")]
        [SerializeField]
        private float _animSpeed;

        private static readonly int Rotation = Shader.PropertyToID("_Rotation");

        private void Update()
        {
            if (RenderSettings.skybox.HasProperty(Rotation))
            {
                RenderSettings.skybox.SetFloat(Rotation, Time.time * _animSpeed);
            }
            else
            {
                Log.Warn("Skybox does not contain a rotation property.");
            }
        }
    }
}
