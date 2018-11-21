﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.Frontend._2_Parser;
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
            WeakTypeReferance outputType, 
            IBox<WeakMemberDefinition> parameterDefinition,
            ICodeElement[] body,
            IFinalizedScope scope,
            IEnumerable<ICodeElement> staticInitializers) : base(scope ?? throw new ArgumentNullException(nameof(scope)), body, staticInitializers)
        {
            OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
            ParameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
        }
        
        public WeakTypeReferance InputType => ParameterDefinition.GetValue().Type;
        public WeakTypeReferance OutputType { get; }
        public IBox<WeakMemberDefinition> ParameterDefinition { get; }

        #region IMethodDefinition

        ITypeReferance IMethodDefinition.InputType => InputType;
        ITypeReferance IMethodDefinition.OutputType => OutputType;
        IMemberDefinition IMethodDefinition.ParameterDefinition => ParameterDefinition.GetValue();

        #endregion



        public override T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.MethodDefinition(this);
        }
    }


    internal class MethodDefinitionMaker : IMaker<IPopulateScope<WeakMethodDefinition>>
    {
        public MethodDefinitionMaker()
        {
        }
        

        public ITokenMatching<IPopulateScope<WeakMethodDefinition>> TryMake(ITokenMatching tokenMatching)
        {
            IPopulateScope<WeakTypeReferance> input = null, output = null;
            var matching = tokenMatching
                .Has(new KeyWordMaker("method"), out var _)
                .HasSquare(x => x
                    .HasLine(y => y
                        .HasElement(z=>z
                            .Has(new MatchOneMaker<IPopulateScope<WeakTypeReferance>>(new TypeReferanceMaker(), new TypeDefinitionMaker()), out input)
                            .Has(new DoneMaker()))
                         .Has(new DoneMaker()))
                    .HasLine(y => y
                        .HasElement(z => z
                            .Has(new MatchOneMaker<IPopulateScope<WeakTypeReferance>>(new TypeReferanceMaker(), new TypeDefinitionMaker()), out output)
                            .Has(new DoneMaker()))
                        .Has(new DoneMaker()))
                    .Has(new DoneMaker()))
                .OptionalHas(new NameMaker(), out var parameterName)
                .Has(new BodyMaker(), out var body);
            if (matching
                .IsMatch)
            {
                var elements = matching.Context.ParseBlock(body);
                
                var parameterDefinition = new MemberDefinitionPopulateScope(
                        parameterName?.Item ?? "input",
                        false,
                        input
                        );
                
                return TokenMatching<IPopulateScope<WeakMethodDefinition>>.Match(
                    matching.Tokens,
                    matching.Context, 
                    new MethodDefinitionPopulateScope(
                        parameterDefinition,
                        elements, 
                        output));
            }

            return TokenMatching<IPopulateScope<WeakMethodDefinition>>.NotMatch(
                    matching.Tokens,
                    matching.Context);
        }
    }

    internal class MethodDefinitionPopulateScope : IPopulateScope<WeakMethodDefinition>
    {
        private readonly IPopulateScope<WeakMemberReferance> parameterDefinition;
        private readonly IPopulateScope<ICodeElement>[] elements;
        private readonly IPopulateScope<WeakTypeReferance> output;
        private readonly Box<IVarifiableType> box = new Box<IVarifiableType>();

        public MethodDefinitionPopulateScope(
            IPopulateScope<WeakMemberReferance> parameterDefinition,
            IPopulateScope<ICodeElement>[] elements,
            IPopulateScope<WeakTypeReferance> output
            )
        {
            this.parameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            this.output = output ?? throw new ArgumentNullException(nameof(output));

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
                output.Run(context), 
                box);
        }
    }

    internal class MethodDefinitionResolveReferance : IPopulateBoxes<WeakMethodDefinition>
    {
        private readonly IPopulateBoxes<WeakMemberReferance> parameter;
        private readonly IResolvableScope methodScope;
        private readonly IPopulateBoxes<ICodeElement>[] lines;
        private readonly IPopulateBoxes<WeakTypeReferance> output;
        private readonly Box<IVarifiableType> box;

        public MethodDefinitionResolveReferance(
            IPopulateBoxes<WeakMemberReferance> parameter, 
            IResolvableScope methodScope, 
            IPopulateBoxes<ICodeElement>[] resolveReferance2,
            IPopulateBoxes<WeakTypeReferance> output,
            Box<IVarifiableType> box)
        {
            this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            this.methodScope = methodScope ?? throw new ArgumentNullException(nameof(methodScope));
            lines = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public WeakMethodDefinition Run(IResolveReferanceContext context)
        {
            return box.Fill(
                new WeakMethodDefinition(
                    output.Run(context),
                    parameter.Run(context).MemberDefinition, 
                    lines.Select(x => x.Run(context)).ToArray(),
                    methodScope.GetFinalized(),
                    new ICodeElement[0]));
        }
    }
    
}