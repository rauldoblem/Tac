﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model
{
    public abstract class AbstractBlockDefinition<TScope> : ICodeElement, IScoped<TScope> where TScope : LocalStaticScope
    {
        protected AbstractBlockDefinition(TScope scope, ICodeElement[] body) {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
        
        public TScope Scope { get; }
        public ICodeElement[] Body { get; }

        public bool ContainsInTree(ICodeElement element) => Equals(element) || Body.Any(x => x.ContainsInTree(element));

        public override bool Equals(object obj)
        {
            var definition = obj as AbstractBlockDefinition<TScope>;
            return definition != null &&
                   EqualityComparer<TScope>.Default.Equals(Scope, definition.Scope) &&
                   EqualityComparer<ICodeElement[]>.Default.Equals(Body, definition.Body);
        }

        public override int GetHashCode()
        {
            var hashCode = 273578712;
            hashCode = hashCode * -1521134295 + EqualityComparer<TScope>.Default.GetHashCode(Scope);
            hashCode = hashCode * -1521134295 + EqualityComparer<ICodeElement[]>.Default.GetHashCode(Body);
            return hashCode;
        }
    }
}