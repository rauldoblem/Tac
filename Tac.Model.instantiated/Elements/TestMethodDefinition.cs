﻿using System;
using System.Collections.Generic;
using Tac.Model.Elements;
using Tac.Model.Operations;

namespace Tac.Model.Instantiated
{
    public class MethodDefinition : AbstractBlockDefinition, IMethodDefinition
    {
        public MethodDefinition(
            ITypeReferance inputType,
            ITypeReferance outputType, 
            IMemberDefinition parameterDefinition, 
            IFinalizedScope scope, 
            ICodeElement[] body, 
            IEnumerable<ICodeElement> staticInitailizers) : base(scope, body, staticInitailizers)
        {
            InputType = inputType;
            OutputType = outputType;
            ParameterDefinition = parameterDefinition;
        }

        public ITypeReferance InputType { get; set; }
        public ITypeReferance OutputType { get; set; }
        public IMemberDefinition ParameterDefinition { get; set; }

        #region IMethodDefinition

        IVarifiableType IMethodType.InputType => InputType;
        IVarifiableType IMethodType.OutputType => OutputType;

        #endregion

        public override T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.MethodDefinition(this);
        }
    }
}