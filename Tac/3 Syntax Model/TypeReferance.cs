﻿using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{
    public class TypeReferance : Referance<TypeDefinition>
    {
        public TypeReferance(NamePath key) : base(key)
        {
        }
        public TypeReferance(string key) : this(new NamePath(new AbstractName[] { new ExplicitName(key) }))
        {
        }
    }
}