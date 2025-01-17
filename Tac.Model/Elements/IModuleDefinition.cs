﻿using System.Collections.Generic;

namespace Tac.Model.Elements
{
    public interface IModuleDefinition : ICodeElement
    {
        IKey Key { get; }
        IFinalizedScope Scope { get; }
        IReadOnlyList<ICodeElement> StaticInitialization { get; }
    }

    public interface IProject<TBacking> 
        where TBacking : IBacking
    {
        IModuleDefinition ModuleDefinition { get; }
        IReadOnlyList<IAssembly<TBacking>> References { get; }
    }
}
