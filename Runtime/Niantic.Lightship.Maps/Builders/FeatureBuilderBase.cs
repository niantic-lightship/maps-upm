// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Extensions;
using Niantic.Lightship.Maps.Core.Features;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders
{
    /// <inheritdoc cref="IFeatureBuilder" />
    [PublicAPI]
    public abstract partial class FeatureBuilderBase : MonoBehaviour, IFeatureBuilder
    {
        [Tooltip("The builder's name (for display purposes only).")]
        [SerializeField]
        private string _builderName;

        [Tooltip("The minimum zoom level for this builder.  This " +
            "field and MaxLOD define the range of maptile zoom " +
            "levels in which this builder will be active.")]
        [Range(0, 17)]
        [SerializeField]
        private int _minLOD;

        [Tooltip("The maximum zoom level for this builder.  This " +
            "field and MaxLOD define the range of maptile zoom " +
            "levels in which this builder will be active.")]
        [Range(0, 17)]
        [SerializeField]
        private int _maxLOD = 17;

        [Tooltip("An offset that can be used to avoid z-fighting")]
        [SerializeField]
        private float _zOffset;

        [SerializeField]
        [HideInInspector]
        private LayerKind _mapLayer = LayerKind.Undefined;

        [SerializeField]
        [HideInInspector]
        private List<FeatureKind> _features = new();

        /// <summary>
        /// Gets the builder's serialized name, if set, otherwise the
        /// returned string is the name of the builder's asset file.
        /// </summary>
        protected string BuilderName => _builderName.NullIfEmptyOrWhitespace() ?? name;

        protected LayerKind Layer => _mapLayer;
        protected List<FeatureKind> Features => _features;

        /// <summary>
        /// An offset that can be used to avoid z-fighting
        /// </summary>
        protected Vector3 ZOffset { get; private set; }

        /// <inheritdoc />
        public Guid Id { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public int MinLOD => _minLOD;

        /// <inheritdoc />
        public int MaxLOD => _maxLOD;

        /// <summary>
        /// Builders may optionally implement this method if they need
        /// to perform any initialization or might need access to their
        /// parent <see cref="ILightshipMapView"/> instance later on.
        /// </summary>
        /// <param name="lightshipMapView">The map to which this builder belongs</param>
        public virtual void Initialize(ILightshipMapView lightshipMapView)
        {
            ZOffset = Vector3.up * _zOffset;
        }
    }
}
