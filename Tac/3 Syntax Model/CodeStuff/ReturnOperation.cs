﻿using System;
using System.Collections.Generic;
using System.Text;
using Tac.Semantic_Model.CodeStuff;

namespace Tac.Semantic_Model.Operations
{
    public class ReturnOperation : ICodeElement
    {
        public ReturnOperation(ICodeElement result)
        {
            Result = result;
        }

        public ICodeElement Result { get; }
    }

    public class VarOperation : ICodeElement
    {
        public VarOperation(ICodeElement varDef)
        {
            VarDef = varDef;
        }

        public ICodeElement VarDef { get; }
    }

    public class Constant : ICodeElement {
        public Constant(string value) {
            Value = value;
        }

        public string Value { get; }
    }
}
