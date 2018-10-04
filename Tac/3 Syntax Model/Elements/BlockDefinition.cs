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
        public BlockDefinition(ICodeElement[] body, IScope scope, IEnumerable<ICodeElement> staticInitailizers) : base(scope ?? throw new System.ArgumentNullException(nameof(scope)), body, staticInitailizers) { }

        public override IBox<ITypeDefinition> ReturnType(IScope root)
        {
            return root.GetTypeOrThrow(RootScope.BlockType);
        }
    }

    public class BlockDefinitionMaker : IMaker<BlockDefinition>
    {
        public BlockDefinitionMaker(Func<ICodeElement[], IScope, IEnumerable<ICodeElement>, BlockDefinition> make)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        private Func<ICodeElement[], IScope, IEnumerable<ICodeElement>, BlockDefinition> Make { get; }

        public IResult<IPopulateScope<BlockDefinition>> TryMake(ElementToken elementToken, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(elementToken.Tokens)
               .Has(ElementMatcher.IsBody, out CurleyBracketToken body)
               .Has(ElementMatcher.IsDone)
               .IsMatch)
            {
                var scope = new LocalStaticScope();

                var innerMatchingContext = matchingContext.Child(scope);
                var elements = innerMatchingContext.ParseBlock(body);

                return ResultExtension.Good(new BlockDefinitionPopulateScope(scope, elements, Make));
            }

            return ResultExtension.Bad<IPopulateScope<BlockDefinition>>();
        }
    }


    public class BlockDefinitionPopulateScope : IPopulateScope<BlockDefinition>
    {
        private LocalStaticScope Scope { get; }
        private IPopulateScope<ICodeElement>[] Elements { get; }
        public Func<ICodeElement[], IScope, IEnumerable<ICodeElement>, BlockDefinition> Make { get; }

        public BlockDefinitionPopulateScope(LocalStaticScope scope, IPopulateScope<ICodeElement>[] elements, Func<ICodeElement[], IScope, IEnumerable<ICodeElement>, BlockDefinition> make)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IResolveReference<BlockDefinition> Run(IPopulateScopeContext context)
        {
            var nextContext = context.Child(this, Scope);
            return new ResolveReferanceBlockDefinition(Scope, Elements.Select(x => x.Run(nextContext)).ToArray(), Make);
        }
    }

    public class ResolveReferanceBlockDefinition : IResolveReference<BlockDefinition>
    {
        private LocalStaticScope Scope { get; }
        private IResolveReference<ICodeElement>[] ResolveReferance { get; }
        private Func<ICodeElement[], IScope, IEnumerable<ICodeElement>, BlockDefinition> Make { get; }

        public ResolveReferanceBlockDefinition(LocalStaticScope scope, IResolveReference<ICodeElement>[] resolveReferance, Func<ICodeElement[], IScope, IEnumerable<ICodeElement>, BlockDefinition> make)
        {
            Scope = scope;
            ResolveReferance = resolveReferance;
            Make = make;
        }

        public BlockDefinition Run(IResolveReferanceContext context)
        {

            var nextContext = context.Child(this, Scope);
            return Make(ResolveReferance.Select(x => x.Run(nextContext)).ToArray(), Scope, new ICodeElement[0]);
        }

        public IBox<ITypeDefinition> GetReturnType(IResolveReferanceContext context)
        {
            return context.Tree.root.GetTypeOrThrow(RootScope.BlockType);
        }
    }
}