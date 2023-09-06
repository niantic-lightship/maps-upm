// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.Common.Editor
{
    /// <summary>
    /// This class checks whether the current project's render pipeline
    /// is compatible with a given sample from the Maps SDK.
    /// </summary>
    internal static class RenderPipelineChecker
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CheckRenderPipeline()
        {
            if (QualitySettings.renderPipeline == null)
            {
                const string title = "Invalid Render Pipeline";
                const string message = "The current project is using the built-in render pipeline.  Please use the Universal Render Pipeline (URP) instead.";

                EditorUtility.DisplayDialog(title, message, "OK");
            }
        }
    }
}

#endif
