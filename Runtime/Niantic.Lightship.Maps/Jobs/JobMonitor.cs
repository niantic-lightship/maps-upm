// Copyright © 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Threading;
using Unity.Jobs;
using UnityEngine;

namespace Niantic.Lightship.Maps.Jobs
{
    /// <summary>
    /// JobMonitor MonoBehaviour
    /// </summary>
    internal class JobMonitor : MonoBehaviour
    {
        /// <summary>
        /// The Unity lifecycle stage in which
        /// JobHandle updates will occur.
        /// </summary>
        public enum UpdateMode
        {
            Update,
            LateUpdate
        }

        /// <summary>
        /// Called when the Job is Completed or the Monitor is destroyed
        /// </summary>
        public event Action OnComplete;

        /// <summary>
        /// Determines in which Unity lifecycle stage to perform
        /// JobHandle updates.
        /// </summary>
        public UpdateMode Mode { get; private set; }

        /// <summary>
        /// The current age of the monitored Job, in Unity frames
        /// </summary>
        private int _age;

        /// <summary>
        /// The <see cref="IJobHandleController"/> managing the <see cref="JobHandle"/>
        /// </summary>
        private IJobHandleController _jobHandleController;

        /// <summary>
        /// Used to cancel the Job
        /// </summary>
        /// <remarks>
        /// When <see cref="CancellationToken.IsCancellationRequested"/>
        /// is set, <see cref="JobHandle.Complete"/> is called, but any
        /// OnComplete callbacks in the <see cref="JobMonitor"/> or
        /// <see cref="IJobHandleController"/> are ignored.
        /// </remarks>
        private CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes the <see cref="JobMonitor"/>
        /// </summary>
        /// <param name="jobHandleController">
        /// The <see cref="IJobHandleController"/> managing the <see cref="JobHandle"/>.
        /// </param>
        /// <param name="updateMode">Determines in which Unity
        /// lifecycle stage to perform JobHandle updates.</param>
        /// <param name="token">Used to cancel the Job</param>
        /// <remarks>
        /// When <see cref="CancellationToken.IsCancellationRequested"/>
        /// is set, <see cref="JobHandle.Complete"/> is called, but any
        /// OnComplete callbacks in the <see cref="JobMonitor"/> or
        /// <see cref="IJobHandleController"/> are ignored.
        /// </remarks>
        public void Initialize(
            IJobHandleController jobHandleController,
            UpdateMode updateMode,
            CancellationToken token = default)
        {
            _jobHandleController = jobHandleController;
            Mode = updateMode;
            _cancellationToken = token;
        }

        #region Unity Methods

        private void Update()
        {
            if (Mode == UpdateMode.Update)
            {
                OnUpdate();
            }
        }

        private void LateUpdate()
        {
            if (Mode == UpdateMode.LateUpdate)
            {
                OnUpdate();
            }
        }

        #endregion

        /// <summary>
        /// Calls <see cref="IJobHandleController.TryComplete"/> and
        /// <see cref="GameObject.Destroy(UnityEngine.Object)"/>s the
        /// <see cref="JobMonitor"/> if successful.
        /// </summary>
        /// <param name="age">The current age, in Unity frames,
        /// of the managed <see cref="JobHandle"/>.</param>
        private void TryComplete(int age = 0)
        {
            var destroy = true;
            if (_jobHandleController != null)
            {
                destroy = _jobHandleController.TryComplete(age);
            }

            if (destroy)
            {
                OnComplete?.Invoke();
                OnComplete = null;
                Destroy(this);
            }
        }

        private void OnUpdate()
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                Cancel();
                return;
            }

            TryComplete(_age++);
        }

        private void Cancel()
        {
            _jobHandleController?.Cancel();
            _jobHandleController = null;
            Destroy(this);
        }

        private void OnDestroy()
        {
            _jobHandleController?.Dispose();
            OnComplete?.Invoke();
            OnComplete = null;
        }
    }
}
