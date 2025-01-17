﻿using System.Collections.Generic;
using Tac.Model;
using Tac.Model.Elements;
using Tac.Model.Instantiated;
using Tac.TestCases;

namespace Tac.Tests.Samples
{
    public class OrTypeSample : ITestCase
    {
        public string Text => @"
module or-test 
{ 
    5 =: ( bool | int ) x ;
    false =: ( bool | int ) y ; 
} ;";

        public IModuleDefinition Module => ModuleDefinition.CreateAndBuild(
             Scope.CreateAndBuild(
                new List<Scope.IsStatic> {
                    new Scope.IsStatic(MemberDefinition.CreateAndBuild(new NameKey("x"), TypeOr.CreateAndBuild(new BooleanType(),new NumberType()), false), false),
                    new Scope.IsStatic(MemberDefinition.CreateAndBuild(new NameKey("y"), TypeOr.CreateAndBuild(new BooleanType(),new NumberType()), false), false)}),
            new[] {
                AssignOperation.CreateAndBuild(
                    ConstantNumber.CreateAndBuild(5),
                    MemberReference.CreateAndBuild(MemberDefinition.CreateAndBuild(new NameKey("x"),TypeOr.CreateAndBuild(new BooleanType(),new NumberType()), false))),
                AssignOperation.CreateAndBuild(
                    ConstantBool.CreateAndBuild(false),
                    MemberReference.CreateAndBuild(MemberDefinition.CreateAndBuild(new NameKey("y"),TypeOr.CreateAndBuild(new BooleanType(),new NumberType()), false)))},
            new NameKey("or-test"));
    }
}
