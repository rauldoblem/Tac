﻿namespace Tac.Semantic_Model.CodeStuff
{
    public interface ICodeElement {
        ITypeDefinition<IScope> ReturnType(ScopeStack scope);
    }
}
