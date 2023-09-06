// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using TMPro;

namespace Niantic.Lightship.Maps.SampleAssets.Experimental.Labels.Editor
{
    /// <summary>
    /// This internal, Editor-only interface exposes serialized fields
    /// and other methods that are meant to be used to modify internal
    /// state of serialized builder assets programmatically.
    /// </summary>
    internal interface ITextMeshLabelWritable
    {
        /// <summary>
        /// The text field of a TextMeshPro component
        /// </summary>
        TMP_Text TextField { set; }
    }
}

#endif
