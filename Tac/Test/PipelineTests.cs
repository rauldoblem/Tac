﻿using Newtonsoft.Json;
using Prototypist.LeftToRight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tac.New;
using Tac.Parser;
using Tac.Semantic_Model;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;
using Tac.Semantic_Model.Operations;
using Tac.TestCases;
using Tac.Tests.Help;
using Tac.Tests.Samples;
using Tac.Tests.Tokenizer;
using Xunit;
using static Tac.Semantic_Model.ScopeTree;

namespace Tac.Tests
{
    public class PipelineTests
    {
        [Fact]
        public void Token_CodeElements_Factorial()
        {
            Toke_CodeElements(new WrappedFactorial());
        }

        [Fact]
        public void Token_CodeElements_Arithmetic()
        {
            Toke_CodeElements(new WrappedArithmetic());
        }

        [Fact]
        public void Token_CodeElements_PointObject()
        {
            Toke_CodeElements(new WrappedPointObject());
        }

        private static void Toke_CodeElements(IWrappedTestCase sample) { 

            var elementMatchingContest = new ElementMatchingContext();

            var scopePopulators = elementMatchingContest.ParseFile(sample.Token as FileToken);

            var  stack = ScopeStack.Root();
            var populateScopeContex = new PopulateScopeContext(stack);
            var referanceResolvers = scopePopulators.Select(populateScope => populateScope.Run(populateScopeContex)).ToArray();

            var resolveReferanceContext = new ResolveReferanceContext();
            var result = referanceResolvers.Select(reranceResolver => reranceResolver.Run(resolveReferanceContext)).ToArray();
            
            var target = sample.CodeElements.ToArray();

            Assert.Equal(result.Length, target.Length);
            for (var i = 0; i < result.Length; i++)
            {
                result[i].ValueEqualOrThrow(target[i]);
            }
        }

        [Fact]
        public void Text_Token_Factorial()
        {
            Text_Token(new WrappedFactorial());
        }

        [Fact]
        public void Text_Token_Arithmetic()
        {
            Text_Token(new WrappedArithmetic());
        }

        [Fact]
        public void Text_Token_PointObject()
        {
            Text_Token(new WrappedPointObject());
        }

        private static void Text_Token(IWrappedTestCase sample)
        {
            var text = sample.Text;
            
            var tokenizer = new Parser.Tokenizer(Symbols.GetSymbols());
            var res = tokenizer.Tokenize(text);

            var target = sample.Token;

            var targetJson = JsonConvert.SerializeObject(target);
            var resJson = JsonConvert.SerializeObject(res);

            target.ValueEqualOrThrow(res);
        }
    }
}
