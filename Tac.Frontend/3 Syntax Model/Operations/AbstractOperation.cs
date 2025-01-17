﻿using Prototypist.LeftToRight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.Frontend;
using Tac.Frontend._2_Parser;
using Tac.Frontend.New;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Model.Operations;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model.Operations;

namespace Tac.Semantic_Model.CodeStuff
{

    public static class StaticSymbolsRegistry {
        private static readonly Prototypist.TaskChain.JumpBallConcurrent<List<string>> listLocker = new Prototypist.TaskChain.JumpBallConcurrent<List<string>>(new List<string>());
        public static string AddOrThrow(string s)
        {
            listLocker.Run(x =>
            {
                if (x.Contains(s))
                {
                    throw new Exception("Already added");
                }
                x.Add(s);
                return x;
            });
            return s;
        }
        // playing some fucking games here
        // I want each symbol to register itself in stead of having a centeral list
        // unfo, ensuring static init is hard in C#

        // my solution:
        // 1. you get the list of symbols from StaticSymbolsRegistry.SymbolsRegistry
        // 2. SymbolsRegistry is partail each symbol creates a member
        //    when that member is initialized the symbol is added to StaticSymbolsRegistry
        // 
        // when you use StaticSymbolsRegistry.SymbolsRegistry
        // we create a SymbolsRegistry so all the symbols are added to listLocker
        // then we set SymbolsRegistry.Symbols to listLocker
        public static SymbolsRegistry SymbolsRegistry = CreateSymbolsRegistry();

        private static SymbolsRegistry CreateSymbolsRegistry() {
            var res = new SymbolsRegistry
            {
                Symbols = listLocker.Run(x => x)
            };
            return res;
        }
    }

    public partial class SymbolsRegistry
    {
        public IEnumerable<string> Symbols { get; set; }

    }

    internal class BinaryOperation
    {
        public delegate IIsPossibly<T> Make<out T>(IIsPossibly<IFrontendCodeElement> left, IIsPossibly<IFrontendCodeElement> right);
        public delegate IIsPossibly<T> MakeBinaryType<out T>(IIsPossibly<IFrontendType> left, IIsPossibly<IFrontendType> right);
    }

    internal abstract class BinaryOperation<TLeft, TRight,TCodeElement> :  IConvertableFrontendCodeElement<TCodeElement>
        where TLeft : class, IFrontendCodeElement
        where TRight : class, IFrontendCodeElement
        where TCodeElement: class, ICodeElement
    {
        public IIsPossibly<TLeft> Left { get; }
        public IIsPossibly<TRight> Right { get; }
        public IIsPossibly<IFrontendCodeElement>[] Operands
        {
            get
            {
                return new IIsPossibly<IFrontendCodeElement>[] { Left, Right };
            }
        }
        
        public BinaryOperation(IIsPossibly<TLeft> left, IIsPossibly<TRight> right)
        {
            this.Left = left ?? throw new ArgumentNullException(nameof(left));
            this.Right = right ?? throw new ArgumentNullException(nameof(right));
        }
        
        public abstract IIsPossibly<IFrontendType> Returns();

        public abstract IBuildIntention<TCodeElement> GetBuildIntention(IConversionContext context);
    }



    internal abstract class BinaryTypeOperation<TLeft, TRight, TType> : IConvertableFrontendType<TType>
        where TLeft : IFrontendType
        where TRight : IFrontendType
        where TType: IVerifiableType 
    { 
        public IIsPossibly<TLeft> Left { get; }
        public IIsPossibly<TRight> Right { get; }
        public IEnumerable<IIsPossibly<IConvertableFrontendType<IVerifiableType>>> Operands { get {
                // this make me sad,
                // if we could mark TLeft, TRight as classes and I would need these ugly casts
                // but a lot of types are structs
                // so I have casts
                // I assume this has something to do with boxing
                // the cast boxes
                yield return (IIsPossibly<IConvertableFrontendType<IVerifiableType>>)Left;
                yield return (IIsPossibly<IConvertableFrontendType<IVerifiableType >>)Right;
            }
        }

        public BinaryTypeOperation(IIsPossibly<TLeft> left, IIsPossibly<TRight> right)
        {
            this.Left = left ?? throw new ArgumentNullException(nameof(left));
            this.Right = right ?? throw new ArgumentNullException(nameof(right));
        }


        public abstract IBuildIntention<TType> GetBuildIntention(IConversionContext context);
    }



    internal class BinaryOperationMaker<TFrontendCodeElement, TCodeElement> : IMaker<IPopulateScope<TFrontendCodeElement, ISetUpValue>>
        where TFrontendCodeElement : class, IConvertableFrontendCodeElement<TCodeElement>
        where TCodeElement : class, ICodeElement
    {

        public BinaryOperationMaker(string symbol, BinaryOperation.Make<TFrontendCodeElement> make
            )
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public string Symbol { get; }
        private BinaryOperation.Make<TFrontendCodeElement> Make { get; }

        public ITokenMatching<IPopulateScope<TFrontendCodeElement, ISetUpValue>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .Has(new BinaryOperationMatcher(Symbol), out (IReadOnlyList<IToken> perface, AtomicToken token, IToken rhs) match);
            if (matching is IMatchedTokenMatching matched)
            {
                var left = matching.Context.ParseLine(match.perface);
                var right = matching.Context.ParseParenthesisOrElement(match.rhs);

                return TokenMatching<IPopulateScope<TFrontendCodeElement, ISetUpValue>>.MakeMatch(
                    matched.Tokens,
                    matched.Context,
                    new BinaryPopulateScope(left, right, Make));
            }

            return TokenMatching<IPopulateScope<TFrontendCodeElement, ISetUpValue>>.MakeNotMatch(
                    matching.Context);
        }


        public static IPopulateScope<TFrontendCodeElement, ISetUpValue> PopulateScope(IPopulateScope<IFrontendCodeElement> left,
                IPopulateScope<IFrontendCodeElement> right,
                BinaryOperation.Make<TFrontendCodeElement> make)
        {
            return new BinaryPopulateScope(left,
                 right,
                 make);
        }
        public static IPopulateBoxes<TFrontendCodeElement> PopulateBoxes(IPopulateBoxes<IConvertableFrontendCodeElement<ICodeElement>> resolveReferance1,
                IPopulateBoxes<IConvertableFrontendCodeElement<ICodeElement>> resolveReferance2,
                BinaryOperation.Make<TFrontendCodeElement> make)
        {
            return new BinaryResolveReferance(resolveReferance1,
                resolveReferance2,
                make);
        }



        private class BinaryPopulateScope : IPopulateScope<TFrontendCodeElement, ISetUpValue>
        {
            private readonly IPopulateScope<IFrontendCodeElement, ISetUpValue> left;
            private readonly IPopulateScope<IFrontendCodeElement, ISetUpValue> right;
            private readonly BinaryOperation.Make<TFrontendCodeElement> make;

            public BinaryPopulateScope(IPopulateScope<IFrontendCodeElement, ISetUpValue> left,
                IPopulateScope<IFrontendCodeElement, ISetUpValue> right,
                BinaryOperation.Make<TFrontendCodeElement> make)
            {
                this.left = left ?? throw new ArgumentNullException(nameof(left));
                this.right = right ?? throw new ArgumentNullException(nameof(right));
                this.make = make ?? throw new ArgumentNullException(nameof(make));
            }

            public IResolvelizeScope<TFrontendCodeElement, ISetUpValue> Run(IDefineMembers scope, IPopulateScopeContext context)
            {
                context.TypeProblem.CreateValue();
                return new BinaryFinalizeScope(
                    left.Run(scope,context),
                    right.Run(scope, context),
                    make);
            }
        }


        private class BinaryFinalizeScope : IResolvelizeScope<TFrontendCodeElement, ISetUpValue>
        {
            public readonly IResolvelizeScope<IFrontendCodeElement, ISetUpValue> left;
            public readonly IResolvelizeScope<IFrontendCodeElement, ISetUpValue> right;
            private readonly BinaryOperation.Make<TFrontendCodeElement> make;

            public BinaryFinalizeScope(
                ISetUpValue setUpSideNode,
                IResolvelizeScope<IFrontendCodeElement, ISetUpValue> resolveReferance1,
                IResolvelizeScope<IFrontendCodeElement, ISetUpValue> resolveReferance2,
                BinaryOperation.Make<TFrontendCodeElement> make)
            {
                SetUpSideNode = setUpSideNode ?? throw new ArgumentNullException(nameof(setUpSideNode));
                left = resolveReferance1 ?? throw new ArgumentNullException(nameof(resolveReferance1));
                right = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
                this.make = make ?? throw new ArgumentNullException(nameof(make));
            }

            public ISetUpValue SetUpSideNode
            {
                get;
            }

            public IPopulateBoxes<TFrontendCodeElement> Run(IResolvableScope parent, IFinalizeScopeContext context)
            {
                return new BinaryResolveReferance(left.Run(parent,context), right.Run(parent,context), make);
            }
        }

        private class BinaryResolveReferance : IPopulateBoxes<TFrontendCodeElement>
        {
            public readonly IPopulateBoxes<IFrontendCodeElement> left;
            public readonly IPopulateBoxes<IFrontendCodeElement> right;
            private readonly BinaryOperation.Make<TFrontendCodeElement> make;

            public BinaryResolveReferance(
                IPopulateBoxes<IFrontendCodeElement> resolveReferance1,
                IPopulateBoxes<IFrontendCodeElement> resolveReferance2,
                BinaryOperation.Make<TFrontendCodeElement> make)
            {
                left = resolveReferance1 ?? throw new ArgumentNullException(nameof(resolveReferance1));
                right = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
                this.make = make ?? throw new ArgumentNullException(nameof(make));
            }


            public IIsPossibly<TFrontendCodeElement> Run(IResolvableScope scope, IResolveReferenceContext context)
            {
                var res = make(
                    left.Run(scope,context),
                    right.Run(scope, context));
                return res;
            }
        }
    }


    internal class BinaryTypeMaker : IMaker<IPopulateScope<IWeakTypeReference>>
    {

        public BinaryTypeMaker(string symbol, BinaryOperation.MakeBinaryType<IWeakTypeReference> make
            )
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Make = make ?? throw new ArgumentNullException(nameof(make));
        }

        public string Symbol { get; }
        private BinaryOperation.MakeBinaryType<IWeakTypeReference> Make { get; }

        public ITokenMatching<IPopulateScope<IWeakTypeReference>> TryMake(IMatchedTokenMatching tokenMatching)
        {
            var matching = tokenMatching
                .Has(new BinaryOperationMatcher(Symbol), out (IReadOnlyList<IToken> perface, AtomicToken token, IToken rhs) match);
            if (matching is IMatchedTokenMatching matched)
            {
                var left = matching.Context.ParseTypeLine(match.perface);
                var right = matching.Context.ParseParenthesisOrElementType(match.rhs);

                return TokenMatching<IPopulateScope<IWeakTypeReference>>.MakeMatch(
                    matched.Tokens,
                    matched.Context, 
                    new BinaryPopulateScope(left, right, Make));
            }

            return TokenMatching<IPopulateScope<IWeakTypeReference>>.MakeNotMatch(
                    matching.Context);
        }


        public static IPopulateScope<IWeakTypeReference> PopulateScope(IPopulateScope<IConvertableFrontendType<IVerifiableType>> left,
                IPopulateScope<IConvertableFrontendType<IVerifiableType>> right,
                BinaryOperation.MakeBinaryType<IWeakTypeReference> make)
        {
            return new BinaryPopulateScope( left,
                 right,
                 make);
        }
        public static IPopulateBoxes<IWeakTypeReference> PopulateBoxes(IPopulateBoxes<IConvertableFrontendType<IVerifiableType>> resolveReferance1,
                IPopulateBoxes<IConvertableFrontendType<IVerifiableType>> resolveReferance2,
                BinaryOperation.MakeBinaryType<IWeakTypeReference> make)
        {
            return new BinaryResolveReferance(resolveReferance1,
                resolveReferance2,
                make);
        }



        private class BinaryPopulateScope : IPopulateScope<IWeakTypeReference>
        {
            private readonly IPopulateScope<IFrontendType> left;
            private readonly IPopulateScope<IFrontendType> right;
            private readonly BinaryOperation.MakeBinaryType<IWeakTypeReference> make;

            public BinaryPopulateScope(IPopulateScope<IFrontendType> left,
                IPopulateScope<IFrontendType> right,
                BinaryOperation.MakeBinaryType<IWeakTypeReference> make)
            {
                this.left = left ?? throw new ArgumentNullException(nameof(left));
                this.right = right ?? throw new ArgumentNullException(nameof(right));
                this.make = make ?? throw new ArgumentNullException(nameof(make));
            }


            public IResolvelizeScope<IWeakTypeReference> Run(IPopulatableScope scope, IPopulateScopeContext context)
            {
                // TODO
                // this is something I don't much like
                // right runs first because of assign
                // in assign you might have something like
                // method [int;int] input { input < ? 2 if { 1 return; } else { input - 1 > fac * input return; } } =: fac
                // if the left runs first than fac will not be found
                // and so it will add it to the scope
                // but if the right is run first 
                // fac works
                // if I add an assign that goes the other way...
                // this will break

                // part of me just thinks 
                // force 'var' on member definition 

                return new BinaryFinalizeScope(
                    left.Run(scope,context),
                    right.Run(scope,context),
                    make);
            }
        }


        private class BinaryFinalizeScope : IResolvelizeScope<IWeakTypeReference>
        {
            public readonly IResolvelizeScope<IFrontendType> left;
            public readonly IResolvelizeScope<IFrontendType> right;
            private readonly BinaryOperation.MakeBinaryType<IWeakTypeReference> make;

            public BinaryFinalizeScope(
                IResolvelizeScope<IFrontendType> resolveReferance1,
                IResolvelizeScope<IFrontendType> resolveReferance2,
                BinaryOperation.MakeBinaryType<IWeakTypeReference> make)
            {
                left = resolveReferance1 ?? throw new ArgumentNullException(nameof(resolveReferance1));
                right = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
                this.make = make ?? throw new ArgumentNullException(nameof(make));
            }


            public IPopulateBoxes<IWeakTypeReference> Run(IResolvableScope parent,IFinalizeScopeContext context)
            {
                return new BinaryResolveReferance(left.Run(parent,context), right.Run(parent,context), make);
            }
        }

        private class BinaryResolveReferance : IPopulateBoxes<IWeakTypeReference>
        {
            public readonly IPopulateBoxes<IFrontendType> left;
            public readonly IPopulateBoxes<IFrontendType> right;
            private readonly BinaryOperation.MakeBinaryType<IWeakTypeReference> make;

            public BinaryResolveReferance(
                IPopulateBoxes<IFrontendType> resolveReferance1,
                IPopulateBoxes<IFrontendType> resolveReferance2,
                BinaryOperation.MakeBinaryType<IWeakTypeReference> make)
            {
                left = resolveReferance1 ?? throw new ArgumentNullException(nameof(resolveReferance1));
                right = resolveReferance2 ?? throw new ArgumentNullException(nameof(resolveReferance2));
                this.make = make ?? throw new ArgumentNullException(nameof(make));
            }


            public IIsPossibly<IWeakTypeReference> Run(IResolvableScope scope, IResolveReferenceContext context)
            {
                var res = make(
                    left.Run(scope,context),
                    right.Run(scope, context));

                return res;
            }
        }
    }

}
