﻿using Prototypist.LeftToRight;
using System;
using System.Collections.Generic;
using System.Linq;
using Tac.Frontend._2_Parser;
using Tac.Model;
using Tac.Model.Elements;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{

    internal class WeakGenericTypeDefinition : ICodeElement, IVarifiableType, IGenericTypeDefinition
    {
        public WeakGenericTypeDefinition(NameKey key, IFinalizedScope scope, GenericTypeParameterDefinition[] typeParameterDefinitions)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            TypeParameterDefinitions = typeParameterDefinitions ?? throw new ArgumentNullException(nameof(typeParameterDefinitions));
        }

        public IKey Key { get; }

        public IFinalizedScope Scope { get; }

        public IGenericTypeParameterDefinition[] TypeParameterDefinitions { get; }

        #region IGenericTypeDefinition

        IFinalizedScope IGenericTypeDefinition.Scope => Scope;

        #endregion

        // huh? this seems to have no uses
        // and that means GenericScope has no uses
        // I have not build that part out yet so it is ok.
        //public bool TryCreateConcrete(IEnumerable<GenericTypeParameter> genericTypeParameters, out IReturnable result)
        //{
        //    if (genericTypeParameters.Select(x => x.Definition).SetEqual(TypeParameterDefinitions).Not())
        //    {
        //        result = default;
        //        return false;
        //    }

        //    result = new TypeDefinition(new GenericScope(Scope, genericTypeParameters),Key);
        //    return true;
        //}


        public T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.GenericTypeDefinition(this);
        }
        
        public IVarifiableType Returns()
        {
            return this;
        }
    }


    internal class GenericTypeParameterDefinition: IGenericTypeParameterDefinition
    {
        public GenericTypeParameterDefinition(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public IKey Key
        {
            get
            {
                return new NameKey(Name);
            }
        }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            return obj is GenericTypeParameterDefinition definition &&
                   Name == definition.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        internal bool Accepts(IVarifiableType b)
        {
            // TODO generic constraints
            return true;
        }
    }

    internal class GenericTypeParameter
    {
        public GenericTypeParameter(IBox<IVarifiableType> typeDefinition, GenericTypeParameterDefinition definition)
        {
            TypeDefinition = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public IBox<IVarifiableType> TypeDefinition { get; }
        public GenericTypeParameterDefinition Definition { get; }
    }

    internal class GenericTypeDefinitionMaker : IMaker<IPopulateScope<WeakGenericTypeDefinition>>
    {

        public GenericTypeDefinitionMaker()
        {
        }

        public ITokenMatching<IPopulateScope<WeakGenericTypeDefinition>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .Has(new KeyWordMaker("type"), out var _)
                .Has(new DefineGenericNMaker(), out AtomicToken[] genericTypes)
                .Has(new NameMaker(), out AtomicToken typeName)
                .Has(new BodyMaker(), out CurleyBracketToken body);
            if (matching is IMatchedTokenMatching matched)
            {
                return TokenMatching<IPopulateScope<WeakGenericTypeDefinition>>.MakeMatch(
                    matched.Tokens,
                    matched.Context,
                    new GenericTypeDefinitionPopulateScope(
                        new NameKey(typeName.Item),
                        tokenMatching.Context.ParseBlock(body),
                        genericTypes.Select(x => new GenericTypeParameterDefinition(x.Item)).ToArray()));
            }

            return TokenMatching<IPopulateScope<WeakGenericTypeDefinition>>.MakeNotMatch(
                    matching.Context);
        }



    }

    internal class GenericTypeDefinitionPopulateScope : IPopulateScope<WeakGenericTypeDefinition>
    {
        private readonly NameKey nameKey;
        private readonly IEnumerable<IPopulateScope<ICodeElement>> lines;
        private readonly GenericTypeParameterDefinition[] genericParameters;
        private readonly Box<IVarifiableType> box = new Box<IVarifiableType>();

        public GenericTypeDefinitionPopulateScope(
            NameKey nameKey, 
            IEnumerable<IPopulateScope<ICodeElement>> lines,
            GenericTypeParameterDefinition[] genericParameters)
        {
            this.nameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            this.lines = lines ?? throw new ArgumentNullException(nameof(lines));
            this.genericParameters = genericParameters ?? throw new ArgumentNullException(nameof(genericParameters));
        }

        public IPopulateBoxes<WeakGenericTypeDefinition> Run(IPopulateScopeContext context)
        {
            var encolsing = context.Scope.TryAddType(nameKey, box);
            
            var nextContext = context.Child();
            lines.Select(x => x.Run(nextContext)).ToArray();
            return new GenericTypeDefinitionResolveReferance(nameKey, genericParameters, nextContext.GetResolvableScope(), box);
        }

        public IBox<IVarifiableType> GetReturnType()
        {
            return box;
        }

    }

    internal class GenericTypeDefinitionResolveReferance : IPopulateBoxes<WeakGenericTypeDefinition>
    {
        private readonly NameKey nameKey;
        private readonly GenericTypeParameterDefinition[] genericParameters;
        private readonly IResolvableScope scope;
        private readonly Box<IVarifiableType> box;

        public GenericTypeDefinitionResolveReferance(
            NameKey nameKey, 
            GenericTypeParameterDefinition[] genericParameters, 
            IResolvableScope scope, 
            Box<IVarifiableType> box)
        {
            this.nameKey = nameKey ?? throw new ArgumentNullException(nameof(nameKey));
            this.genericParameters = genericParameters ?? throw new ArgumentNullException(nameof(genericParameters));
            this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }
        
        public WeakGenericTypeDefinition Run(IResolveReferanceContext context)
        {
            return box.Fill(new WeakGenericTypeDefinition(nameKey, scope.GetFinalized(), genericParameters));
        }
    }
}
