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

                return ResultExtension.Good(new MemberPopulateScope(matchingContext.ScopeStack.TopScope.Cast<LocalStaticScope>(), first.Item, Make));
            }

            return ResultExtension.Bad<IPopulateScope<Member>>();
        }

    }


    public class ImplicitMemberPopulateScope : IPopulateScope<Member>
    {
        private readonly LocalStaticScope topScope;
        private readonly string memberName;
        private readonly Func<int, MemberDefinition, Member> make;

        public ImplicitMemberPopulateScope(LocalStaticScope topScope, string item, Func<int, MemberDefinition, Member> make)
        {
            this.topScope = topScope ?? throw new ArgumentNullException(nameof(topScope));
            memberName = item ?? throw new ArgumentNullException(nameof(item));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public IResolveReference<Member> Run(IPopulateScopeContext context)
        {

            var typeDef = new FollowBox<ITypeDefinition>();
            var innerType = new MemberDefinition(false, new NameKey(memberName), typeDef);
            IBox<MemberDefinition> memberDef = new Box<MemberDefinition>(innerType);

            if (!topScope.TryAddLocal(new NameKey(memberName), memberDef))
            {
                throw new Exception("bad bad bad!");
            }


            return new ImplicitMemberResolveReferance(innerType, make, typeDef);
        }

    }

    public class ImplicitMemberResolveReferance : IResolveReference<Member>
    {
        private readonly MemberDefinition memberDef;
        private readonly Func<int, MemberDefinition, Member> make;
        private readonly FollowBox<ITypeDefinition> typeDef;

        public ImplicitMemberResolveReferance(MemberDefinition innerType, Func<int, MemberDefinition, Member> make, FollowBox<ITypeDefinition> typeDef)
        {
            memberDef = innerType ?? throw new ArgumentNullException(nameof(innerType));
            this.make = make ?? throw new ArgumentNullException(nameof(make));
            this.typeDef = typeDef;
        }

        public IBox<ITypeDefinition> GetReturnType(IResolveReferanceContext context)
        {
            return context.Tree.root.GetTypeOrThrow(RootScope.MemberType);
        }

        public Member Run(IResolveReferanceContext context)
        {
            if (context.TryGetParent<BinaryResolveReferance<AssignOperation>>(out var op))
            {
                typeDef.Follow(op.right.GetReturnType(context));
            }

            return make(0, memberDef);
        }
    }
}