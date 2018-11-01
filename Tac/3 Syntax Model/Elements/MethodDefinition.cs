﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.Model;
using Tac.Model.Elements;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{

    internal class WeakMethodDefinition : WeakAbstractBlockDefinition, IMethodType, IMethodDefinition
    {
        public WeakMethodDefinition(
            IBox<IVarifiableType> outputType, 
            IBox<WeakMemberDefinition> parameterDefinition,
            ICodeElement[] body,
            IWeakFinalizedScope scope,
            IEnumerable<ICodeElement> staticInitializers) : base(scope ?? throw new ArgumentNullException(nameof(scope)), body, staticInitializers)
        {
            OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
        }
        
        public IBox<IVarifiableType> InputType => ParameterDefinition.GetValue().Type;
        public IBox<IVarifiableType> OutputType { get; }
        public IBox<WeakMemberDefinition> ParameterDefinition { get; }

        #region IMethodDefinition

        IVarifiableType IMethodDefinition.InputType => InputType.GetValue();
        IVarifiableType IMethodDefinition.OutputType => OutputType.GetValue();
        IMemberDefinition IMethodDefinition.ParameterDefinition => ParameterDefinition.GetValue();

        #endregion



        public override T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.MethodDefinition(this);
        }

        public T Convert<T>(ITypeConverter<T> context)
        {
            return context.MethodType(this);
        }
    }


    internal class MethodDefinitionMaker : IMaker<WeakMethodDefinition>
    {
        public MethodDefinitionMaker()
        {
        }
        

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
                         new NameKey(inputType.Item)
                        );
                
                var outputTypeName = new NameKey(outputType.Item);
                
                return ResultExtension.Good(new MethodDefinitionPopulateScope(
                    parameterDefinition,
                    elements, 
                    outputTypeName));
            }

            return ResultExtension.Bad<IPopulateScope<WeakMethodDefinition>>();
        }
    }

    internal class MethodDefinitionPopulateScope : IPopulateScope<WeakMethodDefinition>
    {
        private readonly IPopulateScope<WeakMemberReferance> parameterDefinition;
        private readonly IPopulateScope<ICodeElement>[] elements;
        private readonly NameKey outputTypeName;
        private readonly Box<IVarifiableType> box = new Box<IVarifiableType>();

        public MethodDefinitionPopulateScope(
            IPopulateScope<WeakMemberReferance> parameterDefinition,
            IPopulateScope<ICodeElement>[] elements, 
            NameKey outputTypeName
            )
        {
            this.parameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.outputTypeName = outputTypeName ?? throw new ArgumentNullException(nameof(outputTypeName));

        }

        public IBox<IVarifiableType> GetReturnType()
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
                box);
        }
    }

    internal class MethodDefinitionResolveReferance : IPopulateBoxes<WeakMethodDefinition>
    {
        private readonly IPopulateBoxes<WeakMemberReferance> parameter;
        private readonly IResolvableScope methodScope;
        private readonly IPopulateBoxes<ICodeElement>[] lines;
        private readonly NameKey outputTypeName;
        private readonly Box<IVarifiableType> box;

        public MethodDefinitionResolveReferance(
            IPopulateBoxes<WeakMemberReferance> parameter, 
            IResolvableScope methodScope, 
            IPopulateBoxes<ICodeElement>[] resolveReferance2, 
            NameKey outputTypeName,
            Box<IVarifiableType> box)
        {
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.methodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
            lines = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
            this.outputTypeName = outputTypeName ?? throw new ArgumentNullException(nameof(outputTypeName));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public WeakMethodDefinition Run(IResolveReferanceContext context)
        {
            return box.Fill(
                new WeakMethodDefinition(
                    methodScope.GetTypeOrThrow(outputTypeName),
                    parameter.Run(context).MemberDefinition, 
                    lines.Select(x => x.Run(context)).ToArray(),
                    methodScope.GetFinalized(),
                    new ICodeElement[0]));
        }
    }
    
}