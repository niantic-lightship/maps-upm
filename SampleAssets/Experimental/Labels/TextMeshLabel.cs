// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Experimental.Labels
{
    /// <summary>
    /// This concrete <see cref="LabelObject"/> type uses a
    /// TextMeshPro component to display the label's text string.
    /// </summary>
    [PublicAPI]
    public partial class TextMeshLabel : LabelObject
    {
        [SerializeField]
        private TMP_Text _textField;

        /// <inheritdoc />
        public override void Initialize(string labelText, IMapTile parentTile)
        {
            base.Initialize(labelText, parentTile);
            _textField.text = labelText;
        }
    }
}
