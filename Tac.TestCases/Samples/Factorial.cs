﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Model.Operations;
using Tac.Semantic_Model;
using Tac.Semantic_Model.Names;
using Tac.TestCases;
using Tac.TestCases.Help;

namespace Tac.Tests.Samples
{
    public class Factorial : ITestCase
    {
        public string Text
        {
            get
            {
                return 
@"
method [ int ; int ; ] input {
    input <? 2 if {
        1 return ;
    } else {
        input - 1 > fac * input return ;      
    } ;
} ;
";
            }
        }
        
        public ICodeElement[] CodeElements
        {
            get
            {
                
                var ifBlockScope = new FinalizedScope(new Dictionary<IKey, IMemberDefinition> {});
                var elseBlock = new FinalizedScope(new Dictionary<IKey, IMemberDefinition> { });

                var inputKey = new NameKey("input");
                var input = new TestMemberDefinition(inputKey,new TestNumberType(),false);

                var facKey = new NameKey("fac");
                var fac = new TestMemberDefinition(facKey, new TestMethodType() , false);


                var methodScope = new FinalizedScope(new Dictionary<IKey, IMemberDefinition> { { inputKey, input } });
                
                var rootScope = new FinalizedScope(new Dictionary<IKey, IMemberDefinition> { { facKey, fac } });

                var method = new TestMethodDefinition(
                            new TestNumberType(),
                            new TestNumberType(),
                            input,
                            methodScope,
                            new ICodeElement[]{
                                new TestElseOperation(
                                    new TestIfOperation(
                                        new TestLessThanOperation(
                                            new TestMemberReferance(input),
                                            new TestConstantNumber(2)),
                                        new TestBlockDefinition(
                                            ifBlockScope,
                                            new ICodeElement[]{
                                                new TestReturnOperation(
                                                    new TestConstantNumber(1))},
                                            new ICodeElement[0])),
                                    new TestBlockDefinition(
                                        elseBlock,
                                        new ICodeElement[]{
                                            new TestReturnOperation(
                                                new TestMultiplyOperation(
                                                    new TestNextCallOperation(
                                                        new TestSubtractOperation(
                                                            new TestMemberReferance(input),
                                                            new TestConstantNumber(1)),
                                                        new TestMemberReferance(input)),
                                                    new TestMemberReferance(input)))},
                                        new ICodeElement[0]))},
                            new ICodeElement[0]);
                
                return new ICodeElement[] {
                    new TestAssignOperation(
                        method,
                        new TestMemberReferance(
                            fac)),
                };
            }
        }
    }
}
