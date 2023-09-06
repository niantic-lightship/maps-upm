// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Jobs;
using Niantic.Lightship.Maps.Builders.Performance.Utils;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Builders.Standard.LinearFeatures;
using Niantic.Lightship.Maps.Core;
using Niantic.Lightship.Maps.Core.Coordinates;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Core.Utilities;
using Niantic.Lightship.Maps.Jobs;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using Niantic.Lightship.Maps.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using IntReference = Niantic.Lightship.Maps.Builders.Performance.NativeFeatures.UnsafeReference<int>;
using LinearFeatureSet = Niantic.Lightship.Maps.Builders.Performance.LinearFeatures.Structs.LinearFeatureSet;

namespace Niantic.Lightship.Maps.Builders.Performance.LinearFeatures
{
    /// <summary>
    /// A builder for <see cref="ILinearFeature"/>s
    /// </summary>
    [PublicAPI]
    public partial class LinearFeatureBuilderAsync : MeshBuilderAsync
    {
        // Knobs to adjust smooth linear feature building
        [Header("Smooth Linear Feature Knobs")]
        [Tooltip("Linear feature end cap vertex count.")]
        [Range(1, 8)]
        [SerializeField]
        private int _endCapPointCount = 4;

        // 1 is straight line, 0.85 = 31.8 degree deviation ... if bigger then insert smoothing points
        [Tooltip("Linear Feature - insert extra points when bend is more than cos(x degree)")]
        [Range(0.7f, 0.9f)]
        [SerializeField]
        private float _bendThreshold = 0.85f;

        // Sharpness of inserted curve (0.25 - fairly smooth, 0.15 - sharper turns) see algorithm below
        [Tooltip("Linear feature smoothness factor. (0.1 - sharp -> 0.25 smooth turn)")]
        [Range(0.1f, 0.25f)]
        [SerializeField]
        private float _smoothFactor = 0.15f;

        #region Linear feature size values

        [SerializeField]
        [HideInInspector]
        private LinearFeatureSize _linearFeatureSize;

        [SerializeField]
        [HideInInspector]
        private float _customLinearFeatureMin;

        [SerializeField]
        [HideInInspector]
        private float _customLinearFeatureMax;

        private float _linearFeatureWidthMin;
        private float _linearFeatureWidthMax;
        private float _linearFeatureWidth;

        private double _linearFeatureWidthBaseSize;

        #endregion

        private static ChannelLogger Log { get; } = new(nameof(LinearFeatureBuilderAsync));

        /// <inheritdoc />
        public override void Build(IReadOnlyList<MeshTile> tiles)
        {
            var tileCount = tiles.Count;

            // Calculates the linear feature width on each rebuild.
            // Only needs to be done once with any given tile
            var firstTile = tiles[0];
            _linearFeatureWidth = CalculateLinearFeatureWidth(firstTile.Tile.ZoomLevel, firstTile.Tile.Size);

            try
            {
                // Grab ILinearFeatures for each tile and convert them into Native (Job-friendly) representations
                var nativeFeaturesByTile = GetFeaturesByTile<ILinearFeature>(tiles).ToNative();

                // Spin off a new Build Task for each tile
                for (int i = 0; i < tileCount; i++)
                {
                    BuildLinearFeatureMesh(tiles[i], nativeFeaturesByTile[i]);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public override void Initialize(ILightshipMapView lightshipMapView)
        {
            base.Initialize(lightshipMapView);
            _linearFeatureWidthBaseSize = Math.Pow(2, WebMercator12.ZOOM_LEVEL - MaxLOD);
        }

        // Represents the entire build process for all of the ILinearFeatures in a single tile
        private void BuildLinearFeatureMesh(MeshTile tile, NativeArray<LinearFeatureSet> features)
        {
            try
            {
                var token = tile.Token;

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    features.CleanUp();
                    token.ThrowIfCancellationRequested();
                }

                // If there are no valid features for this tile, cancel the build process
                if (features.Length == 0)
                {
                    features.CleanUp();
                    throw new OperationCanceledException();
                }

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    features.CleanUp();
                    token.ThrowIfCancellationRequested();
                }

                // Schedules a series of Jobs which calculate the linear
                // feature mesh shapes based on the ILinearFeature's shape and
                // configurable parameters (such as linear feature thickness).
                var (prepareLinearFeatureHandle, linearFeatureMeshDataArray) = PrepareLinearFeatures(features);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    prepareLinearFeatureHandle.Complete();
                    linearFeatureMeshDataArray.Dispose();
                    token.ThrowIfCancellationRequested();
                }

                // Schedules a Job which combines all of the linear feature
                // meshes for all of this tile's ILinearFeatures into a single mesh.
                var (combineJobHandle, tileStruct) = CombineMeshes(prepareLinearFeatureHandle, linearFeatureMeshDataArray);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    combineJobHandle.Complete();
                    tileStruct.CombinedFeatureMeshes.Dispose();
                    tileStruct.Dispose();
                    token.ThrowIfCancellationRequested();
                }

                // Sends the Job to a JobMonitor to be
                // completed during a future LateUpdate frame.
                var monitor = tile.Monitor.CreateJobMonitor(this);
                var jobHandleController = new CallbackJobHandleController(
                    handle: combineJobHandle,
                    maxAge: JobConstants.TempJobMaxAge,
                    onCompleted: () =>
                    {
                        // When the meshes are built (i.e., the Job is completed),
                        // apply the mesh to the MapTileObject's MeshFilter.
                        ApplyMesh(tile, tileStruct, token);
                        tile.MarkBuilderComplete(this);
                    },
                    onDisposed: () => { tileStruct.Dispose(); },
                    onCancelled: () =>
                    {
                        // If the build process was cancelled early, then Dispose
                        // of any NativeCollections that were created for this tile.
                        tileStruct.CombinedFeatureMeshes.Dispose();
                        tileStruct.Dispose();
                        Release(tile.Filter);
                        SetMeshForFilter(null, tile.Filter);
                        tile.MarkBuilderComplete(this);
                    });

                monitor.Initialize(jobHandleController, JobMonitor.UpdateMode.LateUpdate, token);
            }
            // If the build process was cancelled due to another IMapTile
            // needing to be built on the IMapTileObject, then clear and
            // free any resources that were allocated for this tile.
            catch (OperationCanceledException)
            {
                var monitor = tile.Monitor?.GetMonitor(this);
                if (monitor != null)
                {
                    Destroy(monitor);
                }

                Release(tile.Filter);
                SetMeshForFilter(null, tile.Filter);
                tile.MarkBuilderComplete(this);
            }
            catch (Exception e)
            {
                tile.MarkBuilderComplete(this);
                Log.Error(e.Message);
                throw;
            }
        }

        #region Mesh Building Stages

        /// <summary>
        /// Starts a series of Jobs which calculate the liner feature mesh shapes based on the
        /// ILinearFeature's shape and configurable parameters (such as linear feature thickness).
        /// </summary>
        /// <param name="currentTilesFeatures">The set of <see cref="LinearFeatureSet"/>s
        /// representing the <see cref="ILinearFeature"/>s of the current tile</param>
        /// <returns>A <see cref="Mesh.MeshDataArray"/> containing the meshes for
        /// all of the <see cref="ILinearFeature"/>s of the current tile.</returns>
        private (JobHandle handle, Mesh.MeshDataArray linearFeatureMeshes) PrepareLinearFeatures(
            NativeArray<LinearFeatureSet> currentTilesFeatures)
        {
            var currentLinearFeatureCount = currentTilesFeatures.Length;

            // Allocates a MeshDataArray to contain each feature's mesh
            var linearFeatureMeshDataArr = Mesh.AllocateWritableMeshData(currentLinearFeatureCount);

            var appraiseJobHandles = new NativeArray<JobHandle>(currentLinearFeatureCount, Allocator.TempJob);

            // Determine how many verts and triangles each feature needs
            for (int i = 0; i < currentLinearFeatureCount; i++)
            {
                var currentVertexCount = new IntReference(0);
                var currentIndexCount = new IntReference(0);

                var appraiseMeshJob = new AppraiseMeshJob(
                    currentTilesFeatures[i],
                    currentVertexCount,
                    currentIndexCount,
                    _smoothFactor,
                    _endCapPointCount,
                    _bendThreshold);

                var createMeshJob = new CreateMeshJob(
                    currentVertexCount,
                    currentIndexCount,
                    linearFeatureMeshDataArr,
                    i,
                    _smoothFactor,
                    _endCapPointCount,
                    _bendThreshold,
                    _linearFeatureWidth,
                    currentTilesFeatures[i]);

                // Schedules all of the above jobs and chains them together
                var appraiseMeshJobHandle = appraiseMeshJob.Schedule();
                var createMeshJobHandle = createMeshJob.Schedule(appraiseMeshJobHandle);
                var disposeFeatureHandle = currentTilesFeatures[i].Dispose(createMeshJobHandle);
                var disposeVertexCount = currentVertexCount.Dispose(createMeshJobHandle);
                var disposeIndexCount = currentIndexCount.Dispose(createMeshJobHandle);

                appraiseJobHandles[i] =
                    JobHandle.CombineDependencies(disposeFeatureHandle, disposeVertexCount, disposeIndexCount);
            }

            // Disposes the features NativeArray when the above Jobs are completed
            var disposeFeaturesArrayHandle =
                currentTilesFeatures.Dispose(JobHandle.CombineDependencies(appraiseJobHandles));

            return (appraiseJobHandles.Dispose(disposeFeaturesArrayHandle), linearFeatureMeshDataArr);
        }

        /// <summary>
        /// Starts a Job which combines all of the linear feature meshes for
        /// all of this tile's <see cref="ILinearFeature"/>s into a single mesh.
        /// </summary>
        /// <param name="dependencies">Any <see cref="JobHandle"/>s
        /// for Jobs that need to be completed before this one.</param>
        /// <param name="allLinearFeatureMeshes">A <see cref="Mesh.MeshDataArray"/> containing
        /// the meshes for all of the <see cref="ILinearFeature"/>s of the current tile.</param>
        /// <returns> A tuple with the following components:
        /// <ul>
        /// <li><b>totalVertexCount</b> - The count of all vertices in the current tile's linear feature meshes</li>
        /// <li><b>totalIndexCount</b> - The count of all indices in the current tile's linear feature meshes</li>
        /// <li><b>allMeshesCombined</b> - A <see cref="Mesh.MeshDataArray"/> (of length 1) whose only element
        /// is the mesh of all of the current tile's features combined</li>
        /// </ul>
        /// </returns>
        private (JobHandle handle, MapTileBuildStruct tileStruct) CombineMeshes(
            JobHandle dependencies, Mesh.MeshDataArray allLinearFeatureMeshes)
        {
            var allMeshesCombined = Mesh.AllocateWritableMeshData(1);
            var combinedMeshData = allMeshesCombined[0];

            var vertexSubarraySizes = new NativeArray<int>(allLinearFeatureMeshes.Length, Allocator.TempJob);
            var indexSubarraySizes = new NativeArray<int>(allLinearFeatureMeshes.Length, Allocator.TempJob);

            // Creates NativeArrays with the feature and index counts of each feature
            var calculateSubarraySizesJob = new CalculateSubarraySizesJob
            (
                allLinearFeatureMeshes,
                vertexSubarraySizes,
                indexSubarraySizes
            );

            // A helper struct containing all the relevant
            // NativeContainers needed for the Build process.
            var tileStruct = new MapTileBuildStruct
            (
                default, vertexSubarraySizes,
                default, indexSubarraySizes,
                allLinearFeatureMeshes, allMeshesCombined
            );

            // Sets relevant vertex and index buffer
            // information on the final combinedMeshData.
            var prepareCombinedMeshJob = new PrepareCombinedMeshJob
            (
                combinedMeshData,
                vertexSubarraySizes,
                indexSubarraySizes
            );

            // Combines the individual feature meshes into a single mesh,
            // which is stored in the first/only element of combinedMeshData.
            var combineMeshesJob = new CombineMeshesJob
            {
                Input = allLinearFeatureMeshes,
                Output = allMeshesCombined[0],
                VertexSubarraySizes = vertexSubarraySizes,
                IndexSubarraySizes = indexSubarraySizes
            };

            // Schedules all of the above jobs and chains them together
            var calculateSubarraySizesJobHandle = calculateSubarraySizesJob.Schedule(allLinearFeatureMeshes.Length,
                JobConstants.SmWorkloadBatchSize, dependencies);
            var prepareCombinedMeshJobHandle = prepareCombinedMeshJob.Schedule(calculateSubarraySizesJobHandle);
            var combineMeshesJobHandle = combineMeshesJob.Schedule(allLinearFeatureMeshes.Length,
                JobConstants.LgWorkloadBatchSize, prepareCombinedMeshJobHandle);

            return (combineMeshesJobHandle, tileStruct);
        }

        #endregion

        /// <summary>
        /// Calculates the desired linear feature width (in Unity units) for a given
        /// <paramref name="zoomLevel"/> and <paramref name="size"/> scale factor.
        /// </summary>
        /// <param name="zoomLevel">The current tile's <see cref="IMapTile.ZoomLevel"/></param>
        /// <param name="size">A factor by which to scale the linear feature size</param>
        private float CalculateLinearFeatureWidth(int zoomLevel, double size)
        {
            UpdateLinearFeatureSizes(_linearFeatureSize);

            var linearFeatureWidth = (float)(_linearFeatureWidthMax * _linearFeatureWidthBaseSize / size);
            linearFeatureWidth = (float)MathEx.Clamp(linearFeatureWidth, _linearFeatureWidthMin, _linearFeatureWidthMax);

            return linearFeatureWidth;
        }

        /// <summary>
        /// Updates the builder's minimum and maximum linear
        /// feature widths, given a <see cref="LinearFeatureSize"/>.
        /// </summary>
        /// <param name="linearFeatureSize"></param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="linearFeatureSize"/>
        /// is not a valid <see cref="LinearFeatureSize"/></exception>
        private void UpdateLinearFeatureSizes(LinearFeatureSize linearFeatureSize)
        {
            switch (linearFeatureSize)
            {
                case LinearFeatureSize.Small:
                    _linearFeatureWidthMin = LinearFeatureSizeSettings.SmallLinearFeatureMin;
                    _linearFeatureWidthMax = LinearFeatureSizeSettings.SmallLinearFeatureMax;
                    break;

                case LinearFeatureSize.Medium:
                    _linearFeatureWidthMin = LinearFeatureSizeSettings.MedLinearFeatureMin;
                    _linearFeatureWidthMax = LinearFeatureSizeSettings.MedLinearFeatureMax;
                    break;

                case LinearFeatureSize.Large:
                    _linearFeatureWidthMin = LinearFeatureSizeSettings.LargeLinearFeatureMin;
                    _linearFeatureWidthMax = LinearFeatureSizeSettings.LargeLinearFeatureMax;
                    break;

                case LinearFeatureSize.Custom:
                    _linearFeatureWidthMin = _customLinearFeatureMin;
                    _linearFeatureWidthMax = _customLinearFeatureMax;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(linearFeatureSize), linearFeatureSize, null);
            }
        }
    }
}
