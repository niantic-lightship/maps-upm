// Copyright 2023 Niantic, Inc. All Rights Reserved.

using System;

namespace Niantic.Lightship.Maps.Attributes
{
    /// <summary>
    /// Base class for attributes used to hide serialized fields in the Inspector
    /// </summary>
    internal abstract class HiddenConditionalAttribute : ConditionalAttribute
    {
        /// <inheritdoc />
        protected HiddenConditionalAttribute(
            string conditionalPropertyPath, bool activeIfEqualTo)
            : base(conditionalPropertyPath, activeIfEqualTo)
        {
        }

        /// <inheritdoc />
        public override bool IsHidden(bool value)
            => IsActiveIfEqualTo == value;

        /// <summary>
        /// <see cref="HiddenConditionalAttribute"/>s
        /// do not affect whether a field is disable,
        /// so this will always evaluate to false.
        /// </summary>
        public override bool IsDisabled(bool value) => false;
    }

    /// <summary>
    /// Hides a serialized field in the Inspector
    /// if another serialized field's value is false.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class HiddenIfFalseAttribute : HiddenConditionalAttribute
    {
        /// <summary>
        /// Hides a serialized field in the Inspector
        /// if another serialized field's value is false.
        /// </summary>
        /// <param name="conditionalPropertyPath">The path to
        /// another serialized field.  If that field's value is
        /// false, then the serialized field that this attribute
        /// applies to will be hidden in the Inspector.</param>
        public HiddenIfFalseAttribute(string conditionalPropertyPath)
            : base(conditionalPropertyPath, false)
        {
        }
    }

    /// <summary>
    /// Hides a serialized field in the Inspector
    /// if another serialized field's value is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal class HiddenIfTrueAttribute : HiddenConditionalAttribute
    {
        /// <summary>
        /// Hides a serialized field in the Inspector
        /// if another serialized field's value is true.
        /// </summary>
        /// <param name="conditionalPropertyPath">The path to
        /// another serialized field.  If that field's value is
        /// true, then the serialized field that this attribute
        /// applies to will be hidden in the Inspector.</param>
        public HiddenIfTrueAttribute(string conditionalPropertyPath)
            : base(conditionalPropertyPath, true)
        {
        }
    }
}
