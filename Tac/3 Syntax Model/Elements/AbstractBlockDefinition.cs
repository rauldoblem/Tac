﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tac.Model;
using Tac.Model.Elements;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model
{

    public abstract class WeakAbstractBlockDefinition : ICodeElement, IScoped, IBlockDefinition
    {
        protected WeakAbstractBlockDefinition(IWeakFinalizedScope scope, ICodeElement[] body, IEnumerable<ICodeElement> staticInitailizers) {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            StaticInitailizers = staticInitailizers ?? throw new ArgumentNullException(nameof(staticInitailizers));
        }

        public IWeakFinalizedScope Scope { get; }
        public ICodeElement[] Body { get; }
        public IEnumerable<ICodeElement> StaticInitailizers { get; }

        public IType Returns() { return this; }
    }
}