// Copyright © 2023 Niantic, Inc. All Rights Reserved.

using System;
using Unity.Jobs;

namespace Niantic.Lightship.Maps.Jobs
{
    /// <summary>
    /// Interface representing behavior for a given JobHandle's lifecycle events
    /// </summary>
    internal interface IJobHandleController : IDisposable
    {
        /// <summary>
        /// Callback called when Job is Completed
        /// </summary>
        event Action OnCompletedEvent;

        /// <summary>
        /// Whether or not the Job has completed
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Immediately completes the Job
        /// </summary>
        void Complete();

        /// <summary>
        /// Attempts to Complete the Job, if it is ready
        /// </summary>
        /// <returns>True, if the Job was successfully completed, otherwise false</returns>
        bool TryComplete(int age = 0);

        /// <summary>
        /// Cancels the Job by Completing it but not calling any completion events
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// Class representing behavior for a given JobHandle's lifecycle events
    /// </summary>
    internal class JobHandleController : IJobHandleController
    {
        private JobHandle _jobHandle;

        /// <inheritdoc />
        public event Action OnCompletedEvent;

        /// <inheritdoc />
        public bool IsCompleted => _jobHandle.IsCompleted;

        private readonly int _maxAge;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle">The JobHandle to manage</param>
        /// <param name="maxAge">The max age, in frames, that the Job is permitted to run
        /// prior to being forced to Complete</param>
        // ReSharper disable once MemberCanBeProtected.Global
        public JobHandleController(JobHandle handle, int maxAge = int.MaxValue)
        {
            _jobHandle = handle;
            _maxAge = maxAge;
        }

        /// <summary>
        /// Immediately completes the Job
        /// </summary>
        public void Dispose()
        {
            _jobHandle.Complete();
            OnDispose();
        }

        /// <inheritdoc />
        public void Complete()
        {
            _jobHandle.Complete();
            OnCompleted();
            OnCompletedEvent?.Invoke();
            OnCompletedEvent = null;
            Dispose();
        }

        /// <inheritdoc />
        public void Cancel()
        {
            _jobHandle.Complete();
            OnCancel();
        }

        /// <inheritdoc />
        public bool TryComplete(int currentAge = 0)
        {
            if (!_jobHandle.IsCompleted && currentAge < _maxAge)
            {
                return false;
            }

            Complete();
            return true;
        }

        /// <summary>
        /// Called when the Job is successfully Completed
        /// </summary>
        protected virtual void OnCompleted()
        {
        }

        /// <summary>
        /// Called when the Job is Disposed
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        /// Called when the Job is Cancelled
        /// </summary>
        protected virtual void OnCancel()
        {
        }
    }
}
