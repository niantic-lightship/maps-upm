// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.IO;
using Niantic.Lightship.Maps.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Niantic.Lightship.Maps.Samples.MapThemes.SceneSwitcher
{
    internal class SceneSwitcher : MonoBehaviour
    {
        private const string MainScene = "MapThemes";
        private const string OrbitScene = "OrbitCamera";
        private const string OrthographicScene = "OrthographicCamera";

        private static ChannelLogger Log { get; } = new(nameof(SceneSwitcher));

        private void Start()
        {
            var activeScene = SceneManager.GetActiveScene();

            if (activeScene.name == MainScene)
            {
                ChangeScene();
            }
        }

        public void ChangeScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            var currentSceneDir = Path.GetDirectoryName(activeScene.path);

            var newScenePath = activeScene.name switch
            {
                OrthographicScene => $"{currentSceneDir}/{OrbitScene}.unity",
                OrbitScene => $"{currentSceneDir}/{OrthographicScene}.unity",
                MainScene => $"{currentSceneDir}/Scenes/{OrthographicScene}.unity",
                _ => throw new ArgumentOutOfRangeException(nameof(activeScene.name))
            };

            var sceneIndex = SceneUtility.GetBuildIndexByScenePath(newScenePath);

            if (sceneIndex >= 0)
            {
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                var message = $"Couldn't find scene '{newScenePath}' in build settings";
#if UNITY_EDITOR
                Log.Info(message);
                EditorSceneManager.LoadSceneInPlayMode(newScenePath, new LoadSceneParameters());
#else
                Log.Error(message);
#endif
            }
        }
    }
}
