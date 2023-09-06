// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Niantic.Lightship.Maps.Utilities;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.FeatureLabelsLanguage.SwitchLanguage
{
    internal class SwitchLanguage : MonoBehaviour
    {
        [SerializeField]
        private LightshipMapView _lightshipMapView;

        [SerializeField]
        private LightshipMapManager _lightshipMapManager;

        [SerializeField]
        private TMP_Dropdown _languageDropdown;

        // List of languages to language codes
        private static readonly SortedDictionary<string, string> LanguageSelectionToSetting = new()
        {
            // See full list of language codes here:
            // https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
            { "German (de)", "de" },
            { "English (en)", "en" },
            { "Spanish (es)", "es" },
            { "French (fr)", "fr" },
            { "Italian (it)", "it" }
        };

        private static ChannelLogger Log { get; } = new(nameof(SwitchLanguage));

        private void Start()
        {
            _languageDropdown.AddOptions(LanguageSelectionToSetting.Keys.ToList());
            _languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            var languageValues = LanguageSelectionToSetting.Values.ToList();
            var index = languageValues.IndexOf(_lightshipMapManager.Language);
            _languageDropdown.value = index > 0 ? index : 0;
        }

        private void OnLanguageChanged(int selectedValue)
        {
            var selectedLanguage = _languageDropdown.options[selectedValue].text;
            var languageCode = LanguageSelectionToSetting.GetValueOrDefault(selectedLanguage);

            if (string.IsNullOrEmpty(languageCode))
            {
                Log.Error($"Invalid selection index '{selectedValue}'");
                return;
            }

            _lightshipMapManager.UpdateLanguage(languageCode);
            _lightshipMapView.RefreshMap();
        }
    }
}
