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
using Tac.Semantic_Model;

namespace Tac.Parser
{

    internal partial class MakerRegistry
    {
        private static readonly WithConditions<IPopulateScope<IFrontendCodeElement>> StaticModuleDefinitionMaker = AddElementMakers(
            () => new ModuleDefinitionMaker(),
            MustBeBefore<IPopulateScope<IFrontendCodeElement>>(typeof(MemberMaker)));
#pragma warning disable IDE0052 // Remove unread private members
        private readonly WithConditions<IPopulateScope<IFrontendCodeElement>> ModuleDefinitionMaker = StaticModuleDefinitionMaker;
#pragma warning restore IDE0052 // Remove unread private members
    }
}


namespace Tac.Semantic_Model
{


    internal class WeakModuleDefinition : IScoped, IConvertableFrontendCodeElement<IModuleDefinition>, IFrontendType
    {
        public WeakModuleDefinition(IResolvableScope scope, IEnumerable<IIsPossibly<IFrontendCodeElement>> staticInitialization, IKey Key)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            StaticInitialization = staticInitialization ?? throw new ArgumentNullException(nameof(staticInitialization));
            this.Key = Key ?? throw new ArgumentNullException(nameof(Key));
        }
        
        public IResolvableScope Scope { get; }
        public IEnumerable<IIsPossibly<IFrontendCodeElement>> StaticInitialization { get; }

        public IKey Key
        {
            get;
        }

        public IBuildIntention<IModuleDefinition> GetBuildIntention(IConversionContext context)
        {
            var (toBuild, maker) = ModuleDefinition.Create();
            return new BuildIntention<IModuleDefinition>(toBuild, () =>
            {
                maker.Build(
                    Scope.Convert(context), 
                    StaticInitialization
                        .Select(x=>x.GetOrThrow().PossiblyConvert(context))
                        .OfType<IIsDefinately<ICodeElement>>()
                        .Select(x=>x.Value)
                        .ToArray(),
                    Key);
            });
        }

        public IIsPossibly<IFrontendType> Returns()
        {
            return Possibly.Is(this);
        }
    }
    
    internal class ModuleDefinitionMaker : IMaker<IPopulateScope<WeakModuleDefinition>>
    {
        public ModuleDefinitionMaker()
        {
        }
        

        public ITokenMatching<IPopulateScope<WeakModuleDefinition>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .Has(new KeyWordMaker("module"), out _)
                .Has(new NameMaker(), out var name)
                .Has(new BodyMaker(), out var third);
            if (matching is IMatchedTokenMatching matched)
            {
                var elements = matching.Context.ParseBlock(third);
                var nameKey = new NameKey(name.Item);

                return TokenMatching<IPopulateScope<WeakModuleDefinition>>.MakeMatch(
                    matched.Tokens,
                    matched.Context, 
                    new ModuleDefinitionPopulateScope(elements, nameKey));

            }
            return TokenMatching<IPopulateScope<WeakModuleDefinition>>.MakeNotMatch(
                    matching.Context);
        }


        public static IPopulateScope<WeakModuleDefinition> PopulateScope(IPopulateScope<IConvertableFrontendCodeElement<ICodeElement>>[] elements,
                NameKey nameKey)
        {
            return new ModuleDefinitionPopulateScope(elements,
                nameKey);
        }
        public static IPopulateBoxes<WeakModuleDefinition> PopulateBoxes(IResolvableScope scope,
                IPopulateBoxes<IConvertableFrontendCodeElement<ICodeElement>>[] resolveReferance,
                NameKey nameKey)
        {
            return new ModuleDefinitionResolveReferance(scope,
               resolveReferance,
               nameKey);
        }



        private class ModuleDefinitionPopulateScope : IPopulateScope<WeakModuleDefinition>
        {
            private readonly IPopulateScope<IFrontendCodeElement>[] elements;
            private readonly NameKey nameKey;

            public ModuleDefinitionPopulateScope(
                IPopulateScope<IFrontendCodeElement>[] elements,
                NameKey nameKey)
            {
                this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
                this.nameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            }

            public IResolvelizeScope<WeakModuleDefinition> Run(IPopulatableScope scope, IPopulateScopeContext context)
            {
                var myScope = scope.AddChild();
                return new ModuleDefinitionFinalizeScope(
                    myScope.GetResolvelizableScope(),
                    elements.Select(x => x.Run(myScope, context)).ToArray(),
                    nameKey);
            }
        }

        private class ModuleDefinitionFinalizeScope : IResolvelizeScope<WeakModuleDefinition>
        {
            private readonly IResolvelizableScope scope;
            private readonly IResolvelizeScope<IFrontendCodeElement>[] elements;
            private readonly NameKey nameKey;

            public ModuleDefinitionFinalizeScope(
                IResolvelizableScope scope,
                IResolvelizeScope<IFrontendCodeElement>[] elements,
                NameKey nameKey)
            {
                this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
                this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
                this.nameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            }

            public IPopulateBoxes<WeakModuleDefinition> Run(IResolvableScope parent, IFinalizeScopeContext context)
            {
                var finalScope = scope.FinalizeScope(parent);
                

                return new ModuleDefinitionResolveReferance(
                    finalScope,
                    elements.Select(x => x.Run(finalScope,context)).ToArray(),
                    nameKey);
            }
        }

        private class ModuleDefinitionResolveReferance : IPopulateBoxes<WeakModuleDefinition>
        {
            private readonly IResolvableScope scope;
            private readonly IPopulateBoxes<IFrontendCodeElement>[] resolveReferance;
            private readonly NameKey nameKey;

            public ModuleDefinitionResolveReferance(
                IResolvableScope scope,
                IPopulateBoxes<IFrontendCodeElement>[] resolveReferance,
                NameKey nameKey)
            {
                this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
                this.resolveReferance = resolveReferance ?? throw new ArgumentNullException(nameof(resolveReferance));
                this.nameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            }

            public IIsPossibly<WeakModuleDefinition> Run(IResolvableScope _, IResolveReferenceContext context)
            {
                var innerRes = new WeakModuleDefinition(
                        scope,
                        resolveReferance.Select(x => x.Run(scope,context)).ToArray(),
                        nameKey);

                var res = Possibly.Is(innerRes);

                return res;
            }
        }
    }
}
