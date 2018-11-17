﻿using System.Collections.Generic;

namespace Tac.Model.Elements
{
    public interface IModuleDefinition : ICodeElement, IVarifiableType
    {
        IFinalizedScope Scope { get; }
        IEnumerable<ICodeElement> StaticInitialization { get; }
    }
}