// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if !UNITY_EDITOR
#define NOT_UNITY_EDITOR
#endif

using System;
using System.Diagnostics;
using Niantic.Lightship.Maps.Core.Exceptions;
using Niantic.Lightship.Maps.Linq;
using UnityEditor;

namespace Niantic.Lightship.Maps.Configuration
{
    /// <summary>
    /// Utility class that loads and reads Lightship API
    /// keys from <see cref="MapsAuthConfig"/> files.
    /// </summary>
    internal static class LightshipApiKeyUtil
    {
        /// <summary>
        /// Reads a lightship API key from a <see cref="MapsAuthConfig"/>
        /// file, and prompts the user to enter one if it isn't found.
        /// </summary>
        /// <returns>A Lightship API key, if found</returns>
        public static string Load()
        {
            var authConfig = MapsAuthConfig.Load();
            var lightshipApiKey = !authConfig.IsEmpty() ? authConfig[0].LightshipAPIKey : null;

            EnsureAuthConfigLoaded(authConfig);
            EnsureValidApiKeyInEditor(lightshipApiKey);
            EnsureValidApiKeyInGame(lightshipApiKey);

            return lightshipApiKey;
        }

        private static bool IsInvalidApiKey(string lightshipApiKey) => string.IsNullOrWhiteSpace(lightshipApiKey);

        [Conditional("UNITY_EDITOR")]
        private static void EnsureAuthConfigLoaded(MapsAuthConfig[] authConfig)
        {
            if (authConfig.IsEmpty() || authConfig.Length > 1)
            {
                var title = authConfig.Length > 1 ? "Multiple AuthConfig Assets found." : "No API Key Found!";

                var message =
                    authConfig.Length > 1
                        ? "Please remove any duplicate AuthConfig assets."
                        : $"Missing API Key.{Environment.NewLine} "
                            + "Please add your Lightship API key to an AuthConfig asset in the next window.";

#if UNITY_EDITOR
                var initializeHelperWindow = authConfig.Length == 0;
                EditorUtility.DisplayDialog(title, message, "OK");
                EditorApplication.isPlaying = false;

                if (initializeHelperWindow)
                {
                    AuthConfigHelper.ShowHelperWindow();
                }
#endif
                throw new MissingLightshipApiKeyException();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private static void EnsureValidApiKeyInEditor(string lightshipApiKey)
        {
            if (IsInvalidApiKey(lightshipApiKey))
            {
                var apiKey = lightshipApiKey switch
                {
                    null => "(null)",
                    "" => "(empty)",
                    _ => lightshipApiKey
                };

                var message =
                    $"Lightship API Key '{apiKey}' is invalid.{Environment.NewLine}"
                    + "Please specify a valid API key.";

#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Invalid Lightship API Key", message, "OK");
                EditorApplication.isPlaying = false;
                AuthConfigHelper.ShowHelperWindow();
#endif
                throw new InvalidLightshipApiKeyException(lightshipApiKey);
            }
        }

        [Conditional("NOT_UNITY_EDITOR")]
        private static void EnsureValidApiKeyInGame(string lightshipApiKey)
        {
            if (IsInvalidApiKey(lightshipApiKey))
            {
                throw new InvalidLightshipApiKeyException(lightshipApiKey);
            }
        }
    }
}
