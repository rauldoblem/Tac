﻿using Prototypist.LeftToRight;
using System;
using System.Linq;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;
using Tac.Semantic_Model.Operations;

namespace Tac.Semantic_Model
{
    public class ImplicitMemberMaker : IMaker<Member>
    {
        public ImplicitMemberMaker(Func<int, IBox<MemberDefinition>, Member> make)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        private Func<int, IBox<MemberDefinition>, Member> Make { get; }

        public IResult<IPopulateScope<Member>> TryMake(ElementToken elementToken, ElementMatchingContext matchingContext)
        {
            if (TokenMatching.Start(elementToken.Tokens)
                .Has(ElementMatcher.KeyWord("var"), out var _)
                .Has(ElementMatcher.IsName, out AtomicToken first)
                .Has(ElementMatcher.IsDone)
                .IsMatch)
            {

                return ResultExtension.Good(new MemberPopulateScope(first.Item, Make));
            }

            return ResultExtension.Bad<IPopulateScope<Member>>();
        }

    }


    public class ImplicitMemberPopulateScope : IPopulateScope<Member>
    {
        private readonly string memberName;
        private readonly Func<int, MemberDefinition, Member> make;
        private readonly Box<IReturnable> box = new Box<IReturnable>();

        public ImplicitMemberPopulateScope(string item, Func<int, MemberDefinition, Member> make)
        {
            memberName = item ?? throw new ArgumentNullException(nameof(item));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IResolveReference<Member> Run(IPopulateScopeContext context)
        {

            var typeDef = new FollowBox<IReturnable>();
            var innerType = new MemberDefinition(false, new NameKey(memberName), typeDef);
            IBox<MemberDefinition> memberDef = new Box<MemberDefinition>(innerType);

            if (!context.TryAddMember(new NameKey(memberName), memberDef))
            {
                throw new Exception("bad bad bad!");
            }


            return new ImplicitMemberResolveReferance(innerType, make, typeDef, box);
        }


        public IBox<IReturnable> GetReturnType(IElementBuilders elementBuilders)
        {
            return box;
        }


    }

    public class ImplicitMemberResolveReferance : IResolveReference<Member>
    {
        private readonly MemberDefinition memberDef;
        private readonly Func<int, MemberDefinition, Member> make;
        private readonly FollowBox<IReturnable> typeDef;
        private readonly Box<IReturnable> box;

        public ImplicitMemberResolveReferance(
            MemberDefinition innerType, 
            Func<int, MemberDefinition, Member> make, 
            FollowBox<IReturnable> typeDef,
            Box<IReturnable> box)
        {
            memberDef = innerType ?? throw new ArgumentNullException(nameof(innerType));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
            this.typeDef = typeDef ?? throw new ArgumentNullException(nameof(typeDef));
            this.box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public IBox<IReturnable> GetReturnType(IResolveReferanceContext context)
        {
            if (!context.TryGetParent<BinaryResolveReferance<AssignOperation>>(out var op))
            {
                throw new Exception("the parent must be assign");
            }

            return new DelegateBox<IReturnable>(() => {
                var type = op.left.GetReturnType(context).GetValue();

                if (type.Key == RootKeys.MemberType)
                {
                    return type.Scope.GetTypeOrThrow(RootKeys.MemberTypeParameter.Key).GetValue();
                }

                return type;

            });
        }

        public Member Run(IResolveReferanceContext context)
        {
            if (!context.TryGetParent<BinaryResolveReferance<AssignOperation>>(out var op))
            {
                throw new Exception("the parent must be assign");
            }
            typeDef.Follow(new DelegateBox<IReturnable>(() => {
                var type = op.left.GetReturnType(context).GetValue();

                if (type.Key != RootKeys.MemberType) {
                    return type;
                }

                return type.Scope.GetTypeOrThrow(RootKeys.MemberTypeParameter.Key).GetValue();

            })); 

            return box.Fill(make(0, memberDef));
        }
    }
}