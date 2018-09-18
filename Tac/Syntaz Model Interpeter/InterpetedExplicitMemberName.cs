﻿using Tac.Semantic_Model.Names;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedExplicitMemberName : ExplicitMemberName, IInterpeted
    {
        public InterpetedExplicitMemberName(string name) : base(name)
        {
        }

        public InterpetedResult Interpet(InterpetedContext interpetedContext)
        {
            return new InterpetedResult(this);
        }
    }
}