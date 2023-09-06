// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.Attributes
{
    /// <summary>
    /// Base class for attributes used to disable serialized fields in the Inspector
    /// </summary>
    internal abstract class DisabledConditionalAttribute : ConditionalAttribute
    {
        /// <inheritdoc />
        protected DisabledConditionalAttribute(
            string conditionalPropertyPath, bool activeIfEqualTo)
            : base(conditionalPropertyPath, activeIfEqualTo)
        {
        }

        /// <inheritdoc />
        public override bool IsDisabled(bool value)
            => IsActiveIfEqualTo == value;

        /// <summary>
        /// <see cref="DisabledConditionalAttribute"/>s
        /// do not affect a field's visibility, so
        /// this will always evaluate to false.
        /// </summary>
        public override bool IsHidden(bool value) => false;
    }

    /// <summary>
    /// Disables a serialized field in the Inspector
    /// if another serialized field's value is false.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class DisabledIfFalseAttribute : DisabledConditionalAttribute
    {
        /// <summary>
        /// Disables a serialized field in the Inspector
        /// if another serialized field's value is false.
        /// </summary>
        /// <param name="conditionalPropertyPath">The path to
        /// another serialized field.  If that field's value is
        /// false, then the serialized field that this attribute
        /// applies to will be disabled in the Inspector.</param>
        public DisabledIfFalseAttribute(string conditionalPropertyPath)
            : base(conditionalPropertyPath, false)
        {
        }
    }

    /// <summary>
    /// Disables a serialized field in the Inspector
    /// if another serialized field's value is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class DisabledIfTrueAttribute : DisabledConditionalAttribute
    {
        /// <summary>
        /// Disables a serialized field in the Inspector
        /// if another serialized field's value is true.
        /// </summary>
        /// <param name="conditionalPropertyPath">The path to
        /// another serialized field.  If that field's value is
        /// true, then the serialized field that this attribute
        /// applies to will be disabled in the Inspector.</param>
        public DisabledIfTrueAttribute(string conditionalPropertyPath)
            : base(conditionalPropertyPath, true)
        {
        }
    }
}
