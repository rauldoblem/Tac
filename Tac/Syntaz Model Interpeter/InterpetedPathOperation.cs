﻿using Prototypist.LeftToRight;
using Tac.Semantic_Model;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedPathOperation : PathOperation, IInterpeted
    {
        public InterpetedPathOperation(ICodeElement left, MemberDefinition right) : base(left, right)
        {
        }

        public InterpetedResult Interpet(InterpetedContext interpetedContext)
        {


            var scope = left.Cast<IInterpeted>().Interpet(interpetedContext).Cast<IInterpetedScope>();

            return  InterpetedResult.Create(scope.GetMember(right.Key.Key));
            
            // TODO what happens here, this is not IInterpeted
            // this never makes it that far
            // path operations are a lie
            // this whole object should not exist... 
        }
    }
}