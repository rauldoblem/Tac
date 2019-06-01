﻿using Prototypist.LeftToRight;
using System;
using System.Collections.Generic;
using System.Linq;
using Tac.Frontend;
using Tac.Frontend._2_Parser;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Model.Instantiated;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;

namespace Tac.Semantic_Model
{
    internal class OverlayMemberDefinition: IWeakMemberDefinition
    {
        private readonly IWeakMemberDefinition backing;
        private readonly Overlay overlay;

        public OverlayMemberDefinition(IWeakMemberDefinition backing, Overlay overlay)
        {
            this.backing = backing ?? throw new ArgumentNullException(nameof(backing));
            this.overlay = overlay ?? throw new ArgumentNullException(nameof(overlay));
            this.Type = backing.Type.IfIs(x => Possibly.Is(new OverlayTypeReference(x,overlay)));
        }

        public IIsPossibly<IWeakTypeReference> Type { get; }
        public bool ReadOnly => backing.ReadOnly;
        public IKey Key=> backing.Key;

        public IMemberDefinition Convert(TransformerExtensions.ConversionContext context)
        {
            return MemberDefinitionShared.Convert(Type,context, ReadOnly,Key);
        }

        public IBuildIntention<IMemberDefinition> GetBuildIntention(TransformerExtensions.ConversionContext context) {
            return MemberDefinitionShared.GetBuildIntention(Type, context, ReadOnly, Key);
        }
        
    }

    // very tac-ian 
    internal static class MemberDefinitionShared {

        public static IMemberDefinition Convert(IIsPossibly<IWeakTypeReference> Type,TransformerExtensions.ConversionContext context, bool ReadOnly, IKey Key)
        {
            var (def, builder) = MemberDefinition.Create();

            var buildIntention = Type.GetOrThrow().Cast<IConvertable<ITypeReferance>>().GetBuildIntention(context);
            buildIntention.Build();
            builder.Build(Key, buildIntention.Tobuild, ReadOnly);
            return def;
        }
        public static IBuildIntention<IMemberDefinition> GetBuildIntention(IIsPossibly<IWeakTypeReference> Type, TransformerExtensions.ConversionContext context, bool ReadOnly, IKey Key)
        {
            var (toBuild, maker) = MemberDefinition.Create();
            return new BuildIntention<IMemberDefinition>(toBuild, () =>
            {
                maker.Build(
                    Key,
                    TransformerExtensions.Convert<ITypeReferance>(Type.GetOrThrow(), context),
                    ReadOnly);
            });
        }

    }

    internal interface IWeakMemberDefinition:  IConvertable<IMemberDefinition>, IFrontendType
    {
        IIsPossibly<IWeakTypeReference> Type { get; }
        bool ReadOnly { get; }
        IKey Key { get; }
        IMemberDefinition Convert(TransformerExtensions.ConversionContext context);
    }

    // it is possible members are single instances with look up
    // up I don't think so
    // it is easier just to have simple value objects
    // it is certaianly true at somepoint we will need a flattened list 
    internal class WeakMemberDefinition:  IWeakMemberDefinition
    {
        public WeakMemberDefinition(bool readOnly, IKey key, IIsPossibly<IWeakTypeReference> type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            ReadOnly = readOnly;
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public IIsPossibly<IWeakTypeReference> Type { get; }
        public bool ReadOnly { get; }
        public IKey Key { get; }

        public IMemberDefinition Convert(TransformerExtensions.ConversionContext context)
        {
            return MemberDefinitionShared.Convert(Type, context, ReadOnly, Key);
        }

        public IBuildIntention<IMemberDefinition> GetBuildIntention(TransformerExtensions.ConversionContext context)
        {
            return MemberDefinitionShared.GetBuildIntention(Type, context, ReadOnly, Key);
        }

    }

    internal class MemberDefinitionMaker : IMaker<IPopulateScope<WeakMemberReference>>
    {
        public MemberDefinitionMaker()
        {
        }
        
        public ITokenMatching<IPopulateScope<WeakMemberReference>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .OptionalHas(new KeyWordMaker("readonly"), out var readonlyToken)
                .HasOne(w => w.Has(new TypeReferanceMaker(), out var _),
                        w => w.Has(new TypeDefinitionMaker(), out var _),
                        out var type)
                .Has(new NameMaker(), out var nameToken);
            if (matching is IMatchedTokenMatching matched)
            {
                return TokenMatching<IPopulateScope<WeakMemberReference>>.MakeMatch(
                    matched.Tokens,
                    matched.Context,
                    new MemberDefinitionPopulateScope(nameToken.Item, readonlyToken != default, type));
            }
            return TokenMatching<IPopulateScope<WeakMemberReference>>.MakeNotMatch(
                               matching.Context);
        }


        public static IPopulateScope<WeakMemberReference> PopulateScope(
            string item, 
            bool v, 
            IPopulateScope<IWeakTypeReference> typeToken)
        {
            return new MemberDefinitionPopulateScope(item, v,  typeToken);
        }
        public static IPopulateBoxes<WeakMemberReference> PopulateBoxes(
                string memberName,
                Box<IIsPossibly<WeakMemberReference>> box,
                bool isReadonly,
                IPopulateBoxes<WeakTypeReference> type,
                IResolvableScope scope,
                Box<IIsPossibly<WeakMemberDefinition>> memberDefinitionBox)
        {
            return new MemberDefinitionResolveReferance(
                memberName,
                box,
                isReadonly,
                type,
                scope,
                memberDefinitionBox);
        }


        private class MemberDefinitionPopulateScope : IPopulateScope<WeakMemberReference>
        {
            private readonly string memberName;
            private readonly bool isReadonly;
            private readonly IPopulateScope<IWeakTypeReference> typeName;
            private readonly Box<IIsPossibly<WeakMemberReference>> box = new Box<IIsPossibly<WeakMemberReference>>();
            private readonly Box<IIsPossibly<WeakMemberDefinition>> memberDefinitionBox = new Box<IIsPossibly<WeakMemberDefinition>>();

            public MemberDefinitionPopulateScope(string item, bool v, IPopulateScope<IWeakTypeReference> typeToken)
            {
                memberName = item ?? throw new ArgumentNullException(nameof(item));
                isReadonly = v;
                typeName = typeToken ?? throw new ArgumentNullException(nameof(typeToken));
            }

            public IPopulateBoxes<WeakMemberReference> Run(IPopulateScopeContext context)
            {
                var key = new NameKey(memberName);
                if (!context.Scope.TryAddMember(DefintionLifetime.Instance, key, memberDefinitionBox))
                {
                    throw new Exception("bad bad bad!");
                }
                return new MemberDefinitionResolveReferance(memberName, box, isReadonly, typeName.Run(context), context.GetResolvableScope(), memberDefinitionBox);
            }

            public IBox<IIsPossibly<IFrontendType>> GetReturnType()
            {
                return box;
            }
        }

        private class MemberDefinitionResolveReferance : IPopulateBoxes<WeakMemberReference>
        {
            private readonly string memberName;
            private readonly Box<IIsPossibly<WeakMemberReference>> box;
            private readonly bool isReadonly;
            public readonly IPopulateBoxes<IWeakTypeReference> type;
            private readonly IResolvableScope scope;
            private readonly Box<IIsPossibly<WeakMemberDefinition>> memberDefinitionBox;

            public MemberDefinitionResolveReferance(
                string memberName,
                Box<IIsPossibly<WeakMemberReference>> box,
                bool isReadonly,
                IPopulateBoxes<IWeakTypeReference> type,
                IResolvableScope scope,
                Box<IIsPossibly<WeakMemberDefinition>> memberDefinitionBox)
            {
                this.memberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
                this.box = box ?? throw new ArgumentNullException(nameof(box));
                this.isReadonly = isReadonly;
                this.type = type ?? throw new ArgumentNullException(nameof(type));
                this.scope = scope ?? throw new ArgumentNullException(nameof(scope));
                this.memberDefinitionBox = memberDefinitionBox ?? throw new ArgumentNullException(nameof(memberDefinitionBox));
            }

            public IIsPossibly<WeakMemberReference> Run(IResolveReferenceContext context)
            {
                memberDefinitionBox.Fill(
                    Possibly.Is(
                    new WeakMemberDefinition(
                        isReadonly,
                        new NameKey(memberName),
                        type.Run(context))));

                return box.Fill(Possibly.Is(new WeakMemberReference(Possibly.Is(memberDefinitionBox))));
            }
        }
    }

}