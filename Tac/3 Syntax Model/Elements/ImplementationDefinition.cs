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
    // really really not sure how these work atm
    // for now they just hold everything you need to ake a method

    public class ImplementationDefinition: ITypeDefinition
    {
        public ImplementationDefinition(MemberDefinition contextDefinition, ITypeDefinition outputType, MemberDefinition parameterDefinition, IEnumerable<ICodeElement> metohdBody, IScope scope, IEnumerable<ICodeElement> staticInitializers)
        {
            ContextDefinition = contextDefinition ?? throw new ArgumentNullException(nameof(contextDefinition));
            OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            MethodBody = metohdBody ?? throw new ArgumentNullException(nameof(metohdBody));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            StaticInitialzers = staticInitializers ?? throw new ArgumentNullException(nameof(staticInitializers));
        }

        // dang! these could also be inline definitions 
        public IBox<ITypeDefinition> ContextType
        {
            get
            {
                return ContextDefinition.Type;
            }
        }
        public IBox<ITypeDefinition> InputType
        {
            get
            {
                return ParameterDefinition.Type;
            }
        }
        public ITypeDefinition OutputType { get; }
        public MemberDefinition ContextDefinition { get; }
        public MemberDefinition ParameterDefinition { get; }
        public IScope Scope { get; }
        public IEnumerable<ICodeElement> MethodBody { get; }
        public IEnumerable<ICodeElement> StaticInitialzers { get; }
        
        public IBox<ITypeDefinition> ReturnType(ScopeStack scope) {
                return scope.GetGenericType(new GenericExplicitTypeName(RootScope.ImplementationType.Name,new ITypeDefinition[] { ContextType, InputType, OutputType }));
        }

        public IBox<ITypeDefinition> GetTypeDefinition(ScopeStack scopeStack)
        {
            return scopeStack.GetGenericType(new GenericExplicitTypeName(RootScope.ImplementationType.Name, new ITypeDefinition[] { ContextType, InputType, OutputType }));
        }
    }

    public class ImplementationDefinitionMaker : IMaker<ImplementationDefinition>
    {
        public ImplementationDefinitionMaker(Func<MemberDefinition  , MemberDefinition , IBox<ITypeDefinition>, IEnumerable<ICodeElement> , IScope , IEnumerable<ICodeElement> , ImplementationDefinition> make,
            IElementBuilders elementBuilders)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
            ElementBuilders = elementBuilders ?? throw new ArgumentNullException(nameof(elementBuilders));
        }

        private Func<MemberDefinition,  MemberDefinition, IBox<ITypeDefinition>, IEnumerable<ICodeElement>, IScope, IEnumerable<ICodeElement>, ImplementationDefinition> Make { get; }
        private IElementBuilders ElementBuilders { get; }

        public bool TryMake(ElementToken elementToken, ElementMatchingContext matchingContext, out IPopulateScope<ImplementationDefinition> result)
        {
            if (TokenMatching.Start(elementToken.Tokens)
                .Has(ElementMatcher.KeyWord("implementation"), out var _)
                .Has(ElementMatcher.Generic3, out AtomicToken contextType, out AtomicToken inputType, out AtomicToken outputType)
                .OptionalHas(ElementMatcher.IsName, out AtomicToken contextName)
                .OptionalHas(ElementMatcher.IsName, out AtomicToken parameterName)
                .Has(ElementMatcher.IsBody, out CurleyBacketToken body)
                .Has(ElementMatcher.IsDone)
                .IsMatch)
            {

                var methodScope = new MethodScope();

                var newMatchingContext = matchingContext.Child(methodScope);
                var elements = newMatchingContext.ParseBlock(body);

                var contextDefinition = ElementBuilders.MemberDefinition.Make(
                        false,
                        new ExplicitMemberName(parameterName?.Item ?? "context"),
                        new ExplicitTypeName(contextType.Item)
                        );
                

                var parameterDefinition = ElementBuilders.MemberDefinition.Make(
                        false,
                        new ExplicitMemberName(parameterName?.Item ?? "input"),
                        new ExplicitTypeName(inputType.Item)
                        );

                var outputTypeName= new ExplicitTypeName(outputType.Item);

                result = new PopulateScopeImplementationDefinition(contextDefinition, parameterDefinition, methodScope, elements, outputTypeName,Make);
                return true;
            }

            result = default;
            return false;
        }
    }

    public class PopulateScopeImplementationDefinition : IPopulateScope<ImplementationDefinition>
    {
        private readonly IPopulateScope<MemberDefinition> contextDefinition;
        private readonly IPopulateScope<MemberDefinition> parameterDefinition;
        private readonly MethodScope methodScope;
        private readonly IPopulateScope<ICodeElement>[] elements;
        private readonly ExplicitTypeName outputTypeName;
        private readonly Func<MemberDefinition, MemberDefinition, IBox<ITypeDefinition>, IEnumerable<ICodeElement>, IScope, IEnumerable<ICodeElement>, ImplementationDefinition> make;

        public PopulateScopeImplementationDefinition(IPopulateScope<MemberDefinition> contextDefinition, IPopulateScope<MemberDefinition> parameterDefinition, MethodScope methodScope, IPopulateScope<ICodeElement>[] elements, ExplicitTypeName outputTypeName, Func<MemberDefinition, MemberDefinition, IBox<ITypeDefinition>, IEnumerable<ICodeElement>, IScope, IEnumerable<ICodeElement>, ImplementationDefinition> make)
        {
            this.contextDefinition = contextDefinition ?? throw new ArgumentNullException(nameof(contextDefinition));
            this.parameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            this.methodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.outputTypeName = outputTypeName ?? throw new ArgumentNullException(nameof(outputTypeName));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IResolveReferance<ImplementationDefinition> Run(IPopulateScopeContext context)
        {
            var newContext = context.Child(this, methodScope);
            return new ImplementationDefinitionResolveReferance(contextDefinition.Run(newContext), parameterDefinition.Run(newContext), methodScope, elements.Select(x => x.Run(newContext)).ToArray(), outputTypeName, make);
        }

    }

    public class ImplementationDefinitionResolveReferance : IResolveReferance<ImplementationDefinition>
    {
        private readonly IResolveReferance<MemberDefinition> contextDefinition;
        private readonly IResolveReferance<MemberDefinition> parameterDefinition;
        private readonly MethodScope methodScope;
        private readonly IResolveReferance<ICodeElement>[] elements;
        private readonly ExplicitTypeName outputTypeName;
        private readonly Func<MemberDefinition, MemberDefinition, IBox<ITypeDefinition>, IEnumerable<ICodeElement>, IScope, IEnumerable<ICodeElement>, ImplementationDefinition> make;

        public ImplementationDefinitionResolveReferance(IResolveReferance<MemberDefinition> contextDefinition, IResolveReferance<MemberDefinition> parameterDefinition, MethodScope methodScope, IResolveReferance<ICodeElement>[] elements, ExplicitTypeName outputTypeName, Func<MemberDefinition, MemberDefinition, IBox<ITypeDefinition>, IEnumerable<ICodeElement>, IScope, IEnumerable<ICodeElement>, ImplementationDefinition> make)
        {
            this.contextDefinition = contextDefinition ?? throw new ArgumentNullException(nameof(contextDefinition));
            this.parameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            this.methodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.outputTypeName = outputTypeName ?? throw new ArgumentNullException(nameof(outputTypeName));
        }

        public ImplementationDefinition Run(IResolveReferanceContext context)
        {
            var newContext = context.Child(this, methodScope);
            return make(contextDefinition.Run(newContext), parameterDefinition.Run(newContext), new ScopeStack(context.Tree, methodScope).GetType(outputTypeName), elements.Select(x => x.Run(newContext)).ToArray(), methodScope, new ICodeElement[0]);
        }
    }
}
