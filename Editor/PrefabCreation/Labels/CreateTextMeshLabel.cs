// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.SampleAssets.Experimental.Labels;
using Niantic.Lightship.Maps.SampleAssets.Experimental.Labels.Editor;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.Editor.PrefabCreation.Labels
{
    /// <summary>
    /// Creates a <see cref="TextMeshLabel"/> prefab for
    /// use with <see cref="TextMeshLabelBuilder"/>s.
    /// </summary>
    internal class CreateTextMeshLabel : CreatePrefabBase<TextMeshLabel>
    {
        /// <inheritdoc />
        protected override void InitializeComponents(
            GameObject rootObject, TextMeshLabel textMeshLabel)
        {
            base.InitializeComponents(rootObject, textMeshLabel);
            ITextMeshLabelWritable label = textMeshLabel;

            // Create a child with a TextMeshPro component
            var textMeshObject = new GameObject("LabelText");
            var textComponent = textMeshObject.AddComponent<TextMeshPro>();
            textMeshObject.transform.SetParent(rootObject.transform, false);

            textComponent.text = "Label Text";
            textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
            textComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            textComponent.transform.Rotate(Vector3.right, 90);
            label.TextField = textComponent;
        }
    }
}
