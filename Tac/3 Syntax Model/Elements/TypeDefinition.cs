﻿using Prototypist.LeftToRight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.Frontend;
using Tac.Frontend._2_Parser;
using Tac.Model;
using Tac.Model.Elements;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{

    internal class WeakTypeDefinition : IFrontendCodeElement, IScoped, IFrontendType
    {
        public WeakTypeDefinition(IFinalizedScope scope, IIsPossibly<IKey> key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        public IIsPossibly<IKey> Key { get; }
        public IFinalizedScope Scope { get; }
        

        IIsPossibly<IFrontendType> IFrontendCodeElement.Returns()
        {
            return Possibly.Is(this);
        }
    }


    internal class TypeDefinitionMaker : IMaker<IPopulateScope<WeakTypeReferance>>
    {
        public TypeDefinitionMaker()
        {
        }
        
        public ITokenMatching<IPopulateScope<WeakTypeReferance>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .Has(new KeyWordMaker("type"), out var _)
                .OptionalHas(new NameMaker(), out var typeName)
                .Has(new BodyMaker(), out var body);

            if (matching is IMatchedTokenMatching matched)
            {
               var elements = tokenMatching.Context.ParseBlock(body);
                
               return TokenMatching<IPopulateScope<WeakTypeReferance>>.MakeMatch(
                    matched.Tokens,
                    matched.Context, 
                    new TypeDefinitionPopulateScope(
                       elements, 
                       typeName != default ? new NameKey(typeName.Item).Cast<IKey>(): new ImplicitKey()));
            }

            return TokenMatching<IPopulateScope<WeakTypeReferance>>.MakeNotMatch(
                    matching.Context);
        }
    }

    internal class TypeDefinitionPopulateScope : IPopulateScope<WeakTypeReferance>
    {
        private readonly IPopulateScope<IFrontendCodeElement>[] elements;
        private readonly IKey key;
        private readonly Box<IIsPossibly<WeakTypeDefinition>> definitionBox = new Box<IIsPossibly<WeakTypeDefinition>>();
        private readonly WeakTypeReferance typeReferance;
        private readonly Box<IIsPossibly<WeakTypeReferance>> box;

        public TypeDefinitionPopulateScope(IPopulateScope<IFrontendCodeElement>[] elements, IKey typeName)
        {
            this.elements = elements ?? throw new ArgumentNullException(nameof(elements));
            key = typeName ?? throw new ArgumentNullException(nameof(typeName));
            typeReferance = new WeakTypeReferance(Possibly.Is(definitionBox));
            box = new Box<IIsPossibly<WeakTypeReferance>>(Possibly.Is(typeReferance));
        }

        public IBox<IIsPossibly<IFrontendType>> GetReturnType()
        {
            return box;
        }

        public IPopulateBoxes<WeakTypeReferance> Run(IPopulateScopeContext context)
        {
            var encolsing = context.Scope.TryAddType(key, box);
            var nextContext = context.Child();
            elements.Select(x => x.Run(nextContext)).ToArray();
            return new TypeDefinitionResolveReference(
                nextContext.GetResolvableScope(),
                definitionBox,
                typeReferance,
                key);
        }
    }

    internal class TypeDefinitionResolveReference : IPopulateBoxes<WeakTypeReferance>
    {
        private readonly IResolvableScope scope;
        private readonly Box<IIsPossibly<WeakTypeDefinition>> definitionBox;
        private readonly WeakTypeReferance typeReferance;
        private readonly IKey key;

        public TypeDefinitionResolveReference(
            IResolvableScope scope, 
            Box<IIsPossibly<WeakTypeDefinition>> definitionBox, 
            WeakTypeReferance typeReferance, 
            IKey key)
        {
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.definitionBox = definitionBox ?? throw new ArgumentNullException(nameof(definitionBox));
            this.typeReferance = typeReferance ?? throw new ArgumentNullException(nameof(typeReferance));
            this.key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public IIsPossibly<WeakTypeReferance> Run(IResolveReferenceContext context)
        {
            definitionBox.Fill(Possibly.Is(new WeakTypeDefinition(scope.GetFinalized(), Possibly.Is(key))));
            return Possibly.Is(typeReferance);
        }
    }
}
