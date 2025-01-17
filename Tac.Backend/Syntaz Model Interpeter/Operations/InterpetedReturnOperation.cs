﻿using System;
using Prototypist.LeftToRight;
using Tac.Syntaz_Model_Interpeter.Run_Time_Objects;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedReturnOperation<T> : InterpetedTrailingOperation<T,T>
        where T : IInterpetedAnyType
    {
        public override IInterpetedResult<IInterpetedMember<T>> Interpet(InterpetedContext interpetedContext)
        {
            var argumentResult = Argument.Interpet(interpetedContext);

            if (argumentResult.IsReturn(out var argumentReturned, out var argumentValue))
            {
                return InterpetedResult.Return<IInterpetedMember<T>>(argumentReturned);
            }

            return InterpetedResult.Return<IInterpetedMember<T>>(argumentValue);
        }
        
    }
}