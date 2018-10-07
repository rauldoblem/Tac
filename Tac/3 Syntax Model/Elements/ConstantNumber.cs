﻿using System;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model.Operations
{
    public class ConstantNumber : ICodeElement
    {
        public ConstantNumber(double value) 
        {
            Value = value;
        }

        public double Value { get; }

        public IBox<ITypeDefinition> ReturnType(RootScope rootScope)
        {
            return rootScope.NumberType;
        }
    }

    public class ConstantNumberMaker : IMaker<ConstantNumber>
    {
        private readonly Func<double, ConstantNumber> make;

        public ConstantNumberMaker(Func<double, ConstantNumber> Make) {
            make = Make ?? throw new ArgumentNullException(nameof(Make));
        }

        public IResult<IPopulateScope<ConstantNumber>> TryMake(ElementToken elementToken, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(elementToken.Tokens)
                .Has(ElementMatcher.IsNumber, out double dub)
                .Has(ElementMatcher.IsDone)
                .IsMatch)
            {
                return ResultExtension.Good(new ConstantNumberPopulateScope(dub, make));
            }
            
            return ResultExtension.Bad<IPopulateScope<ConstantNumber>>();
        }
    }
    
    public class ConstantNumberPopulateScope : IPopulateScope<ConstantNumber>
    {
        private readonly double dub;
        private readonly Func<double, ConstantNumber> make;

        public ConstantNumberPopulateScope(double dub, Func<double, ConstantNumber> Make)
        {
            this.dub = dub;
            make = Make;
        }

        public IResolveReference<ConstantNumber> Run(IPopulateScopeContext context)
        {
            return new ConstantNumberResolveReferance(dub, make);
        }
    }

    public class ConstantNumberResolveReferance : IResolveReference<ConstantNumber>
    {
        private readonly double dub;
        private readonly Func<double, ConstantNumber> make;

        public ConstantNumberResolveReferance(double dub, Func<double, ConstantNumber> Make)
        {
            this.dub = dub;
            make = Make;
        }

        public IBox<ITypeDefinition> GetReturnType(IResolveReferanceContext context)
        {
            return context.RootScope.NumberType;
        }

        public ConstantNumber Run(IResolveReferanceContext context)
        {
            return make(dub);
        }
    }
}
