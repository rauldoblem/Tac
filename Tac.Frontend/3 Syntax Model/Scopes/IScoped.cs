﻿using Prototypist.LeftToRight;
using Prototypist.TaskChain.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tac._3_Syntax_Model.Elements.Atomic_Types;
using Tac.Frontend;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Parser;

namespace Tac.Semantic_Model
{
    // I am not really sure this is a useful concept
    //
    internal interface IScoped
    {
        IResolvableScope Scope { get; }
    }

    // ok.. you are here
    // you are here
    // you are here
    // IFrontendGenericType can be overlayed
    // when you overlay it, it returns an 
    // OverlayWeakTypeDefinition or a OverlayGenericTypeDefinition
    // ...
    // but how does that work with methods?
    // they are IFrontendGenericType but they return ...?
    // 
    // ...
    //
    // maybe we just have a generic type called Generic<T>
    // it has a method called overlay
    // overlay returns T or Generic<T>
    // well... you have to overlay all the types
    // so before you call the method you should know if the result will be generic
    // maybe there are two method on generic
    // Overlay and GenericOverlay
    //
    // no no no, that does not work
    // how do you use it anywhere
    // inside a generic method
    // a generic type looks just like a type
    //
    // maybe things that can be generic, just have a flag
    // so no more GenericTypeDefiniiton
    // instead typeDefinition can be generic
    // typeDefinition has an overlay function that returns a typeDefinition
    //
    // I should start by making the generic type interfact
    // I think it returns an or<IGeneric,IFrontendType<IVarifiableType>>
    // 
    // ah! a IFrontendGenericType is a IFrontendType<IVarifiableType>
    // two methods

    internal interface IFrontendGenericType : IFrontendType<IGenericType>
    {
        IIsPossibly<Tac._3_Syntax_Model.Elements.Atomic_Types.GemericTypeParameterPlacholder>[] TypeParameterDefinitions { get; }
        OrType<IFrontendGenericType, IFrontendType<IVerifiableType>> Overlay(TypeParameter[] typeParameters);
    }

    internal class TypeParameter {
        public readonly Tac._3_Syntax_Model.Elements.Atomic_Types.GemericTypeParameterPlacholder parameterDefinition;
        public readonly IFrontendType<IVerifiableType> frontendType;

        public TypeParameter(Tac._3_Syntax_Model.Elements.Atomic_Types.GemericTypeParameterPlacholder parameterDefinition, IFrontendType<IVerifiableType> frontendType)
        {
            this.parameterDefinition = parameterDefinition ?? throw new ArgumentNullException(nameof(parameterDefinition));
            this.frontendType = frontendType ?? throw new ArgumentNullException(nameof(frontendType));
        }
    }

    internal class ScopeTemplate : NewScope //, IFinalizedScopeTemplate
    {
        public ScopeTemplate(Tac._3_Syntax_Model.Elements.Atomic_Types.GemericTypeParameterPlacholder[] typeParameterDefinitions, NewScope parent):base(parent)
        {
            TypeParameterDefinitions = typeParameterDefinitions ?? throw new ArgumentNullException(nameof(typeParameterDefinitions));
            foreach (var item in typeParameterDefinitions)
            {
                // for the sake of validation type parameters are types 

                if (!TryAddType(item.Key, new Box<IIsPossibly< IFrontendType<IVerifiableType>>>(Possibly.Is(new GemericTypeParameterPlacholder(item.Key))))) {
                    throw new Exception("that is not right!");
                }
            }
        }

        public GemericTypeParameterPlacholder[] TypeParameterDefinitions {get;}

    }

    internal class NewScope : IPopulatableScope, IResolvableScope
    {
        public NewScope Parent { get; }

        public IEnumerable<IKey> MemberKeys
        {
            get
            {
                return members.Keys;
            }
        }

        protected readonly ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<IIsPossibly<WeakMemberDefinition>>>>> members
            = new ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<IIsPossibly<WeakMemberDefinition>>>>>();

        protected readonly ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<IIsPossibly<IFrontendType<IVerifiableType>>>>>> types
            = new ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<IIsPossibly<IFrontendType<IVerifiableType>>>>>>();

        protected readonly ConcurrentDictionary<NameKey, ConcurrentSet<Visiblity<IBox<IIsPossibly<IFrontendGenericType>>>>> genericTypes
            = new ConcurrentDictionary<NameKey, ConcurrentSet<Visiblity<IBox<IIsPossibly<IFrontendGenericType>>>>>();

        public NewScope(NewScope parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public NewScope()
        {
            TryAddType(new NameKey("int"), new Box<IIsPossibly<IFrontendType<IVerifiableType>>>(Possibly.Is(new NumberType())));
            TryAddType(new NameKey("string"), new Box<IIsPossibly<IFrontendType<IVerifiableType>>>(Possibly.Is(new StringType())));
            TryAddType(new NameKey("any"), new Box<IIsPossibly<IFrontendType<IVerifiableType>>>(Possibly.Is(new AnyType())));
            TryAddType(new NameKey("empty"), new Box<IIsPossibly<IFrontendType<IVerifiableType>>>(Possibly.Is(new EmptyType())));
            TryAddType(new NameKey("bool"), new Box<IIsPossibly<IFrontendType<IVerifiableType>>>(Possibly.Is(new BooleanType())));
            // TODO, I need to figure out how method types work
            //
            TryAddGeneric(
                new NameKey("method"),
                new Box<IIsPossibly<IFrontendGenericType>>(Possibly.Is(new GenericMethodType())));
            TryAddGeneric(
                new NameKey("implementation"), new Box<IIsPossibly<IFrontendGenericType>>(Possibly.Is(new GenericImplementationType())));
        }

        public IResolvableScope ToResolvable()
        {
            return this;
        }

        public bool TryAddGeneric(NameKey key, IBox<IIsPossibly<IFrontendGenericType>> definition)
        {
            var list = genericTypes.GetOrAdd(new NameKey(key.Name), new ConcurrentSet<Visiblity<IBox<IIsPossibly<IFrontendGenericType>>>>());
            var visiblity = new Visiblity<IBox<IIsPossibly<IFrontendGenericType>>>(DefintionLifetime.Static, definition);
            return list.TryAdd(visiblity);
        }
        
        public bool TryAddMember(DefintionLifetime defintionLifetime, IKey key, IBox<IIsPossibly<WeakMemberDefinition>> definition)
        {
            var list = members.GetOrAdd(key, new ConcurrentSet<Visiblity<IBox<IIsPossibly<WeakMemberDefinition>>>>());
            var visiblity = new Visiblity<IBox<IIsPossibly<WeakMemberDefinition>>>(defintionLifetime, definition);
            return list.TryAdd(visiblity);
        }

        public bool TryAddType(IKey key, IBox<IIsPossibly<IFrontendType<IVerifiableType>>> definition)
        {
            var list = types.GetOrAdd(key, new ConcurrentSet<Visiblity<IBox<IIsPossibly<IFrontendType<IVerifiableType>>>>>());
            var visiblity = new Visiblity<IBox<IIsPossibly<IFrontendType<IVerifiableType>>>>(DefintionLifetime.Static, definition);
            return list.TryAdd(visiblity);
        }
        
        public bool TryGetMember(IKey name, bool staticOnly, out IBox<IIsPossibly<IWeakMemberDefinition>> member)
        {
            if (!members.TryGetValue(name, out var items))
            {
                goto seeParent;
            }

            var thing = items.SingleOrDefault();

            if (thing == default)
            {
                goto seeParent;
            }

            member = thing.Definition;
            return true;

            seeParent:
            if (Parent != null)
            {
                return Parent.TryGetMember(name, staticOnly, out member);
            }
            else
            {
                member = default;
                return false;
            }
        }

        public bool TryGetType(IKey name, out IBox<IIsPossibly<IFrontendType<IVerifiableType>>> type)
        {
            if (name is GenericNameKey generic)
            {
                if (!genericTypes.TryGetValue(new NameKey(generic.Name), out var set)){goto exit;}
                
                var typesBoxes = generic.Types.Select(x=>
                {
                    TryGetType(x, out var innerTypeBox);
                    if (innerTypeBox == default) {
                        throw new Exception("I guess that is exceptional");
                    }
                    return innerTypeBox;
                }).ToList();

                type = new DelegateBox<IIsPossibly<IFrontendType<IVerifiableType>>>(() =>
                {
                    var overlayed = set.Select(x => x.Definition.GetValue())
                        .Where(x => x.IsDefinately(out var _, out var _))
                        .Where(x => x.GetOrThrow().TypeParameterDefinitions.Length == typesBoxes.Count())
                        .Single()
                        .Assign(out var single).GetOrThrow()
                        .Overlay(single.GetOrThrow().TypeParameterDefinitions.Zip(typesBoxes, (x, y) => new TypeParameter(x.GetOrThrow(), y.GetValue().GetOrThrow())).ToArray());
                    if (overlayed.Is(out IFrontendType<IVerifiableType> frontendType)) {
                        return Possibly.Is(frontendType);
                    }
                    if (overlayed.Is(out IFrontendGenericType frontendGeneric))
                    {
                        return Possibly.Is(frontendGeneric);
                    }
                    throw new Exception("the or type should have been IFrontendType<IVarifiableType> or IFrontendGenericType");
                });
                
                return true;
            }
            
            if (!types.TryGetValue(name, out var items)) {goto exit;}

            var thing = items.SingleOrDefault();

            if (thing == default){ goto exit; }
            
            type = thing.Definition;
            return true;

            // goto 🤘
            exit:
            if (Parent != null)
            {
                return Parent.TryGetType(name, out type);
            }
            else
            {
                type = default;
                return false;
            }
        }
        
        public IBuildIntention<IFinalizedScope> GetBuildIntention(TransformerExtensions.ConversionContext context)
        {
            var (toBuild, maker) = Model.Instantiated.Scope.Create();
            return new BuildIntention<IFinalizedScope>(toBuild, () =>
            {
                maker.Build(
                    members.Select(x=>new Tac.Model.Instantiated.Scope.IsStatic(x.Value.Single().Definition.GetValue().GetOrThrow().Convert(context),false)).ToArray(),
                    types.SelectMany(x=> x.Value.Select(y=> new Tac.Model.Instantiated.Scope.TypeData(x.Key,y.Definition.GetValue().GetOrThrow().Convert(context)))).ToList(),
                    genericTypes.SelectMany(x => x.Value.Select(y => new Tac.Model.Instantiated.Scope.GenericTypeData(x.Key, y.Definition.GetValue().GetOrThrow().Convert(context)))).ToList()
                    );
            });
        }
    }

}
