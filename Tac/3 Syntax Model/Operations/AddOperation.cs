﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model.Operations
{

    public class AddOperation : BinaryOperation<ICodeElement,ICodeElement>
    {
        public AddOperation(ICodeElement left, ICodeElement right) : base(left, right)
        {
        }

        public override IBox<ITypeDefinition> ReturnType(RootScope rootScope) {
            if (left.ReturnType(rootScope) == rootScope.NumberType && right.ReturnType(rootScope) == rootScope.NumberType)
            {
                return rootScope.NumberType;
            }
            else if (left.ReturnType(rootScope) == rootScope.StringType || right.ReturnType(rootScope) == rootScope.StringType)
            {
                return rootScope.StringType;
            }
            else
            {
                throw new Exception("add expects string and int");
            }
        }
    }

    public class AddOperationMaker : BinaryOperationMaker<AddOperation>
    {
        public AddOperationMaker(Func<ICodeElement, ICodeElement, AddOperation> make) : base("+", make)
        {
        }
    }
}
