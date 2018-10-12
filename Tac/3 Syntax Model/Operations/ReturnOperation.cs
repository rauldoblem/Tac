﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model.Operations
{
    public class ReturnOperation : ICodeElement
    {
        public ReturnOperation(ICodeElement result)
        {
            Result = result;
        }

        public ICodeElement Result { get; }
        
        public IReturnable ReturnType(IElementBuilders elementBuilders)
        {
            return elementBuilders.EmptyType();
        }
    }
    
    public class TrailingOperationMaker<T> : IOperationMaker<T>
        where T : class, ICodeElement
    {
        public TrailingOperationMaker(string name, Func<ICodeElement, T> make)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public string Name { get; }
        private Func<ICodeElement, T> Make { get; }

        public IResult<IPopulateScope<T>> TryMake(IEnumerable<IToken> tokens, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(tokens)
            .Has(ElementMatcher.IsTrailingOperation(Name), out var perface, out var _)
            .IsMatch)
            {
                var left = matchingContext.ParseLine(perface);
                
                return ResultExtension.Good(new TrailingPopulateScope<T>(left,Make));
            }
            return ResultExtension.Bad<IPopulateScope<T>>();
        }
        
    }

    public class TrailingPopulateScope<T> : IPopulateScope<T>
        where T : ICodeElement
    {
        private readonly IPopulateScope<ICodeElement> left;
        private readonly Func<ICodeElement, T> make;
        private readonly DelegateBox<IReturnable> box = new DelegateBox<IReturnable>();

        public TrailingPopulateScope(IPopulateScope<ICodeElement> left, Func<ICodeElement, T> make)
        {
            this.left = left ?? throw new ArgumentNullException(nameof(left));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IBox<IReturnable> GetReturnType(IElementBuilders elementBuilders)
        {
            throw new NotImplementedException();
        }

        public IResolveReference<T> Run(IPopulateScopeContext context)
        {
            var nextContext = context.Child(this);
            return new TrailingResolveReferance<T>(left.Run(nextContext),  make, box);
        }
    }



    public class TrailingResolveReferance<T> : IResolveReference<T>
        where T : ICodeElement
    {
        public readonly IResolveReference<ICodeElement> left;
        private readonly Func<ICodeElement, T> make;
        private readonly DelegateBox<IReturnable> box;

        public TrailingResolveReferance(IResolveReference<ICodeElement> resolveReferance1,  Func<ICodeElement, T> make, DelegateBox<IReturnable> box)
        {
            left = resolveReferance1 ?? throw new ArgumentNullException(nameof(resolveReferance1));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public IBox<IReturnable> GetReturnType(IResolveReferanceContext context)
        {
            return box;
        }

        public T Run(IResolveReferanceContext context)
        {
            var nextContext = context.Child(this);
            var res = make(left.Run(nextContext));
            box.Set(()=>res.ReturnType(context.ElementBuilders));
            return res;
        }
    }


    public class ReturnOperationMaker : TrailingOperationMaker<ReturnOperation>
    {
        public ReturnOperationMaker(Func<ICodeElement, ReturnOperation> make) : base("return", make)
        {
        }
    }
}