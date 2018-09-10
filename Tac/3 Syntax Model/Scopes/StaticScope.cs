﻿using System.Collections.Generic;
using Tac.Semantic_Model.Operations;

namespace Tac.Semantic_Model
{

    public class StaticScope : Scope {
        public override bool Equals(object obj) => obj is StaticScope && base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public bool TryAddStaticMember(MemberDefinition definition)
        {
            return TryAdd(DefintionLifetime.Static, definition);
        }
        
        public bool TryAddStaticType(TypeDefinition definition)
        {
            return TryAdd(DefintionLifetime.Static, definition);
        }

        public bool TryAddStaticGenericType(GenericTypeDefinition definition)
        {
            return TryAddGeneric(DefintionLifetime.Static, definition);
        }
    }
    
}