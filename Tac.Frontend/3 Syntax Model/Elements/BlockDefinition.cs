﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tac.Frontend;
using Tac.Frontend._2_Parser;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Model.Instantiated;
using Tac.New;
using Tac.Parser;
using static Tac.Frontend.TransformerExtensions;

namespace Tac.Semantic_Model
{

    internal class WeakBlockDefinition : WeakAbstractBlockDefinition<IBlockDefinition>
    {
        public WeakBlockDefinition(
            IIsPossibly<IFrontendCodeElement<ICodeElement>>[] body,
            IResolvableScope scope,
            IEnumerable<IIsPossibly<IFrontendCodeElement<ICodeElement>>> staticInitailizers) : 
            base(scope, body, staticInitailizers) { }

        public override IBuildIntention<IBlockDefinition> GetBuildIntention(ConversionContext context)
        {
            var (toBuild, maker) = BlockDefinition.Create();
            return new BuildIntention<IBlockDefinition>(toBuild, () =>
            {
                maker.Build(
                    Scope.Convert(context),
                    Body.Select(x => x.GetOrThrow().Convert(context)).ToArray(),
                    StaticInitailizers.Select(x => x.GetOrThrow().Convert(context)).ToArray());
            });
        }

        public override IIsPossibly<IFrontendType<IVerifiableType>> Returns()
        {
            return Possibly.Is(new _3_Syntax_Model.Elements.Atomic_Types.BlockType());
        }
    }

    internal class BlockDefinitionMaker : IMaker<IPopulateScope<WeakBlockDefinition>>
    {
        public BlockDefinitionMaker()
        {
        }
        
        public ITokenMatching<IPopulateScope<WeakBlockDefinition>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var match = tokenMatching
               .Has(new BodyMaker(), out var body);

            if (match is IMatchedTokenMatching
               matched)
            {
                var elements = tokenMatching.Context.ParseBlock(body);

                return TokenMatching<IPopulateScope<WeakBlockDefinition>>.MakeMatch(matched.Tokens.Skip(1).ToArray(), matched.Context, new BlockDefinitionPopulateScope(elements));
            }

            return TokenMatching<IPopulateScope<WeakBlockDefinition>>.MakeNotMatch(tokenMatching.Context);
        }
    }
    
    internal class BlockDefinitionPopulateScope : IPopulateScope<WeakBlockDefinition>
    {
        // TODO object??
        // is it worth adding another T?
        // this is the type the backend owns
        private IPopulateScope<IFrontendCodeElement<ICodeElement>>[] Elements { get; }

        public BlockDefinitionPopulateScope(IPopulateScope<IFrontendCodeElement<ICodeElement>>[] elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public IPopulateBoxes<WeakBlockDefinition> Run(IPopulateScopeContext context)
        {
            var nextContext = context.Child();
            return new ResolveReferanceBlockDefinition(
                nextContext.GetResolvableScope(), 
                Elements.Select(x => x.Run(nextContext)).ToArray());
        }

        public IBox<IIsPossibly<IFrontendType<IVerifiableType>>> GetReturnType()
        {
            return new Box<IIsPossibly<IFrontendType<IVerifiableType>>>(Possibly.Is(new _3_Syntax_Model.Elements.Atomic_Types.BlockType()));
        }
    }

    internal class ResolveReferanceBlockDefinition : IPopulateBoxes<WeakBlockDefinition>
    {
        private IResolvableScope Scope { get; }
        private IPopulateBoxes<IFrontendCodeElement<ICodeElement>>[] ResolveReferance { get; }

        public ResolveReferanceBlockDefinition(
            IResolvableScope scope, 
            IPopulateBoxes<IFrontendCodeElement<ICodeElement>>[] resolveReferance)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            ResolveReferance = resolveReferance ?? throw new ArgumentNullException(nameof(resolveReferance));
        }

        public IIsPossibly<WeakBlockDefinition> Run(IResolveReferenceContext context)
        {
            return 
                    Possibly.Is(
                        new WeakBlockDefinition(
                            ResolveReferance.Select(x => x.Run(context)).ToArray(), 
                            Scope, 
                            new IIsPossibly<IFrontendCodeElement<ICodeElement>>[0]));
        }
        
    }
    

}