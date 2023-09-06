// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.Jobs;

namespace Niantic.Lightship.Maps.Builders.Performance.Utils.Interfaces
{
    /// <summary>
    /// Represents an object (usually an <see cref="IMapTileObject"/>)
    /// that has Job-related functionality.
    /// </summary>
    internal interface IBuilderJobMonitorManager
    {
        /// <summary>
        /// Returns a cached <see cref="JobMonitor"/>, if it exists
        /// </summary>
        /// <param name="builder">The builder currently referencing this object</param>
        JobMonitor GetMonitor(IFeatureBuilder builder);

        /// <summary>
        /// Returns a newly created <see cref="JobMonitor"/>
        /// </summary>
        /// <param name="builder">The builder currently referencing this object</param>
        JobMonitor CreateJobMonitor(IFeatureBuilder builder);
    }
}
