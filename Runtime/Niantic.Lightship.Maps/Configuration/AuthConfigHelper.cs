// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using System.Linq;
using Niantic.Lightship.Maps.Editor;
using Niantic.Lightship.Maps.ExtensionMethods;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Configuration
{
    internal class AuthConfigHelper : EditorWindow
    {
        private const string Name = "API Key Helper";

        private string _lightshipKey = "";
        private MapsAuthConfig _authConfig;
        private string _currentApiKey;

        [MenuItem(MenuConstants.MainMenuRoot + Name)]
        public static void ShowHelperWindow()
        {
            var auth = GetWindow<AuthConfigHelper>(Name);
            auth.Initialize();
        }

        private void Initialize()
        {
            _authConfig = MapsAuthConfig.Load().FirstOrDefault();
            _currentApiKey = _authConfig.IsReferenceNull() ? "" : _authConfig!.LightshipAPIKey;
        }

        private void OnGUI()
        {
            GUILayout.Label("Add Lightship API Key", EditorStyles.boldLabel);

            _lightshipKey = EditorGUILayout.TextField("Lightship API Key", _lightshipKey);
            GUILayout.Label($"Current API Key: {_currentApiKey} ", EditorStyles.label);

            if (GUILayout.Button("Setup"))
            {
                if (_authConfig.IsReferenceNull())
                {
                    _authConfig = MapsAuthConfig.Create();
                }

                if (!string.IsNullOrEmpty(_lightshipKey))
                {
                    if (string.IsNullOrWhiteSpace(_authConfig.LightshipAPIKey))
                    {
                        _authConfig.LightshipAPIKey = _lightshipKey;
                        EditorUtility.DisplayDialog(
                            "Lightship API Key",
                            "Lightship API Key has been set correctly",
                            "Ok");
                    }
                    else
                    {
                        var changeApiKey = EditorUtility.DisplayDialog(
                            "Override current API Key?",
                            "API Key found. This action will override current API key.",
                            "Ok",
                            "Cancel");

                        _authConfig.LightshipAPIKey = changeApiKey ? _lightshipKey : _authConfig.LightshipAPIKey;
                    }

                    EditorUtility.SetDirty(_authConfig);
                    AssetDatabase.SaveAssetIfDirty(_authConfig);
                    AssetDatabase.Refresh();
                }
                else
                {
                    EditorUtility.DisplayDialog("Lightship API Key", "Insert a valid API Key and try again", "Ok");
                }

                Close();
            }
        }
    }
}
#endif
