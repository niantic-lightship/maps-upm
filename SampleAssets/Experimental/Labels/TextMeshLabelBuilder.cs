// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;

namespace Niantic.Lightship.Maps.SampleAssets.Experimental.Labels
{
    /// <summary>
    /// A builder derived from <see cref="LabelBuilder{T}"/>,
    /// where the <see cref="LabelObject"/> being built is
    /// of type <see cref="TextMeshLabel"/>.
    /// </summary>
    [PublicAPI]
    public class TextMeshLabelBuilder : LabelBuilder<TextMeshLabel>
    {
    }
}
