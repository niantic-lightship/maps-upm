// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using UnityObject = UnityEngine.Object;

namespace Niantic.Lightship.Maps.ObjectPools
{
    /// <summary>
    /// A container type that acts as a handle to a live
    /// object managed by an <see cref="ObjectPool{T}"/>
    /// </summary>
    [PublicAPI]
    public readonly struct PooledObject<T> : IDisposable where T : UnityObject
    {
        private readonly IPooledObjectAccessor<T> _objectPool;
        private readonly long _id;

        /// <summary>
        /// Gets the pooled object's value
        /// </summary>
        public T Value => _objectPool.GetValue(_id);

        /// <summary>
        /// Checks whether this <see cref="PooledObject{T}"/> handle
        /// points to an object that's still alive.  This can be useful
        /// to check before accessing the pooled object through the
        /// <see cref="Value"/> property if this pooled object handle
        /// may have been disposed elsewhere.
        /// </summary>
        public bool IsAlive => _objectPool.IsAlive(_id);

        /// <summary>
        /// Releases this object back to the pool.  Accessing the
        /// <see cref="Value"/> property after <see cref="Dispose"/> is
        /// called will throw an <see cref="ObjectDisposedException"/>.
        /// </summary>
        public void Dispose() => _objectPool.Release(_id);

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="id">A unique id for this instance</param>
        /// <param name="objectPool">An interface allowing access to the pool</param>
        internal PooledObject(long id, IPooledObjectAccessor<T> objectPool)
        {
            _objectPool = objectPool;
            _id = id;
        }
    }
}
