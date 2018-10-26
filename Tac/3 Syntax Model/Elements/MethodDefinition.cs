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

    public interface IMethodDefinition: IBlockDefinition {

        IReturnable InputType { get; }
        IReturnable OutputType { get; }
        IMemberDefinition ParameterDefinition { get; }
    }

    public class WeakMethodDefinition : WeakAbstractBlockDefinition, IWeakReturnable
    {
        public delegate WeakMethodDefinition Make(
            IBox<IWeakReturnable> outputType,
            IBox<WeakMemberDefinition> parameterDefinition,
            IWeakCodeElement[] body,
            IWeakFinalizedScope scope,
            IEnumerable<IWeakCodeElement> staticInitializers);

        public WeakMethodDefinition(
            IBox<IWeakReturnable> outputType, 
            IBox<WeakMemberDefinition> parameterDefinition,
            IWeakCodeElement[] body,
            IWeakFinalizedScope scope,
            IEnumerable<IWeakCodeElement> staticInitializers) : base(scope ?? throw new ArgumentNullException(nameof(scope)), body, staticInitializers)
        {
            OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
        }
        


        public IBox<IWeakReturnable> InputType
        {
            get
            {
                return ParameterDefinition.GetValue().Type;
            }
        }
        public IBox<IWeakReturnable> OutputType { get; }
        public IBox<WeakMemberDefinition> ParameterDefinition { get; }
    }


    public class MethodDefinitionMaker : IMaker<WeakMethodDefinition>
    {
        public MethodDefinitionMaker(
            WeakMethodDefinition.Make make,
            IElementBuilders elementBuilders)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
            ElementBuilders = elementBuilders ?? throw new ArgumentNullException(nameof(elementBuilders));
        }

        private WeakMethodDefinition.Make Make { get; }
        private IElementBuilders ElementBuilders { get; }

        public IResult<IPopulateScope<WeakMethodDefinition>> TryMake(ElementToken elementToken, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(elementToken.Tokens)
                .Has(ElementMatcher.KeyWord("method"), out var _)
                .Has(ElementMatcher.Generic2, out AtomicToken inputType, out AtomicToken outputType)
                .OptionalHas(ElementMatcher.IsName, out AtomicToken parameterName)
                .Has(ElementMatcher.IsBody, out CurleyBracketToken body)
                .Has(ElementMatcher.IsDone)
                .IsMatch)
            {
                var elements = matchingContext.ParseBlock(body);
                
                var parameterDefinition = new MemberDefinitionPopulateScope(
                        parameterName?.Item ?? "input",
                        false,
                         new NameKey(inputType.Item),
                        matchingContext.Builders.MemberReferance
                        );
                

                var outputTypeName = new NameKey(outputType.Item);
                
                return ResultExtension.Good(new MethodDefinitionPopulateScope(
                    parameterDefinition,
                    elements, 
                    outputTypeName, 
                    Make));
            }

            return ResultExtension.Bad<IPopulateScope<WeakMethodDefinition>>();
        }
    }

    public class MethodDefinitionPopulateScope : IPopulateScope<WeakMethodDefinition>
    {
        private readonly IPopulateScope<WeakMemberReferance> parameterDefinition;
        private readonly IPopulateScope<IWeakCodeElement>[] elements;
        private readonly NameKey outputTypeName;
        private readonly WeakMethodDefinition.Make make;
        private readonly Box<IWeakReturnable> box = new Box<IWeakReturnable>();

        public MethodDefinitionPopulateScope(
            IPopulateScope<WeakMemberReferance> parameterDefinition,
            IPopulateScope<IWeakCodeElement>[] elements, 
            NameKey outputTypeName,
            WeakMethodDefinition.Make make
            )
        {
            this.parameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.outputTypeName = outputTypeName ?? throw new ArgumentNullException(nameof(outputTypeName));
            this.make = make ?? throw new ArgumentNullException(nameof(make));

        }

        public IBox<IWeakReturnable> GetReturnType(IElementBuilders elementBuilders)
        {
            return box;
        }

        public IPopulateBoxes<WeakMethodDefinition> Run(IPopulateScopeContext context)
        {

            var nextContext = context.Child();
            return new MethodDefinitionResolveReferance(
                parameterDefinition.Run(nextContext),
                nextContext.GetResolvableScope(), 
                elements.Select(x => x.Run(nextContext)).ToArray(), 
                outputTypeName, 
                make, 
                box);
        }
    }

    public class MethodDefinitionResolveReferance : IPopulateBoxes<WeakMethodDefinition>
    {
        private readonly IPopulateBoxes<WeakMemberReferance> parameter;
        private readonly IResolvableScope methodScope;
        private readonly IPopulateBoxes<IWeakCodeElement>[] lines;
        private readonly NameKey outputTypeName;
        private readonly WeakMethodDefinition.Make make;
        private readonly Box<IWeakReturnable> box;

        public MethodDefinitionResolveReferance(
            IPopulateBoxes<WeakMemberReferance> parameter, 
            IResolvableScope methodScope, 
            IPopulateBoxes<IWeakCodeElement>[] resolveReferance2, 
            NameKey outputTypeName,
            WeakMethodDefinition.Make make, 
            Box<IWeakReturnable> box)
        {
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.methodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
            lines = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
            this.outputTypeName = outputTypeName ?? throw new ArgumentNullException(nameof(outputTypeName));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public WeakMethodDefinition Run(IResolveReferanceContext context)
        {
            return box.Fill(
                make(
                    methodScope.GetTypeOrThrow(outputTypeName),
                    parameter.Run(context).MemberDefinition, 
                    lines.Select(x => x.Run(context)).ToArray(),
                    methodScope.GetFinalized(),
                    new IWeakCodeElement[0]));
        }
    }
}