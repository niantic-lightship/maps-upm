// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityObject = UnityEngine.Object;

namespace Niantic.Lightship.Maps.ObjectPools
{
    /// <summary>
    /// This internal interface is meant to allow instances of <see cref="PooledObject{T}"/> to
    /// access the <see cref="ObjectPool{T}"/> that they're managed by.  It's intentionally as
    /// narrowly-defined as possible, and is implemented explicitly in an effort to prevent these
    /// methods from being called by anything other than a <see cref="PooledObject{T}"/>.
    /// </summary>
    /// <typeparam name="T">The pooled objects' derived type</typeparam>
    internal interface IPooledObjectAccessor<out T> where T : UnityObject
    {
        /// <summary>
        /// Checks whether a pooled object is still alive
        /// </summary>
        /// <param name="id">The pooled object's id</param>
        /// <returns>True if the object is still alive</returns>
        bool IsAlive(long id);

        /// <summary>
        /// Gets a pooled object by its unique id
        /// </summary>
        /// <param name="id">The pooled object's id</param>
        /// <returns>The pooled object</returns>
        T GetValue(long id);

        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        /// <param name="id">The pooled object's id</param>
        void Release(long id);
    }
}
