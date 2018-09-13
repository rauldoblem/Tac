﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;

namespace Tac.Semantic_Model
{
    public class ObjectDefinition: ITypeDefinition, ICodeElement
    {
        public ObjectDefinition(ObjectScope scope, IEnumerable<ICodeElement> codeElements) {
            Scope = scope;
            CodeElements = codeElements.ToArray();
        }

        public IScope Scope { get; }
        public ICodeElement[] CodeElements { get; }

        public override bool Equals(object obj)
        {
            return obj is ObjectDefinition definition &&
                   EqualityComparer<IScope>.Default.Equals(Scope, definition.Scope);
        }

        public override int GetHashCode()
        {
            var hashCode = 1953067843;
            hashCode = hashCode * -1521134295 + EqualityComparer<IScope>.Default.GetHashCode(Scope);
            return hashCode;
        }

        public ITypeDefinition ReturnType(ScopeStack scope) {
            return this;
        }
    }
}
