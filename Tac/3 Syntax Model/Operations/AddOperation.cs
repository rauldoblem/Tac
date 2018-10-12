﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac._3_Syntax_Model.Elements.Atomic_Types;
using Tac.Parser;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model.Operations
{

    public class AddOperation : BinaryOperation<ICodeElement,ICodeElement>
    {
        public AddOperation(ICodeElement left, ICodeElement right) : base(left, right)
        {
        }

        public override IReturnable ReturnType(IElementBuilders elementBuilders) {
            if (left.ReturnType(elementBuilders) is NumberType && right.ReturnType(elementBuilders) is NumberType)
            {
                return elementBuilders.NumberType();
            }
            else if (left.ReturnType(elementBuilders) is StringType || right.ReturnType(elementBuilders) is StringType)
            {
                return elementBuilders.StringType();
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