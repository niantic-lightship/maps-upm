// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if !UNITY_EDITOR
#define NOT_UNITY_EDITOR
#endif

using System;
using System.Diagnostics;
using Niantic.Platform.Debugging;
using Niantic.Platform.Debugging.Unity;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.Common
{
    /// <summary>
    /// This class is used to initialize the Platform logging system at runtime.
    /// </summary>
    internal static class RuntimeLogInitializer
    {
        /// <summary>
        /// Initialize the Platform logging system before the scene is loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            InitializeUnityLogStream();
        }

        /// <summary>
        /// This method should only be called if we're not running from the Unity
        /// Editor, since EditorLogInitializer will have already initialized the
        /// Platform logging system when the Editor was started.
        /// </summary>
        [Conditional("NOT_UNITY_EDITOR")]
        private static void InitializeUnityLogStream()
        {
            var logStream = new UnityLogStream();
            LogService.RegisterLogStream(logStream);
            LogService.MaxLogLevel = LogLevel.Verbose;
        }
    }
}
