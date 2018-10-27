﻿using Prototypist.LeftToRight;
using System;
using System.Collections.Generic;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model.Operations
{
    public interface IPathOperation : IBinaryOperation<ICodeElement, IMemberReferance>
    {
    }

    public class WeakPathOperation : BinaryOperation<IWeakCodeElement, IWeakCodeElement>
    {
        public const string Identifier = ".";

        public WeakPathOperation(IWeakCodeElement left, IWeakCodeElement right) : base(left, right)
        {
        }

        public override IWeakReturnable Returns(IElementBuilders elementBuilders)
        {
            // should this check to see if the left contains the member defined on the rhs?
            return right.Cast<WeakMemberDefinition>();
        }
    }


    public class PathOperationMaker : IOperationMaker<WeakPathOperation>
    {
        public PathOperationMaker( BinaryOperation.Make<WeakPathOperation> make
            )
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }
        
        private BinaryOperation.Make<WeakPathOperation> Make { get; }

        public IResult<IPopulateScope<WeakPathOperation>> TryMake(IEnumerable<IToken> tokens, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(tokens)
            .Has(ElementMatcher.IsBinaryOperation(WeakPathOperation.Identifier), out var perface, out var token, out var rhs)
            .IsMatch)
            {
                var left = matchingContext.ParseLine(perface);
                var right = matchingContext.ExpectPathPart(left.GetReturnType(matchingContext.Builders)).ParseParenthesisOrElement(rhs);

                return ResultExtension.Good(new BinaryPopulateScope<WeakPathOperation>(left, right, Make, new Converter()));
            }

            return ResultExtension.Bad<IPopulateScope<WeakPathOperation>>();
        }


        private class Converter : IConverter<WeakPathOperation>
        {
            public T Convert<T>(IOpenBoxesContext<T> context, WeakPathOperation co)
            {
                return context.PathOperation(co);
            }
        }

    }
}
