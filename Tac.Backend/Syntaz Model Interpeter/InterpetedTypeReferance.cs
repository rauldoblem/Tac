﻿using System;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Syntaz_Model_Interpeter;
using Tac.Syntaz_Model_Interpeter.Run_Time_Objects;

namespace Tac.Backend.Syntaz_Model_Interpeter
{
    internal class InterpetedTypeReferance : IInterpetedOperation<IInterpetedData>, ITypeReferance
    {
        public IVerifiableType TypeDefinition
        {
            get; private set;
        }

        public T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.TypeReferance(this);
        }

        public IInterpetedResult<IInterpetedMember<IInterpetedData>> Interpet(InterpetedContext interpetedContext)
        {
            return InterpetedResult.Create<IInterpetedMember<IInterpetedData>>(new InterpetedMember<IInterpetedData>( new RunTimeType()));
        }

        public IVerifiableType Returns()
        {
            return this;
        }

        internal void Init(IVerifiableType typeDefinition)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
        }

        void IInterpetedOperation.Interpet(InterpetedContext interpetedContext)
        {
            Interpet(interpetedContext);
        }
    }
}