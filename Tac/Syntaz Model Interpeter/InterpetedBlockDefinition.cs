﻿using Prototypist.LeftToRight;
using Prototypist.TaskChain.DataTypes;
using System.Collections.Generic;
using Tac.Semantic_Model;
using Tac.Semantic_Model.CodeStuff;
using Tac.Semantic_Model.Names;

namespace Tac.Syntaz_Model_Interpeter
{
    internal class InterpetedBlockDefinition : BlockDefinition, IInterpeted
    {
        public InterpetedBlockDefinition(ICodeElement[] body, IScope scope, IEnumerable<ICodeElement> staticInitailizers) : base(body, scope, staticInitailizers)
        {
        }

        private InterpetedStaticScope StaticStuff { get; } = InterpetedStaticScope.Empty();

        public InterpetedResult Interpet(InterpetedContext interpetedContext)
        {
            var res = InterpetedInstanceScope.Make(StaticStuff, Scope);

            var scope = interpetedContext.Child(res);

            foreach (var line in Body)
            {
                var result = line.Cast<IInterpeted>().Interpet(scope);
                if (result.IsReturn)
                {
                    if (result.HasValue)
                    {
                        return InterpetedResult.Create(result.Get());
                    }
                    else
                    {
                        return InterpetedResult.Create();
                    }
                }
            }

            return InterpetedResult.Create();
        }
    }
}