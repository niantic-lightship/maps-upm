// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures
{
    /// <summary>
    /// A single unmanaged value
    /// </summary>
    /// <remarks>Functionally equivalent to an array of length 1.
    /// When you need just one value, UnsafeReference can be preferable to an
    /// <see cref="UnsafeList{T}"/> because it better conveys the intent</remarks>
    /// <typeparam name="T">The type of value</typeparam>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeReference<T> :
        INativeDisposable,
        IEquatable<UnsafeReference<T>>
        where T : unmanaged, IEquatable<T>
    {
        private UnsafeList<T> _backingList;
        private readonly ulong _arrayHandle;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialValue">The initial value to store</param>
        public UnsafeReference(T initialValue)
        {
            var array = new T[1];
            array[0] = initialValue;
            var buffer = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out _arrayHandle);
            _backingList = new UnsafeList<T>((T*)buffer, 1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialValue">The initial value to store</param>
        public UnsafeReference(ref T initialValue)
        {
            var array = new T[1];
            array[0] = initialValue;
            var buffer = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out _arrayHandle);
            _backingList = new UnsafeList<T>((T*)buffer, 1);
        }

        public T Value
        {
            get { return _backingList[0]; }
            set { _backingList[0] = value; }
        }

        #region INativeDisposable

        /// <summary>
        /// Releases resources used by this <see cref="UnsafeReference{T}"/>
        /// </summary>
        public void Dispose()
        {
            _backingList.Dispose();
            UnsafeUtility.ReleaseGCObject(_arrayHandle);
        }

        /// <summary>
        /// Schedules a Job which releases any resources
        /// </summary>
        public JobHandle Dispose(JobHandle dependencies)
        {
            var disposeJob = new DisposeJob { ArrayHandle = _arrayHandle }.Schedule(dependencies);

            return _backingList.Dispose(disposeJob);
        }

        [BurstCompile]
        private struct DisposeJob : IJob
        {
            public ulong ArrayHandle;

            public void Execute()
            {
                UnsafeUtility.ReleaseGCObject(ArrayHandle);
            }
        }

        #endregion
        #region IEquatable<T>

        public bool Equals(UnsafeReference<T> other)
        {
            return _backingList[0].Equals(other._backingList[0]);
        }

        public override bool Equals(object obj)
        {
            return obj is UnsafeReference<T> other && other._backingList[0].Equals(_backingList[0]);
        }

        public override int GetHashCode()
        {
            return _backingList[0].GetHashCode();
        }

        #endregion
    }
}
