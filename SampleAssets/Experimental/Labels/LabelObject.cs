// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Experimental.Labels
{
    /// <summary>
    /// The base type from which all instances of labels placed on the map are derived.
    /// </summary>
    [PublicAPI]
    public abstract class LabelObject : MonoBehaviour
    {
        private double _scale;
        private double _clampedScale;

        /// <summary>
        /// The <see cref="IMapTile"/> that this label is associated with.
        /// </summary>
        public IMapTile ParentTile { get; private set; }

        /// <summary>
        /// Sets the label's values when it's being
        /// instantiated and placed on the map.
        /// </summary>
        /// <param name="labelText">The label's text string.</param>
        /// <param name="parentTile">The label's parent maptile.</param>
        public virtual void Initialize(string labelText, IMapTile parentTile)
        {
            ParentTile = parentTile;
        }

        /// <summary>
        /// This method is used to set the label's scale values that are
        /// viewable from the Inspector in debug mode.  These values can
        /// be useful when choosing values for a label builder's scaling
        /// factor and min and max scale.
        /// </summary>
        /// <param name="scale">The raw scale calculated by the builder</param>
        /// <param name="clampedScale">The scale value after being clamped</param>
        [Conditional("UNITY_EDITOR")]
        internal void SetScaleForInspector(double scale, double clampedScale)
        {
            _scale = scale;
            _clampedScale = clampedScale;
        }
    }
}
