// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.Jobs
{
    /// <summary>
    /// Constants used for Job scheduling and management
    /// </summary>
    internal static class JobConstants
    {
        public const int SmWorkloadBatchSize = 64;
        public const int MdWorkloadBatchSize = 32;
        public const int LgWorkloadBatchSize = 8;
        public const int XlWorkloadBatchSize = 1;

        public const int TempJobMaxAge = 4;
    }
}
