﻿using System;
using Tac.Syntaz_Model_Interpeter.Run_Time_Objects;
using Tac.Model;

namespace Tac.Syntaz_Model_Interpeter
{

    internal interface IInterpetedImplementationDefinition: IInterpetedOperation
    {

    }

    internal class InterpetedImplementationDefinition<TIn, TMethodIn, TMethodOut> : IInterpetedOperation<IInterpetedImplementation<TIn,TMethodIn,TMethodOut>>, IInterpetedImplementationDefinition
    {
        public void Init(
            InterpetedMemberDefinition<TMethodIn> parameterDefinition, 
            InterpetedMemberDefinition<TIn> contextDefinition, 
            IInterpeted[] methodBody,
            IInterpetedScopeTemplate scope)
        {
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            ContextDefinition = contextDefinition ?? throw new ArgumentNullException(nameof(contextDefinition));
            MethodBody = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public InterpetedMemberDefinition<TMethodIn> ParameterDefinition { get; private set; }
        public InterpetedMemberDefinition<TIn> ContextDefinition { get; private set; }
        public IInterpeted[] MethodBody { get; private set; }
        public IInterpetedScopeTemplate Scope { get; private set; }

        public IInterpetedResult<IInterpetedMember<IInterpetedImplementation<TIn, TMethodIn, TMethodOut>>> Interpet(InterpetedContext interpetedContext)
        {
            return InterpetedResult.Create(
                new InterpetedMember<IInterpetedImplementation<TIn, TMethodIn, TMethodOut>>(
                new InterpetedImplementation<TIn, TMethodIn, TMethodOut>(
                ParameterDefinition,
                ContextDefinition,
                MethodBody,
                interpetedContext,
                Scope)));
        }
        
        void IInterpetedOperation.Interpet(InterpetedContext interpetedContext)
        {
            Interpet(interpetedContext);
        }

        public IInterpetedImplementation<TIn, TMethodIn, TMethodOut> GetDefault(InterpetedContext interpetedContext)
        {
            return new InterpetedImplementation< TIn, TMethodIn, TMethodOut > (
                    new InterpetedMemberDefinition<TMethodIn>().Init(new NameKey("input")),
                    new InterpetedMemberDefinition<TIn>().Init(new NameKey("context")),
                    new IInterpeted[] { },
                    interpetedContext,
                    Scope);
        }
    }
    
}