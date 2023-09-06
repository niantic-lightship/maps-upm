// Copyright 2023 Niantic, Inc. All Rights Reserved.

#if UNITY_EDITOR

using System;
using Niantic.Lightship.Maps.SampleAssets.Experimental.Labels.Editor;
using TMPro;

// ReSharper disable once CheckNamespace

namespace Niantic.Lightship.Maps.SampleAssets.Experimental.Labels
{
    /// <inheritdoc cref="ITextMeshLabelWritable" />
    public partial class TextMeshLabel : ITextMeshLabelWritable
    {
        /// <inheritdoc />
        TMP_Text ITextMeshLabelWritable.TextField { set => _textField = value; }
    }
}

#endif
