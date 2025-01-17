﻿using System;
using System.Collections.Generic;
using Tac.Model.Elements;
using Tac.Model.Operations;

namespace Tac.Model.Instantiated
{

    public class MethodDefinition : IInternalMethodDefinition,
        IMethodDefinitionBuilder
    {
        private readonly Buildable<IEnumerable<ICodeElement>> buildableStaticInitailizers = new Buildable<IEnumerable<ICodeElement>>();
        private readonly Buildable<ICodeElement[]> buildableBody = new Buildable<ICodeElement[]>();
        private readonly Buildable<IFinalizedScope> buildableScope = new Buildable<IFinalizedScope>();
        private readonly Buildable<IVerifiableType> buildableInputType = new Buildable<IVerifiableType>();
        private readonly Buildable<IVerifiableType> buildableOutputType = new Buildable<IVerifiableType>();
        private readonly Buildable<IMemberDefinition> buildableParameterDefinition = new Buildable<IMemberDefinition>();
        private readonly BuildableValue<bool> buildableIsEntryPoint = new BuildableValue<bool>();

        private MethodDefinition() { }

        #region IMethodDefinition

        public IVerifiableType InputType => buildableInputType.Get();
        public IVerifiableType OutputType => buildableOutputType.Get();
        public IMemberDefinition ParameterDefinition => buildableParameterDefinition.Get();
        public IFinalizedScope Scope { get => buildableScope.Get(); }
        public ICodeElement[] Body { get => buildableBody.Get(); }
        public IEnumerable<ICodeElement> StaticInitailizers { get => buildableStaticInitailizers.Get(); }
        public bool IsEntryPoint { get => buildableIsEntryPoint.Get(); }


        public T Convert<T, TBacking>(IOpenBoxesContext<T, TBacking> context)
            where TBacking : IBacking
        {
            return context.MethodDefinition(this);
        }

        public IVerifiableType Returns()
        {
            return MethodType.CreateAndBuild(InputType, OutputType);
        }
        
        #endregion
        
        public void Build(
            IVerifiableType inputType,
            IVerifiableType outputType,
            IMemberDefinition parameterDefinition,
            IFinalizedScope scope,
            ICodeElement[] body,
            IEnumerable<ICodeElement> staticInitailizers,
            bool isEntryPoint)
        {
            buildableInputType.Set(inputType);
            buildableOutputType.Set(outputType);
            buildableParameterDefinition.Set(parameterDefinition);
            buildableScope.Set(scope);
            buildableBody.Set(body);
            buildableStaticInitailizers.Set(staticInitailizers);
            buildableIsEntryPoint.Set(isEntryPoint);
        }

        public static (IInternalMethodDefinition, IMethodDefinitionBuilder) Create()
        {
            var res = new MethodDefinition();
            return (res, res);
        }

        public static IInternalMethodDefinition CreateAndBuild(
            IVerifiableType inputType,
            IVerifiableType outputType,
            IMemberDefinition parameterDefinition,
            IFinalizedScope scope,
            ICodeElement[] body,
            IEnumerable<ICodeElement> staticInitailizers,
            bool isEntryPoint) {
            var (x, y) = Create();
            y.Build(inputType, outputType, parameterDefinition, scope, body, staticInitailizers, isEntryPoint);
            return x;
        }
    }

    public interface IMethodDefinitionBuilder
    {
        void Build(
            IVerifiableType inputType,
            IVerifiableType outputType,
            IMemberDefinition parameterDefinition,
            IFinalizedScope scope,
            ICodeElement[] body,
            IEnumerable<ICodeElement> staticInitailizers,
            bool isEntryPoint );
    }
}