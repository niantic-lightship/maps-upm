// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Async.Logging;
using Niantic.Lightship.Maps.Builders.Performance.Areas.Jobs;
using Niantic.Lightship.Maps.Builders.Performance.NativeFeatures;
using Niantic.Lightship.Maps.Builders.Performance.Utils;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Jobs;
using Niantic.Lightship.Maps.Builders.Performance.Utils.Structs;
using Niantic.Lightship.Maps.Core.Features;
using Niantic.Lightship.Maps.Jobs;
using Niantic.Lightship.Maps.MapTileObjectHelpers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using CalculateSubarraySizesJob = Niantic.Lightship.Maps.Builders.Performance.Areas.Jobs.CalculateSubarraySizesJob;

namespace Niantic.Lightship.Maps.Builders.Performance.Areas
{
    /// <summary>
    /// A builder for <see cref="AreaFeature"/>s
    /// </summary>
    [PublicAPI]
    public class AreaFeatureBuilderAsync : MeshBuilderAsync
    {
        /// <inheritdoc />
        public override void Build(IReadOnlyList<MeshTile> tiles)
        {
            var tileCount = tiles.Count;

            try
            {
                // Grab IAreaFeatures for each tile and convert them into Native (Job-friendly) representations
                var nativeFeaturesByTile = GetFeaturesByTile<IAreaFeature>(tiles).ToNative();

                // Spin off a new Build Task for each tile
                for (int i = 0; i < tileCount; i++)
                {
                    BuildAreaMesh(tiles[i], nativeFeaturesByTile[i]);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Represents the entire build process for all of the IAreaFeatures in a single tile
        /// </summary>
        /// <param name="tile">The <see cref="MeshTile"/> to build</param>
        /// <param name="nativeFeatures">The <paramref name="tile"/>'s features</param>
        private void BuildAreaMesh(MeshTile tile, NativeArray<NativeAreaFeature> nativeFeatures)
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

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    nativeFeatures.CleanUp();
                    token.ThrowIfCancellationRequested();
                }

                // Schedules a series of Jobs which calculate the mesh
                // shapes for each IAreaFeature for this tile and then
                // combines all of those meshes into a single mesh.
                var (combineJobHandle, tileStruct) = CreateAndCombineMeshes(nativeFeatures);

                // If the build process was cancelled early, then Dispose
                // of any NativeCollections that were created for this tile.
                if (token.IsCancellationRequested)
                {
                    combineJobHandle.Complete();
                    nativeFeatures.CleanUp();
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
                        // apply the mesh to the MapTileObject's MeshFilter
                        // and Dispose of any remaining NativeCollections.
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
                nativeFeatures.CleanUp();
                tile.MarkBuilderComplete(this);
                Log.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Schedules Jobs which calculate meshes for each of a
        /// tile's <see cref="IAreaFeature"/>s and combines them.
        /// </summary>
        /// <param name="allFeatures">A Job-friendly representation
        /// of this tile's <see cref="IAreaFeature"/>s.</param>
        /// <returns>The <see cref="JobHandle"/> representing the work
        /// to create the meshes and a <see cref="MapTileBuildStruct"/>
        /// containing the tile's mesh data.</returns>
        private static (JobHandle handle, MapTileBuildStruct tileStruct)
            CreateAndCombineMeshes(NativeArray<NativeAreaFeature> allFeatures)
        {
            int featureCount = allFeatures.Length;

            // Allocates a MeshDataArray to contain each feature's mesh
            var featureMeshes = Mesh.AllocateWritableMeshData(featureCount);

            // Allocates a MeshDataArray to contain the final, single mesh
            // consisting of the tile's feature meshes combined together.
            var combinedFeatureMesh = Mesh.AllocateWritableMeshData(1);

            var combinedMeshData = combinedFeatureMesh[0];

            var vertexSubarraySizes = new NativeArray<int>(featureCount, Allocator.TempJob);
            var indexSubarraySizes = new NativeArray<int>(featureCount, Allocator.TempJob);

            // Creates NativeArrays with the feature and index counts of each feature
            var calculateSubarraySizesJob = new CalculateSubarraySizesJob
            (
                allFeatures,
                vertexSubarraySizes,
                indexSubarraySizes
            );

            var verticesList = new NativeList<Vertex>(featureCount, Allocator.TempJob);
            var indicesList = new NativeList<int>(featureCount, Allocator.TempJob);

            // Using a DeferredJobArray (which is just an alias for the
            // NativeLists) allows Jobs depending on those NativeLists to wait
            // for them to be populated prior to being used in subsequent Jobs.
            var vertices = verticesList.AsDeferredJobArray();
            var indices = indicesList.AsDeferredJobArray();

            // A helper struct containing all the relevant
            // NativeContainers needed for the Build process.
            var tileStruct = new MapTileBuildStruct
            (
                verticesList, vertexSubarraySizes,
                indicesList, indexSubarraySizes,
                featureMeshes, combinedFeatureMesh
            );

            // Uses the contents of the subarray size arrays to set the sizes
            // of the NativeLists containing the meshes' vertices and indices.
            var createArraysJob = new CreateVertexAndIndexArraysJob
            (
                verticesList,
                vertexSubarraySizes,
                indicesList,
                indexSubarraySizes
            );

            // Populates the (deferred) vertex and index arrays
            // with the actual vertex and index data of the features.
            var populateFlattenedArraysJob = new PopulateFlattenedArraysJob
            (
                allFeatures,
                vertices,
                vertexSubarraySizes,
                indices,
                indexSubarraySizes
            );

            // Sets relevant vertex and index buffer
            // information on the final combinedMeshData.
            var prepareCombinedMeshJob = new PrepareCombinedMeshJob
            (
                combinedMeshData,
                vertexSubarraySizes,
                indexSubarraySizes
            );

            // Creates the individual meshes for each feature
            var createMeshesJob = new CreateMeshesJob
            (
                featureMeshes,
                vertices,
                vertexSubarraySizes,
                indices,
                indexSubarraySizes
            );

            // Combines the individual feature meshes
            // into a single mesh, which is stored in
            // the first/only element of combinedMeshData.
            var combineMeshesJob = new CombineMeshesJob
            {
                Input = featureMeshes,
                Output = combinedMeshData,
                VertexSubarraySizes = vertexSubarraySizes,
                IndexSubarraySizes = indexSubarraySizes
            };

            // Schedules all of the above jobs and chains them together
            var calculateSubarraySizesJobHandle =
                calculateSubarraySizesJob.Schedule(featureCount, JobConstants.SmWorkloadBatchSize);
            var createArraysJobHandle = createArraysJob.Schedule(calculateSubarraySizesJobHandle);
            var populateFlattenedArraysJobHandle = populateFlattenedArraysJob.Schedule(
                featureCount, JobConstants.MdWorkloadBatchSize, createArraysJobHandle);
            var prepareCombinedMeshJobHandle = prepareCombinedMeshJob.Schedule(populateFlattenedArraysJobHandle);
            var createMeshesJobHandle = createMeshesJob.Schedule(
                featureMeshes.Length, JobConstants.LgWorkloadBatchSize, prepareCombinedMeshJobHandle);
            var combineMeshesJobHandle = combineMeshesJob.Schedule(
                featureMeshes.Length, JobConstants.LgWorkloadBatchSize, createMeshesJobHandle);

            return (combineMeshesJobHandle, tileStruct);
        }
    }
}
