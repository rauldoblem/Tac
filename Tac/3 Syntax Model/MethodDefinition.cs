﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model
{
    public class MethodDefinition: AbstractBlockDefinition<MethodScope>, ITypeSource, ITypeDefinition<MethodScope>
    {
        public MethodDefinition(ITypeSource outputType, MemberDefinition parameterDefinition, ICodeElement[] body, MethodScope scope, IEnumerable<ICodeElement> staticInitializers) : base(scope ?? throw new ArgumentNullException(nameof(scope)), body, staticInitializers)
        {
            OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            scope.TryAddParameter(parameterDefinition);
        }

        public ITypeSource InputType { get => ParameterDefinition.Type; }
        public ITypeSource OutputType { get; }
        public MemberDefinition ParameterDefinition { get; }

        public override bool Equals(object obj)
        {
            return obj is MethodDefinition definition && definition != null &&
                   base.Equals(obj) &&
                   EqualityComparer<ITypeSource>.Default.Equals(InputType, definition.InputType) &&
                   EqualityComparer<ITypeSource>.Default.Equals(OutputType, definition.OutputType) &&
                   EqualityComparer<MemberDefinition>.Default.Equals(ParameterDefinition, definition.ParameterDefinition);
        }

        public override int GetHashCode()
        {
            var hashCode = -814421114;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSource>.Default.GetHashCode(InputType);
            hashCode = hashCode * -1521134295 + EqualityComparer<ITypeSource>.Default.GetHashCode(OutputType);
            hashCode = hashCode * -1521134295 + EqualityComparer<MemberDefinition>.Default.GetHashCode(ParameterDefinition);
            return hashCode;
        }

        public override ITypeDefinition<IScope> ReturnType(ScopeStack scope) {
            if (!InputType.TryGetTypeDefinition(scope, out var input)) {
                throw new Exception($"could not find a type assocated with {InputType}");
            }

            if (!OutputType.TryGetTypeDefinition(scope, out var output))
            {
                throw new Exception($"could not find a type assocated with {OutputType}");
            }
            
            return RootScope.MethodType(input, output);
        }

        public bool TryGetTypeDefinition(ScopeStack scope, out ITypeDefinition<IScope> typeDefinition)
        {
            typeDefinition = RootScope.MethodType(InputType.GetTypeDefinitionOrThrow(scope), OutputType.GetTypeDefinitionOrThrow(scope));
            return true;
        }
    }
}