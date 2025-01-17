﻿using System;
using System.Collections.Generic;
using Tac.Model.Elements;
using Tac.Model.Operations;

namespace Tac.Model.Instantiated
{
    public class ImplementationDefinition : IImplementationDefinition, IImplementationDefinitionBuilder
    {
        private readonly Buildable<IVerifiableType> buildableOutputType = new Buildable<IVerifiableType>();
        private readonly Buildable<IMemberDefinition> buildableContextDefinition = new Buildable<IMemberDefinition>();
        private readonly Buildable<IMemberDefinition> buildableParameterDefinition = new Buildable<IMemberDefinition>();
        private readonly Buildable<IFinalizedScope> buildableScope = new Buildable<IFinalizedScope>();
        private readonly Buildable<IEnumerable<ICodeElement>> buildableMethodBody = new Buildable<IEnumerable<ICodeElement>>();
        private readonly Buildable<IEnumerable<ICodeElement>> buildableStaticInitialzers = new Buildable<IEnumerable<ICodeElement>>();

        private ImplementationDefinition()
        {
        }

        #region IImplementationDefinition
        
        public IVerifiableType OutputType { get => buildableOutputType.Get(); }
        public IMemberDefinition ContextDefinition { get => buildableContextDefinition.Get(); }
        public IMemberDefinition ParameterDefinition { get => buildableParameterDefinition.Get(); }
        public IFinalizedScope Scope { get => buildableScope.Get(); }
        public IEnumerable<ICodeElement> MethodBody { get => buildableMethodBody.Get(); }
        public IEnumerable<ICodeElement> StaticInitialzers { get => buildableStaticInitialzers.Get(); }

        public T Convert<T, TBacking>(IOpenBoxesContext<T, TBacking> context)
            where TBacking : IBacking
        {
            return context.ImplementationDefinition(this);
        }

        public IVerifiableType Returns()
        {
            return ImplementationType.CreateAndBuild(ParameterDefinition.Type,OutputType, ContextDefinition.Type);
        }

        #endregion
        
        public void Build(IVerifiableType outputType, IMemberDefinition contextDefinition, IMemberDefinition parameterDefinition, IFinalizedScope scope, IEnumerable<ICodeElement> methodBody, IEnumerable<ICodeElement> staticInitialzers) {
            buildableOutputType.Set(outputType);
            buildableContextDefinition.Set(contextDefinition);
            buildableParameterDefinition.Set(parameterDefinition);
            buildableMethodBody.Set(methodBody);
            buildableStaticInitialzers.Set(staticInitialzers);
            buildableScope.Set(scope);
        }
        
        public static (IImplementationDefinition, IImplementationDefinitionBuilder) Create()
        {
            var res = new ImplementationDefinition();
            return (res, res);
        }

        public static IImplementationDefinition CreateAndBuild(IVerifiableType outputType, IMemberDefinition contextDefinition, IMemberDefinition parameterDefinition, IFinalizedScope scope, IEnumerable<ICodeElement> methodBody, IEnumerable<ICodeElement> staticInitialzers) {
            var (x, y) = Create();
            y.Build(outputType, contextDefinition, parameterDefinition, scope, methodBody, staticInitialzers);
            return x;
        }
    }

    public interface IImplementationDefinitionBuilder
    {
        void Build(IVerifiableType outputType, IMemberDefinition contextDefinition, IMemberDefinition parameterDefinition, IFinalizedScope scope, IEnumerable<ICodeElement> methodBody, IEnumerable<ICodeElement> staticInitialzers);
    }
}
