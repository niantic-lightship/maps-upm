// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample.FloatingText
{
    internal class FloatingText : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _textField;

        private void Start()
        {
            // Destroy self after it's finished anim
            Destroy(gameObject, 2.0f);
        }

        public void SetText(string text)
        {
            _textField.text = text;
        }
    }
}
