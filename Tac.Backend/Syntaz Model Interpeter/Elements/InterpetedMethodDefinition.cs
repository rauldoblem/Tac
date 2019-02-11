﻿using System;
using Tac.Model;
using Tac.Syntaz_Model_Interpeter.Run_Time_Objects;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedMethodDefinition<TIn, TOut> : IInterpetedOperation<IInterpetedMethod<TIn, TOut>>
    {
        public void Init(
            InterpetedMemberDefinition parameterDefinition, 
            IInterpeted[] methodBody,
            IInterpetedScopeTemplate scope)
        {
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            Body = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public InterpetedMemberDefinition ParameterDefinition { get; private set; }
        public IInterpeted[] Body { get; private set; }
        public IInterpetedScopeTemplate Scope { get; private set; }
        
        public IInterpetedResult<IInterpetedMember<IInterpetedMethod<TIn,TOut>>> Interpet(InterpetedContext interpetedContext)
        {
            return InterpetedResult.Create(
                new InterpetedMember<IInterpetedMethod<TIn, TOut>>(
                    new InterpetedMethod<TIn, TOut>(
                        ParameterDefinition,
                        Body, 
                        interpetedContext,
                        Scope)));
        }
        
        public IInterpeted GetDefault(InterpetedContext interpetedContext)
        {
            return new InterpetedMethod<TIn, TOut>(
                new InterpetedMemberDefinition().Init(new NameKey("input")),
                new IInterpeted[] { },
                interpetedContext,
                Scope);
        }
    }
}