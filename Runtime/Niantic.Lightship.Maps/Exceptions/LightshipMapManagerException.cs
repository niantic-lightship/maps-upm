// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using JetBrains.Annotations;
using Niantic.Lightship.Maps.Core.Exceptions;

namespace Niantic.Lightship.Maps.Exceptions
{
    /// <summary>
    /// Base class for exceptions thrown from <see cref="LightshipMapManager"/>
    /// </summary>
    [PublicAPI]
    public abstract class LightshipMapManagerException : LightshipMapsSdkException
    {
        protected LightshipMapManagerException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// This exception will be thrown from <see cref="LightshipMapView"/>
    /// if its <see cref="LightshipMapManager"/> fails to initialize.
    /// </summary>
    [PublicAPI]
    public class LightshipMapManagerNotInitializedException : LightshipMapManagerException
    {
        internal LightshipMapManagerNotInitializedException() :
            base($"{nameof(LightshipMapManager)} failed to initialize!")
        {
        }
    }
}
