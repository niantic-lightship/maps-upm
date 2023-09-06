// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.ExtensionMethods;
using Niantic.Lightship.Maps.Utilities;
using UnityObject = UnityEngine.Object;

namespace Niantic.Lightship.Maps.ObjectPools
{
    /// <summary>
    /// A general-purpose object pool for types derived from Unity's <see cref="UnityObject"/>
    /// </summary>
    /// <typeparam name="T">The pool's derived type</typeparam>
    [PublicAPI]
    public class ObjectPool<T> : IPooledObjectAccessor<T> where T : UnityObject
    {
        private readonly Dictionary<long, T> _liveObjects = new();
        private readonly Queue<T> _pool = new();
        private readonly Action<T> _onCreate;
        private readonly Action<PooledObject<T>> _onAcquire;
        private readonly Action<T> _onRelease;
        private readonly T _source;
        private long _objectId;

        private static ChannelLogger Log { get; } = new(nameof(ObjectPool<T>));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">The object to be duplicated in the pool.</param>
        /// <param name="onCreate">An action invoked on a newly
        /// instantiated object when it is first created.</param>
        /// <param name="onAcquire">An action invoked on a new or reused object
        /// when it is acquired, before it is returned to the caller.</param>
        /// <param name="onRelease">An action invoked on an object when it is released.</param>
        /// <exception cref="ArgumentNullException">Thrown if source is null.</exception>
        public ObjectPool(
            T source,
            Action<T> onCreate = null,
            Action<PooledObject<T>> onAcquire = null,
            Action<T> onRelease = null)
        {
            _source = source.IsReferenceNotNull() ? source : throw new ArgumentNullException(nameof(source));
            _onCreate = onCreate;
            _onRelease = onRelease;
            _onAcquire = onAcquire;
        }

        #region IPooledObjectAccessor

        /// <inheritdoc />
        bool IPooledObjectAccessor<T>.IsAlive(long id) => _liveObjects.ContainsKey(id);

        /// <inheritdoc />
        T IPooledObjectAccessor<T>.GetValue(long id)
        {
            VerifyObjectIsAlive(id, throwIfDisposed: true);
            return _liveObjects[id];
        }

        /// <inheritdoc />
        void IPooledObjectAccessor<T>.Release(long id)
        {
            VerifyObjectIsAlive(id, throwIfDisposed: false);

            if (_liveObjects.Remove(id, out var value))
            {
                _onRelease?.Invoke(value);
                _pool.Enqueue(value);
            }
        }

        #endregion

        /// <summary>
        /// Gets an object in the Pool if one is available.  Otherwise,
        /// returns a new instance of the object and calls onCreate() on it.
        /// </summary>
        /// <returns>An object in the Pool, if one is available.
        /// Otherwise, a new instance of the object.</returns>
        public PooledObject<T> GetOrCreate()
        {
            if (!_pool.TryDequeue(out var instance))
            {
                // If we couldn't reuse an instance of an object
                // that has been released, instantiate a new one
                instance = UnityObject.Instantiate(_source);
                _onCreate?.Invoke(instance);
            }

            // Add the instance to our live object table
            var id = Interlocked.Increment(ref _objectId);
            _liveObjects[id] = instance;

            // Initialize our new or reused instance
            var pooledObject = new PooledObject<T>(id, this);
            _onAcquire?.Invoke(pooledObject);

            return pooledObject;
        }

        /// <summary>
        /// Checks whether an object with a given id is still alive
        /// </summary>
        /// <param name="id">The object's unique id</param>
        /// <param name="throwIfDisposed">Whether to throw if the object is not alive</param>
        /// <exception cref="ObjectDisposedException">Thrown if the object's id isn't
        /// found in the live object list, which can happen if an object is accessed
        /// after its <see cref="PooledObject{T}"/> has been disposed.</exception>
        private void VerifyObjectIsAlive(long id, bool throwIfDisposed)
        {
            if (!_liveObjects.ContainsKey(id))
            {
                var message = $"Pooled object '{id}' has already been disposed";
                Log.Error(message);

                if (throwIfDisposed)
                {
                    throw new ObjectDisposedException(message);
                }
            }
        }
    }
}
