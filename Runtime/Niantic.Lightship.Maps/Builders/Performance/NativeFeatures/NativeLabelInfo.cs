// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Features;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Builders.Performance.NativeFeatures
{
    /// <inheritdoc cref="ILabelInfo"/>
    [PublicAPI]
    [BurstCompile]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct NativeLabelInfo : INativeDisposable
    {
        /// <inheritdoc cref="ILabelInfo.Priority"/>
        public readonly int Priority;

        /// <inheritdoc cref="ILabelInfo.MinZoom"/>
        public readonly int MinZoom;

        /// <inheritdoc cref="ILabelInfo.MaxZoom"/>
        public readonly int MaxZoom;

        /// <inheritdoc cref="ILabelInfo.PosX"/>
        public readonly float PosX;

        /// <inheritdoc cref="ILabelInfo.PosY"/>
        public readonly float PosY;

        /// <inheritdoc cref="ILabelInfo.Text"/>
        public UnsafeList<byte> Text { get; }

        private readonly ulong _textHandle;

        internal NativeLabelInfo(
            int priority,
            int minZoom,
            int maxZoom,
            float posX,
            float posY,
            in UnsafeList<byte> text,
            ulong textHandle)
        {
            Priority = priority;
            MinZoom = minZoom;
            MaxZoom = maxZoom;
            PosX = posX;
            PosY = posY;
            Text = text;
            _textHandle = textHandle;
        }

        /// <summary>
        /// Disposes this feature's Native Collections
        /// and releases any resources.
        /// </summary>
        public void Dispose()
        {
            if (_textHandle != 0)
            {
                UnsafeUtility.ReleaseGCObject(_textHandle);
            }

            Text.Dispose();
        }

        /// <summary>
        /// Schedules a Job which disposes this feature's
        /// Native Collections and releases any resources.
        /// </summary>
        public JobHandle Dispose(JobHandle dependencies)
        {
            var disposeJobHandle = new DisposeJob { TextHandle = _textHandle }.Schedule(dependencies);
            var textDisposeHandle = Text.Dispose(dependencies);

            return JobHandle.CombineDependencies(disposeJobHandle, textDisposeHandle);
        }

        [BurstCompile]
        private struct DisposeJob : IJob
        {
            public ulong TextHandle;

            public void Execute()
            {
                if (TextHandle != 0)
                {
                    UnsafeUtility.ReleaseGCObject(TextHandle);
                }
            }
        }
    }
}
