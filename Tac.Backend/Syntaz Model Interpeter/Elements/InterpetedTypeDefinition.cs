﻿
using Tac.Syntaz_Model_Interpeter.Run_Time_Objects;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedTypeDefinition: IInterpetedOperation<IInterpedEmpty>
    {
        public void Init() { }
        
        public IInterpetedResult<IInterpetedMember<IInterpedEmpty>> Interpet(InterpetedContext interpetedContext)
        {
            return InterpetedResult.Create(TypeManager.EmptyMember(TypeManager.Empty()));
        }
    }
}