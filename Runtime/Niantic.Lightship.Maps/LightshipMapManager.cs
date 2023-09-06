// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Configuration;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;

namespace Niantic.Lightship.Maps
{
    /// <summary>
    /// This class is responsible for initializing the <c>Maps.Core</c>
    /// library, which is the Maps SDK component that's responsible
    /// for downloading and parsing raw maptile data.  The entrypoint to
    /// the <c>Maps.Core</c> library is <see cref="LightshipMapsSystem"/>,
    /// which <see cref="LightshipMapManager"/> initializes in its
    /// <see cref="Awake"/> method.  NOTE: <see cref="LightshipMapsSystem"/>
    /// is a singleton, so there should only be a maximum of one active
    /// <see cref="LightshipMapManager"/> at a time in any given scene.
    /// </summary>
    [PublicAPI]
    [DefaultExecutionOrder(DefaultExecutionOrder)]
    public class LightshipMapManager : MonoBehaviour
    {
        /// <summary>
        /// The value used for <see cref="LightshipMapManager"/>'s
        /// <see cref="UnityEngine.DefaultExecutionOrder"/> attribute.
        /// Note that this value is less than, and relative to,
        /// <see cref="LightshipMapView.DefaultExecutionOrder"/>.
        /// This allows <see cref="LightshipMapManager.Awake"/>
        /// to run before any <see cref="LightshipMapView.Awake"/>
        /// methods execute, since <see cref="LightshipMapView"/>
        /// needs to call into <see cref="LightshipMapManager"/>
        /// during its initialization.
        /// </summary>
        public const int DefaultExecutionOrder =
            LightshipMapView.DefaultExecutionOrder - 10;

        [Tooltip("An optional ISO 639-1 language code specifying which " +
            "localized strings will be used for labels, if a translation " +
            "in that language is available.  If no translation matching " +
            "this language exists, then the translation in the language " +
            "used by people geographically local to that area, which is " +
            "guaranteed to always exist, will be used as a fallback.  If " +
            "this field is empty, all labels will use the 'local' version " +
            "of their text strings by default.")]
        [SerializeField]
        private string _labelLanguage = "en";

        private ILightshipMapsSystem _mapsSystem;

        private string _lightshipApiKey;

        private static ChannelLogger Log { get; } = new(nameof(LightshipMapManager));

        /// <summary>
        /// The ISO 639-1 language code specifying which
        /// localized strings are used for labels.
        /// </summary>
        public string Language { get => _labelLanguage; }

        /// <summary>
        /// Creates a new <see cref="IMapView"/>, which is primarily
        /// intended to be used by a <see cref="LightshipMapView"/>.
        /// </summary>
        /// <returns>A new <see cref="IMapView"/></returns>
        public IMapView CreateMapView() => _mapsSystem.CreateMapView();

        /// <summary>
        /// True if <see cref="LightshipMapManager"/> initialized
        /// successfully, or false if initialization failed.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Called at startup to initialize our
        /// <see cref="LightshipMapsSystem"/>.
        /// </summary>
        private void Awake()
        {
            // Load a Lightship API key, or prompt to enter one
            _lightshipApiKey = LightshipApiKeyUtil.Load();

            Log.Info("Initializing Maps.Core");
            _mapsSystem = LightshipMapsSystem.Initialize(
                Application.persistentDataPath,
                _lightshipApiKey,
                labelLanguage: _labelLanguage);

            IsInitialized = _mapsSystem != null;
        }

        /// <summary>
        /// Shuts down and re-initializes the map
        /// </summary>
        public void Reinitialize()
        {
            _mapsSystem.Shutdown();
            _mapsSystem = LightshipMapsSystem.Initialize(
                Application.persistentDataPath,
                _lightshipApiKey,
                labelLanguage: Language);
        }

        /// <summary>
        /// Called at shutdown to shut down our
        /// <see cref="LightshipMapsSystem"/>.
        /// </summary>
        private void OnDestroy()
        {
            _mapsSystem?.Shutdown();
        }

        /// <summary>
        /// Updates the language.  After a language update, the map must be
        /// refreshed by calling <see cref="ILightshipMapView.RefreshMap"/>.
        /// <param name="language">ISO 639-1 language code to use for map labels.</param>
        /// </summary>
        public void UpdateLanguage(string language)
        {
            _labelLanguage = language;
            Reinitialize();
        }
    }
}
