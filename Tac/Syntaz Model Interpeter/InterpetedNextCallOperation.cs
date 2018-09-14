﻿using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedNextCallOperation : NextCallOperation, IInterpeted
    {
        public InterpetedNextCallOperation(ICodeElement left, ICodeElement right) : base(left, right)
        {
        }
    }
}