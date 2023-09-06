// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;
using UnityEngine;

namespace Niantic.Lightship.Maps.Attributes
{
    /// <summary>
    /// Base class for attributes that can be applied to serialized
    /// fields to conditionally modify their behavior based upon
    /// a boolean value stored in another serialized field.
    /// </summary>
    internal abstract class ConditionalAttribute : PropertyAttribute
    {
        /// <summary>
        /// The path to the property whose value is used to
        /// evaluate a condition defined in a derived type.
        /// </summary>
        public string ConditionalPropertyPath { get; private set; }

        /// <summary>
        /// The value that the conditional property's value is
        /// compared against.  Generally, if these values match,
        /// then the behavior of the serialized field to which
        /// this attribute is applied will be modified, otherwise
        /// the field's behavior will fall back to its default.
        /// </summary>
        protected readonly bool IsActiveIfEqualTo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conditionalPropertyPath">The path
        /// to the property whose value is used to evaluate
        /// a condition defined in a derived type.</param>
        /// <param name="activeIfEqualTo">The value that the
        /// conditional property's value is compared against.
        /// Generally, if these values match, then the behavior
        /// of the serialized field to which this attribute is
        /// applied will be modified, otherwise the field's
        /// behavior will fall back to its default.</param>
        protected ConditionalAttribute(
            string conditionalPropertyPath, bool activeIfEqualTo)
        {
            ConditionalPropertyPath = conditionalPropertyPath;
            IsActiveIfEqualTo = activeIfEqualTo;
        }

        /// <summary>
        /// Overridden in derived types to determine
        /// whether a field is hidden in the Inspector.
        /// </summary>
        public abstract bool IsHidden(bool value);

        /// <summary>
        /// Overridden in derived types to determine
        /// whether a field is disabled in the Inspector.
        /// </summary>
        public abstract bool IsDisabled(bool value);
    }
}
