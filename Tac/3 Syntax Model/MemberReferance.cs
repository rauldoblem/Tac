﻿using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{
    public class MemberReferance : Referance<MemberDefinition>
    {
        public MemberReferance(NamePath key) : base(key)
        {
        }
        public MemberReferance(string key) : this(new NamePath(new AbstractName[] { new ExplicitName(key) }))
        {
        }
    }
}