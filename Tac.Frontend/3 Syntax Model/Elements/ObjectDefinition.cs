﻿using Prototypist.LeftToRight;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.Frontend;
using Tac.Frontend._2_Parser;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Model.Instantiated;
using Tac.Model.Operations;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;
using Tac.Semantic_Model;

namespace Tac.Parser
{

    internal partial class MakerRegistry
    {
        private static readonly WithConditions<IPopulateScope<IFrontendCodeElement>> StaticObjectDefinitionMaker = AddElementMakers(
            () => new ObjectDefinitionMaker(),
            MustBeBefore<IPopulateScope<IFrontendCodeElement>>(typeof(MemberMaker)));
#pragma warning disable IDE0052 // Remove unread private members
        private readonly WithConditions<IPopulateScope<IFrontendCodeElement>> ObjectDefinitionMaker = StaticObjectDefinitionMaker;
#pragma warning restore IDE0052 // Remove unread private members
    }
}


namespace Tac.Semantic_Model
{

    internal class WeakObjectDefinition: IConvertableFrontendCodeElement<IObjectDefiniton>,  IScoped, IFrontendType
    {
        public WeakObjectDefinition(IResolvableScope scope, IEnumerable<IIsPossibly<WeakAssignOperation>> assigns, ImplicitKey key) {
            if (assigns == null)
            {
                throw new ArgumentNullException(nameof(assigns));
            }

            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Assignments = assigns.ToArray();
        }

        public IResolvableScope Scope { get; }
        public IIsPossibly<WeakAssignOperation>[] Assignments { get; }

        public IKey Key
        {
            get;
        }

        public IBuildIntention<IObjectDefiniton> GetBuildIntention(IConversionContext context)
        {
            var (toBuild, maker) = ObjectDefiniton.Create();
            return new BuildIntention<IObjectDefiniton>(toBuild, () =>
            {
                maker.Build(Scope.Convert(context), 
                    Assignments.Select(x => x.GetOrThrow().Convert(context)).ToArray());
            });
        }

        public IIsPossibly<IFrontendType> Returns()
        {
            return Possibly.Is(this);
        }
    }

    internal class ObjectDefinitionMaker : IMaker<IPopulateScope<WeakObjectDefinition>>
    {
        public ObjectDefinitionMaker()
        {
        }

        public ITokenMatching<IPopulateScope<WeakObjectDefinition>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .Has(new KeyWordMaker("object"), out var _)
                .Has(new BodyMaker(), out var block);
            if (matching is IMatchedTokenMatching matched)
            {

                var elements = tokenMatching.Context.ParseBlock(block);
                
                return TokenMatching<IPopulateScope<WeakObjectDefinition>>.MakeMatch(
                    matched.Tokens,
                    matched.Context, 
                    new ObjectDefinitionPopulateScope(elements));
            }
            return TokenMatching<IPopulateScope<WeakObjectDefinition>>.MakeNotMatch(
                    matching.Context);
        }

        public static IPopulateScope<WeakObjectDefinition> PopulateScope(IPopulateScope<IConvertableFrontendCodeElement<ICodeElement>>[] elements)
        {
            return new ObjectDefinitionPopulateScope(elements);
        }
        public static IPopulateBoxes<WeakObjectDefinition> PopulateBoxes(IResolvableScope scope,
                IPopulateBoxes<IConvertableFrontendCodeElement<ICodeElement>>[] elements,
                Box<IIsPossibly<IFrontendType>> box,
                ImplicitKey key)
        {
            return new ResolveReferanceObjectDefinition( scope,
                 elements,
                box,
                key);
        }

        private class ObjectDefinitionPopulateScope : IPopulateScope<WeakObjectDefinition>
        {
            private readonly IPopulateScope<IFrontendCodeElement>[] elements;
            private readonly Box<IIsPossibly<IFrontendType>> box = new Box<IIsPossibly<IFrontendType>>();

            public ObjectDefinitionPopulateScope(IPopulateScope<IFrontendCodeElement>[] elements)
            {
                this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            }

            public IBox<IIsPossibly<IFrontendType>> GetReturnType()
            {
                return box;
            }

            public IPopulateBoxes<WeakObjectDefinition> Run(IPopulateScopeContext context)
            {
                var nextContext = context.Child();
                var key = new ImplicitKey();
                nextContext.Scope.TryAddType(key, box);
                return new ResolveReferanceObjectDefinition(
                    nextContext.GetResolvableScope(),
                    elements.Select(x => x.Run(nextContext)).ToArray(),
                    box,
                    key);
            }
        }

        private class ResolveReferanceObjectDefinition : IPopulateBoxes<WeakObjectDefinition>
        {
            private readonly IResolvableScope scope;
            private readonly IPopulateBoxes<IFrontendCodeElement>[] elements;
            private readonly Box<IIsPossibly<IFrontendType>> box;
            private readonly ImplicitKey key;

            public ResolveReferanceObjectDefinition(
                IResolvableScope scope,
                IPopulateBoxes<IFrontendCodeElement>[] elements,
                Box<IIsPossibly<IFrontendType>> box,
                ImplicitKey key)
            {
                this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
                this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
                this.box = box ?? throw new ArgumentNullException(nameof(box));
                this.key = key ?? throw new ArgumentNullException(nameof(key));
            }

            public IIsPossibly<WeakObjectDefinition> Run(IResolveReferenceContext context)
            {
                var innerRes = new WeakObjectDefinition(
                            scope,
                            elements.Select(x => x.Run(context).Cast<IIsPossibly<WeakAssignOperation>>()).ToArray(),
                            key);
                var res = Possibly.Is(innerRes);

                box.Fill(innerRes.Returns());

                return res;
            }
        }
    }

}
