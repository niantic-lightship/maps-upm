// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Async.Logging;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.Structures.Jobs;
using Niantic.Lightship.Maps.Builders.Performance.Utils;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Jobs;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Niantic.Lightship.Maps.Builders.Performance.Structures
{
    /// <summary>
    /// A builder for <see cref="StructureFeature"/>s
    /// </summary>
    [PublicAPI]
    public class StructureFeatureBuilderAsync : MeshBuilderAsync
    {
        [Tooltip("The minimum height of generated building meshes.")]
        [SerializeField]
        private float _minHeight;

        [Tooltip("The maximum height of generated building meshes.")]
        [SerializeField]
        private float _maxHeight = 0.2f;

        /// <inheritdoc />
        public override void Build(IReadOnlyList<MeshTile> tiles)
        {
            var tileCount = tiles.Count;

            try
            {
                // Grab IStructureFeatures for each tile and convert them into Native (Job-friendly) representations
                var nativeFeaturesByTile = GetFeaturesByTile<IStructureFeature>(tiles).ToNative();

                // Spin off a new Build Task for each tile
                for (int i = 0; i < tileCount; i++)
                {
                    BuildStructureMesh(tiles[i], nativeFeaturesByTile[i]);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Represents the entire build process for all of the IStructureFeatures in a single tile
        /// </summary>
        /// <param name="tile">The <see cref="MeshTile"/> to be built</param>
        /// <param name="nativeFeatures">The <paramref name="tile"/>'s features</param>
        private void BuildStructureMesh(MeshTile tile, NativeArray<NativeStructureFeature> nativeFeatures)
        {
            try
            {
                var token = tile.Token;

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    nativeFeatures.CleanUp();
                    token.ThrowIfCancellationRequested();
                }

                // If there are no valid features for this tile, cancel the build process
                if (nativeFeatures.Length == 0)
                {
                    nativeFeatures.CleanUp();
                    throw new OperationCanceledException();
                }

                // The tops and walls of each IStructureFeature are calculated separately
                // but store in the same MeshDataArray (allMeshes) so that they can all be
                // combined together into a single mesh more readily.

                // Starts a Job which calculates the mesh shapes for the tops of each
                // IStructureFeature, storing them in the first half of allMeshes.
                var (createTopMeshesHandle, topAndWallMeshes) = CreateTopMeshes(nativeFeatures);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    createTopMeshesHandle.Complete();
                    nativeFeatures.CleanUp();
                    topAndWallMeshes.Dispose();
                    token.ThrowIfCancellationRequested();
                }

                // Schedules a Job which calculates the mesh shapes for the walls of each
                // IStructureFeature, storing them in the second half of allMeshes.
                var createWallMeshesHandle = CreateWallMeshes(createTopMeshesHandle, topAndWallMeshes, nativeFeatures);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    createWallMeshesHandle.Complete();
                    nativeFeatures.CleanUp();
                    topAndWallMeshes.Dispose();
                    token.ThrowIfCancellationRequested();
                }

                // Schedules a Job which combines all of the top and wall meshes
                // for all of this tile's IStructureFeatures into a single mesh.
                var (combineMeshesJobHandle, tileStruct) = CombineMeshes(createWallMeshesHandle, topAndWallMeshes);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    combineMeshesJobHandle.Complete();
                    nativeFeatures.CleanUp();
                    tileStruct.CombinedFeatureMeshes.Dispose();
                    tileStruct.Dispose();
                    token.ThrowIfCancellationRequested();
                }

                // Sends the Job to a JobMonitor to be
                // completed during a future LateUpdate frame.
                var monitor = tile.Monitor.CreateJobMonitor(this);
                var jobHandleController = new CallbackJobHandleController(
                    handle: combineMeshesJobHandle,
                    maxAge: JobConstants.TempJobMaxAge,
                    onCompleted: () =>
                    {
                        // When the meshes are built (i.e., the Job is completed),
                        // apply the mesh to the MapTileObject's MeshFilter
                        // and Dispose of any remaining NativeCollections
                        ApplyMesh(tile, tileStruct, token);
                        nativeFeatures.CleanUp();
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
                        nativeFeatures.CleanUp();
                        tile.MarkBuilderComplete(this);
                    });
                monitor.Initialize(jobHandleController, JobMonitor.UpdateMode.LateUpdate, token);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                token.ThrowIfCancellationRequested();
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
                nativeFeatures.CleanUp();
                Log.Error(e.Message);
                throw;
            }
        }

        #region Mesh Building Stages

        /// <summary>
        /// Starts a Job which calculates the mesh shapes for the tops of each
        /// IStructureFeature, storing them in the first half of the output
        /// MeshDataArray.
        /// </summary>
        /// <param name="features">The <see cref="IStructureFeature"/>s to be built</param>
        /// <returns>
        /// <para>A <see cref="Mesh.MeshDataArray"/> whose length is double the
        /// number of <see cref="IStructureFeature"/>s in the current tile
        /// </para>
        /// <para>The first half will contain the top meshes of the current <see cref="MeshTile"/>'s
        /// <see cref="IStructureFeature"/>s; the second half will contain the wall meshes of the
        /// current <see cref="MeshTile"/>'s <see cref="IStructureFeature"/>s
        /// </para>
        /// </returns>
        private (JobHandle, Mesh.MeshDataArray) CreateTopMeshes(NativeArray<NativeStructureFeature> features)
        {
            var meshes = Mesh.AllocateWritableMeshData(features.Length * 2);

            var createTopMeshesJob = new CreateTopMeshesJob(features, meshes, _minHeight, _maxHeight);

            var createTopMeshesHandle =
                createTopMeshesJob.Schedule(features.Length, JobConstants.LgWorkloadBatchSize);

            return (createTopMeshesHandle, meshes);
        }

        /// <summary>
        /// Starts a Job which calculates the mesh shapes for the walls of each
        /// IStructureFeature, storing them in the second half of the input
        /// MeshDataArray.
        /// </summary>
        /// <param name="dependencies">Any <see cref="JobHandle"/>s
        /// for Jobs that need to be completed before this one.</param>
        /// <param name="topAndWallMeshes">
        /// <para>A <see cref="Mesh.MeshDataArray"/> whose length is double the
        /// number of <see cref="IStructureFeature"/>s in the current tile
        /// </para>
        /// <para>The first half contains the top meshes of the current <see cref="MeshTile"/>'s
        /// <see cref="IStructureFeature"/>s; the second half will contain the wall meshes of the
        /// current <see cref="MeshTile"/>'s <see cref="IStructureFeature"/>s.
        /// </para>
        /// </param>
        /// <param name="features">The <see cref="IStructureFeature"/>s to be built</param>
        private JobHandle CreateWallMeshes(JobHandle dependencies, Mesh.MeshDataArray topAndWallMeshes,
            NativeArray<NativeStructureFeature> features)
        {
            var createWallMeshJob = new CreateWallMeshesJob(features, topAndWallMeshes, _minHeight, _maxHeight);

            return createWallMeshJob
                .Schedule(features.Length, JobConstants.LgWorkloadBatchSize, dependencies);
        }

        /// <summary>
        /// Starts a Job which combines all of the top and wall meshes for
        /// all of this tile's IStructureFeatures into a single mesh.
        /// </summary>
        /// <param name="dependencies">Any <see cref="JobHandle"/>s for
        /// Jobs that need to be completed before this one.</param>
        /// <param name="topAndWallMeshes">
        /// <para>A <see cref="Mesh.MeshDataArray"/> whose length is double the
        /// number of <see cref="IStructureFeature"/>s in the current tile.
        /// </para>
        /// <para>The first half contains the top meshes of the current <see cref="MeshTile"/>'s
        /// <see cref="IStructureFeature"/>s; the second half contains the wall meshes of the
        /// current <see cref="MeshTile"/>'s <see cref="IStructureFeature"/>s.
        /// </para>
        /// </param>
        /// <returns>A <see cref="Mesh.MeshDataArray"/> (of length 1) whose only element
        /// is the mesh of all the current <see cref="MeshTile"/>'s features combined</returns>
        private (JobHandle, MapTileBuildStruct) CombineMeshes(JobHandle dependencies,
            Mesh.MeshDataArray topAndWallMeshes)
        {
            // Allocates a MeshDataArray to contain the final, single mesh
            // consisting of the tile's feature meshes combined together.
            var combinedMeshDataArray = Mesh.AllocateWritableMeshData(1);
            var combinedMeshData = combinedMeshDataArray[0];

            var vertexSubarraySizes = new NativeArray<int>(topAndWallMeshes.Length, Allocator.TempJob);
            var indexSubarraySizes = new NativeArray<int>(topAndWallMeshes.Length, Allocator.TempJob);

            // Creates NativeArrays with the feature and index counts of each feature
            var calculateSubarraySizesJob = new CalculateSubarraySizesJob
            (
                topAndWallMeshes,
                vertexSubarraySizes,
                indexSubarraySizes
            );

            // A helper struct containing all the relevant
            // NativeContainers needed for the Build process.
            var tileStruct = new MapTileBuildStruct
            (
                default, vertexSubarraySizes,
                default, indexSubarraySizes,
                topAndWallMeshes, combinedMeshDataArray
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
                Input = topAndWallMeshes,
                Output = combinedMeshData,
                VertexSubarraySizes = vertexSubarraySizes,
                IndexSubarraySizes = indexSubarraySizes
            };

            // Schedules all of the above jobs and chains them together
            var calculateSubarraySizesJobHandle = calculateSubarraySizesJob.Schedule(topAndWallMeshes.Length,
                JobConstants.SmWorkloadBatchSize, dependencies);
            var prepareCombinedMeshJobHandle = prepareCombinedMeshJob.Schedule(calculateSubarraySizesJobHandle);
            var combineJobHandle = combineMeshesJob.Schedule(topAndWallMeshes.Length,
                JobConstants.LgWorkloadBatchSize, prepareCombinedMeshJobHandle);

            return (combineJobHandle, tileStruct);
        }

        #endregion
    }
}
