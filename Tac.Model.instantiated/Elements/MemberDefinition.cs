﻿using System;
using Tac.Model.Elements;

namespace Tac.Model.Instantiated
{
    public class MemberDefinition : IMemberDefinition, IMemberDefinitionBuilder
    {
        private readonly Buildable<IKey> buildableKey = new Buildable<IKey>();
        private readonly Buildable<ITypeReferance> buildableType = new Buildable<ITypeReferance>();
        private readonly BuildableValue<bool> buildableReadOnly = new BuildableValue<bool>();
        
        public IKey Key { get => buildableKey.Get(); }
        public ITypeReferance Type { get => buildableType.Get(); }
        public bool ReadOnly { get => buildableReadOnly.Get(); }
        
        public T Convert<T>(IOpenBoxesContext<T> context)
        {
            return context.MemberDefinition(this);
        }

        public IVerifiableType Returns()
        {
            return this;
        }

        public void Build(IKey key, ITypeReferance type, bool readOnly)
        {
            buildableKey.Set(key);
            buildableType.Set(type);
            buildableReadOnly.Set(readOnly);
        }

        public static (IMemberDefinition, IMemberDefinitionBuilder) Create()
        {
            var res = new MemberDefinition();
            return (res, res);
        }

        public static IMemberDefinition CreateAndBuild(IKey key, ITypeReferance type, bool readOnly)
        {
            var (x, y) = Create();
            y.Build(key, type, readOnly);
            return x;
        }

    }

    public interface IMemberDefinitionBuilder
    {
        void Build(IKey key, ITypeReferance type, bool readOnly);
    }
}