﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model
{

    public class BlockDefinition : AbstractBlockDefinition
    {
        public BlockDefinition(ICodeElement[] body, IResolvableScope scope, IEnumerable<ICodeElement> staticInitailizers) : base(scope ?? throw new System.ArgumentNullException(nameof(scope)), body, staticInitailizers) { }
        
    }

    public class BlockDefinitionMaker : IMaker<BlockDefinition>
    {
        public BlockDefinitionMaker(Func<ICodeElement[], IResolvableScope, IEnumerable<ICodeElement>, BlockDefinition> make)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        private Func<ICodeElement[], IResolvableScope, IEnumerable<ICodeElement>, BlockDefinition> Make { get; }

        public IResult<IPopulateScope<BlockDefinition>> TryMake(ElementToken elementToken, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(elementToken.Tokens)
               .Has(ElementMatcher.IsBody, out CurleyBracketToken body)
               .Has(ElementMatcher.IsDone)
               .IsMatch)
            {
                var scope = Scope.LocalStaticScope();

                var innerMatchingContext = matchingContext.Child(scope);
                var elements = innerMatchingContext.ParseBlock(body);

                return ResultExtension.Good(new BlockDefinitionPopulateScope(scope, elements, Make));
            }

            return ResultExtension.Bad<IPopulateScope<BlockDefinition>>();
        }
    }


    public class BlockDefinitionPopulateScope : IPopulateScope<BlockDefinition>, IReturnable
    {
        private ILocalStaticScope Scope { get; }
        private IPopulateScope<ICodeElement>[] Elements { get; }
        public Func<ICodeElement[], IResolvableScope, IEnumerable<ICodeElement>, BlockDefinition> Make { get; }
        private readonly Box<IReturnable> box = new Box<IReturnable>();

        public BlockDefinitionPopulateScope(ILocalStaticScope scope, IPopulateScope<ICodeElement>[] elements, Func<ICodeElement[], IResolvableScope, IEnumerable<ICodeElement>, BlockDefinition> make)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IResolveReference<BlockDefinition> Run(IPopulateScopeContext context)
        {
            var resolvable = Scope.ToResolvable();
            var nextContext = context.Child(this, Scope);
            return new ResolveReferanceBlockDefinition(resolvable, Elements.Select(x => x.Run(nextContext)).ToArray(), Make,box);
        }

        public IBox<IReturnable> GetReturnType(IElementBuilders elementBuilders)
        {
            return box;
        }
    }

    public class ResolveReferanceBlockDefinition : IResolveReference<BlockDefinition>
    {
        private IResolvableScope Scope { get; }
        private IResolveReference<ICodeElement>[] ResolveReferance { get; }
        private Func<ICodeElement[], IResolvableScope, IEnumerable<ICodeElement>, BlockDefinition> Make { get; }
        private readonly Box<IReturnable> box;

        public ResolveReferanceBlockDefinition(
            IResolvableScope scope, 
            IResolveReference<ICodeElement>[] resolveReferance, 
            Func<ICodeElement[], IResolvableScope, IEnumerable<ICodeElement>, BlockDefinition> make,
            Box<IReturnable> box)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            ResolveReferance = resolveReferance ?? throw new ArgumentNullException(nameof(resolveReferance));
            Make = make ?? throw new ArgumentNullException(nameof(make));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public BlockDefinition Run(IResolveReferanceContext context)
        {

            var nextContext = context.Child(this, Scope);
            return box.Fill(Make(ResolveReferance.Select(x => x.Run(nextContext)).ToArray(), Scope, new ICodeElement[0]));
        }
    }
}