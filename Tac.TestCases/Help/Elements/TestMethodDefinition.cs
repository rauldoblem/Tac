﻿using System;
using System.Collections.Generic;
using Tac.Model.Operations;

namespace Tac.Model.Elements
{
    public class TestMethodDefinition : TestAbstractBlockDefinition, IMethodDefinition
    {
        public TestMethodDefinition(
            IVarifiableType inputType, 
            IVarifiableType outputType, 
            IMemberDefinition parameterDefinition, 
            IFinalizedScope scope, 
            ICodeElement[] body, 
            IEnumerable<ICodeElement> staticInitailizers) : base(scope, body, staticInitailizers)
        {
            InputType = inputType;
            OutputType = outputType;
            ParameterDefinition = parameterDefinition;
        }

        public IVarifiableType InputType { get; set; }
        public IVarifiableType OutputType { get; set; }
        public IMemberDefinition ParameterDefinition { get; set; }

        public override T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.MethodDefinition(this);
        }
    }
}