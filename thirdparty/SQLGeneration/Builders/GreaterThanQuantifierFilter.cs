﻿using System;
using SQLGeneration.Parsing;

namespace SQLGeneration.Builders
{
    /// <summary>
    /// Represents a filter that see that a value is greater than all or some of the values.
    /// </summary>
    public class GreaterThanQuantifierFilter : QuantifierFilter
    {
        /// <summary>
        /// Initializes a new insstance of an GreaterThanQuantifierFilter.
        /// </summary>
        /// <param name="leftHand">The value being compared to the set of values.</param>
        /// <param name="quantifier">The quantifier to use to compare the value to the set of values.</param>
        /// <param name="valueProvider">The source of values.</param>
        public GreaterThanQuantifierFilter(IFilterItem leftHand, Quantifier quantifier, IValueProvider valueProvider)
            : base(leftHand, quantifier, valueProvider)
        {
        }

        /// <summary>
        /// Provides information to the given visitor about the current builder.
        /// </summary>
        /// <param name="visitor">The visitor requesting information.</param>
        protected override void OnAccept(BuilderVisitor visitor)
        {
            visitor.VisitGreaterThanQuantifierFilter(this);
        }
    }
}
