﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{

    public class ModuleDefinition : IScoped, ICodeElement, ITypeDefinition
    {
        public ModuleDefinition(IScope scope, IEnumerable<ICodeElement> staticInitialization)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            StaticInitialization = staticInitialization ?? throw new ArgumentNullException(nameof(staticInitialization));
        }
        
        public IScope Scope { get; }
        public IEnumerable<ICodeElement> StaticInitialization { get; }

        public IBox<ITypeDefinition> ReturnType(ScopeTree scope)
        {
            return new Box<ITypeDefinition>(this);
        }
    }


    public class ModuleDefinitionMaker : IMaker<ModuleDefinition>
    {
        public ModuleDefinitionMaker(Func<IScope, IEnumerable<ICodeElement>, ModuleDefinition> make)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        private Func<IScope, IEnumerable<ICodeElement>, ModuleDefinition> Make { get; }

        public IResult<IPopulateScope<ModuleDefinition>> TryMake(ElementToken elementToken, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(elementToken.Tokens)
                            .Has(ElementMatcher.KeyWord("module"), out var frist)
                            .Has(ElementMatcher.IsBody, out CurleyBacketToken third)
                            .Has(ElementMatcher.IsDone)
                            .IsMatch)
            {

                var scope = new StaticScope();

                var elementMatchingContext = matchingContext.Child(scope);
                var elements = elementMatchingContext.ParseBlock(third);
                
                return ResultExtension.Good(new ModuleDefinitionPopulateScope(scope, elements, Make));

            }
            return ResultExtension.Bad<IPopulateScope<ModuleDefinition>>();
        }
    }
    
    public class ModuleDefinitionPopulateScope : IPopulateScope<ModuleDefinition>
    {
        private readonly StaticScope scope;
        private readonly IPopulateScope<ICodeElement>[] elements;
        private readonly Func<IScope, IEnumerable<ICodeElement>, ModuleDefinition> make;

        public ModuleDefinitionPopulateScope(StaticScope scope, IPopulateScope<ICodeElement>[] elements, Func<IScope, IEnumerable<ICodeElement>, ModuleDefinition> make)
        {
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IResolveReferance<ModuleDefinition> Run(IPopulateScopeContext context)
        {
            var nextContext = context.Child(this, scope);
            return new ModuleDefinitionResolveReferance(scope, elements.Select(x => x.Run(nextContext)).ToArray(), make);
        }

    }

    public class ModuleDefinitionResolveReferance : IResolveReferance<ModuleDefinition>
    {
        private readonly StaticScope scope;
        private readonly IResolveReferance<ICodeElement>[] resolveReferance;
        private readonly Func<IScope, IEnumerable<ICodeElement>, ModuleDefinition> make;

        public ModuleDefinitionResolveReferance(StaticScope scope, IResolveReferance<ICodeElement>[] resolveReferance, Func<IScope, IEnumerable<ICodeElement>, ModuleDefinition> make)
        {
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.resolveReferance = resolveReferance ?? throw new ArgumentNullException(nameof(resolveReferance));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public ModuleDefinition Run(IResolveReferanceContext context)
        {
            var nextContext = context.Child(this, scope);
            return make(scope, resolveReferance.Select(x => x.Run(nextContext)).ToArray());
        }


        public IBox<ITypeDefinition> GetReturnType(IResolveReferanceContext context)
        {
            return context.Tree.Root.GetTypeOrThrow(RootScope.ModuleType.Key);
        }
    }

}
