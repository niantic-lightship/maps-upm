// Copyright © 2023 Niantic, Inc. All Rights Reserved.

using System;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Jobs
{
    /// <summary>
    /// A <see cref="JobHandleController"/> with additional
    /// callbacks for Completion, Cancellation, and Disposal.
    /// </summary>
    internal class CallbackJobHandleController : JobHandleController
    {
        private Action _onCompleted;
        private Action _onCancelled;
        private Action _onDisposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle">The <see cref="JobHandle"/> to manage</param>
        /// <param name="maxAge">The max age, in frames, that the Job is
        /// permitted to run prior to being forced to Complete.</param>
        /// <param name="onCompleted">Called when the Job successfully Completes</param>
        /// <param name="onDisposed">Called when the Job is Disposed</param>
        /// <param name="onCancelled">Called when the Job is Cancelled</param>
        public CallbackJobHandleController(
            JobHandle handle,
            int maxAge,
            Action onCompleted,
            Action onDisposed = null,
            Action onCancelled = null)
            : base(handle, maxAge)
        {
            _onCompleted = onCompleted;
            _onDisposed = onDisposed;
            _onCancelled = onCancelled;
        }

        /// <inheritdoc />
        protected override void OnCompleted()
        {
            _onCompleted?.Invoke();
            _onCompleted = null;
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            _onDisposed?.Invoke();
            _onDisposed = null;
        }

        /// <inheritdoc />
        protected override void OnCancel()
        {
            _onCancelled?.Invoke();
            _onCancelled = null;
        }
    }
}
