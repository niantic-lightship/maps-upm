// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Themes;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.MapThemes.ThemeSelector
{
    internal class ThemeSelector : MonoBehaviour
    {
        [SerializeField]
        private LightshipMapView _lightshipMapView;

        [SerializeField]
        private MapTheme[] _mapThemes;

        [SerializeField]
        [HideInInspector]
        private TMP_Text _textField;

        private int _themeIndex;

        private void Awake()
        {
            SetNewTheme(0);
        }

        public void LastTheme() => UpdateCurrentTheme(-1);
        public void NextTheme() => UpdateCurrentTheme(1);

        private void UpdateCurrentTheme(int offset)
        {
            var theme = SetNewTheme(offset);
            _lightshipMapView.SetMapTheme(theme);
        }

        private MapTheme SetNewTheme(int offset)
        {
            var ind = (_themeIndex + offset) % _mapThemes.Length;
            _themeIndex = ind < 0 ? _mapThemes.Length - 1 : ind;
            var theme = _mapThemes[_themeIndex];
            _textField.text = theme.ThemeName;
            return theme;
        }
    }
}
