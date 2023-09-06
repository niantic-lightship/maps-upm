// Copyright 2022 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using JetBrains.Annotations;
using Niantic.Platform.Debugging;
using Niantic.Platform.Debugging.Unity;
using UnityEditor;

namespace Niantic.Lightship.Maps.Samples.Common.Editor
{
    /// <summary>
    /// This class is used to initialize the Platform logging system before the
    /// Unity Editor runs any Editor scripts, which sends logged events through
    /// the Platform Logger in these cases. For more information, please see
    /// <see href="https://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html">
    /// Unity's documentation on running editor script code on launch.</see>
    /// </summary>
    [PublicAPI]
    [InitializeOnLoad]
    public class EditorLogInitializer
    {
        /// <summary>
        /// Executed once at startup
        /// </summary>
        static EditorLogInitializer()
        {
            var logStream = new UnityLogStream();
            LogService.RegisterLogStream(logStream);
            LogService.MaxLogLevel = LogLevel.Verbose;
        }
    }
}

#endif
