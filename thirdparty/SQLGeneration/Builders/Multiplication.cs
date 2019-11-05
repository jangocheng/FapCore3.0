﻿using System;
using SQLGeneration.Parsing;

namespace SQLGeneration.Builders
{
    /// <summary>
    /// Represents the multiplication of two items in a command.
    /// </summary>
    public class Multiplication : ArithmeticExpression
    {
        /// <summary>
        /// Initializes a new instance of a Multiplication.
        /// </summary>
        /// <param name="leftHand">The left hand side of the expression.</param>
        /// <param name="rightHand">The right hand side of the expression.</param>
        public Multiplication(IProjectionItem leftHand, IProjectionItem rightHand)
            : base(leftHand, rightHand)
        {
        }

        /// <summary>
        /// Provides information to the given visitor about the current builder.
        /// </summary>
        /// <param name="visitor">The visitor requesting information.</param>
        protected override void OnAccept(BuilderVisitor visitor)
        {
            visitor.VisitMultiplication(this);
        }
    }
}
