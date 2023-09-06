// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.SampleAssets.Cameras.OrbitCamera.Internal.Input
{
    [Serializable]
    internal readonly struct TransformData
    {
        /// <summary>
        /// Id for this positional info. For example, each finger of a touch
        /// event or each mouse button would have different, consistent Ids.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The position, if any, that this input occured at. Often in screen space,
        /// but since the space is assigned by the IInputSource, it may not be.
        /// </summary>
        public readonly Vector3 Position;

        public TransformData(int id, Vector3 position)
        {
            Id = id;
            Position = position;
        }
    }
}
