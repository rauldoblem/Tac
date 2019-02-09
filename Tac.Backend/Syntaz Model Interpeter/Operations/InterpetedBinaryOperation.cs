﻿using System;
using Tac.Syntaz_Model_Interpeter.Run_Time_Objects;

namespace Tac.Syntaz_Model_Interpeter
{
    internal abstract class InterpetedBinaryOperation<TLeft,TRight,TRes>: IInterpetedOperation<TRes>
        where TLeft: IInterpetedData
        where TRight: IInterpetedData
        where TRes: IInterpetedData
    {
        public void Init(IInterpetedOperation<TLeft> left, IInterpetedOperation<TRight> right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public abstract IInterpetedResult<TRes> Interpet(InterpetedContext interpetedContext);

        public IInterpetedOperation<TLeft> Left { get; private set; }
        public IInterpetedOperation<TRight> Right { get; private set; }
    }
}