﻿using Prototypist.LeftToRight;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedMultiplyOperation : MultiplyOperation, IInterpeted
    {
        public InterpetedMultiplyOperation(ICodeElement left, ICodeElement right) : base(left, right)
        {
        }
        
        public InterpetedResult Interpet(InterpetedContext interpetedContext)
        {
            return new InterpetedResult(
                left.Cast<IInterpeted>().Interpet(interpetedContext).Get<double>() *
                right.Cast<IInterpeted>().Interpet(interpetedContext).Get<double>());
        }
    }
}