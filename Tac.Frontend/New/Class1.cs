﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac.Frontend;
using Tac.Model.Elements;
using Tac.Parser;
using Tac.Semantic_Model;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Operations;
using static Tac._3_Syntax_Model.Elements.Atomic_Types.PrimitiveTypes;

namespace Tac.New
{
    public static class ResultExtension{
        public static bool TryGetValue<T>(this IResult<T> self, out T res) {
            if (self.HasValue) {
                res = self.Value;
                return true;

            }
            res = default;
            return false;
        }
        
        public static Result<T> Good<T>(T value)
        {
            return new Result<T>(true, value);
        }

        public static Result<T> Bad<T>()
        {
            return new Result<T>(false, default);
        }
    }
    
    public interface IResult<out T>
    {
        bool HasValue { get; }
        T Value { get; }
    }
    
    public class Result<T> : IResult<T>
    {
        public Result(bool hasResult, T value)
        {
            HasValue = hasResult;
            Value = value;
        }

        public bool HasValue { get;}
        public T Value {get;}

    }

    internal interface IMaker
    {
        ITokenMatching TryMake(ITokenMatching elementToken);
    }

    internal interface IMaker<out TCodeElement>
    {
        ITokenMatching<TCodeElement> TryMake(IMatchedTokenMatching elementToken);
    }

    // hmm the parsing is almost a step as well? 


    internal interface IPopulateScopeContext
    {

        IPopulatableScope Scope { get; }
        IPopulateScopeContext Child();
        IPopulateScopeContext TemplateChild(IGenericTypeParameterPlacholder[] parameters);

    }

    internal class PopulateScopeContext : IPopulateScopeContext
    {
        private readonly ResolvableScope stack;

        public PopulateScopeContext(ResolvableScope stack)
        {
            this.stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        public IPopulatableScope Scope
        {
            get { return stack; }
        }
        

        public IPopulateScopeContext Child()
        {
            return new PopulateScopeContext(new ResolvableScope(stack));
        }

        public IResolvableScope GetResolvableScope()
        {
            return stack.ToResolvable();
        }

        public IPopulateScopeContext TemplateChild(IGenericTypeParameterPlacholder[] parameters)
        {
            var template = new ScopeTemplate(parameters,stack);
            return new PopulateScopeContext(template);
        }
    }

    public interface IResolveReferenceContext  {
    }

    public class ResolveReferanceContext : IResolveReferenceContext
    {
    }

    // TODO I think I should protect these!
    // you are only allowed to put things in scope during this step


    internal interface IPopulateScope<out TCodeElement> 
    {
        IFinalizeScope<TCodeElement> Run(IPopulateScopeContext context);
    }

    internal interface IFinalizeScopeContext { }

    internal interface IFinalizeScope<out TCodeElement>
    {
        IPopulateBoxes<TCodeElement> Run(IFinalizeScopeContext context);
    }


    internal interface IPopulateBoxes<out TCodeElement> 
    {
        IIsPossibly<TCodeElement> Run(IResolveReferenceContext context);
    }
    
}
