﻿using Prototypist.LeftToRight;
using Prototypist.TaskChain.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tac.Semantic_Model.Names;

namespace Tac.Semantic_Model
{
    public interface IScoped
    {
        IResolvableScope Scope { get; }
    }


    public class ScopeTree
    {
        private class Scope : IPopulatableScope, IResolvableScope
            
        {

            protected readonly ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<MemberDefinition>>>> members
        = new ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<MemberDefinition>>>>();

            protected readonly ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<IReturnable>>>> types
                = new ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<IReturnable>>>>();


            protected readonly ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<GenericTypeDefinition>>>> genericTypes = new ConcurrentDictionary<IKey, ConcurrentSet<Visiblity<IBox<GenericTypeDefinition>>>>();

            public bool TryAddMember(DefintionLifetime defintionLifetime, IKey key, IBox<MemberDefinition> definition)
            {
                var list = members.GetOrAdd(key, new ConcurrentSet<Visiblity<IBox<MemberDefinition>>>());
                var visiblity = new Visiblity<IBox<MemberDefinition>>(defintionLifetime, definition);
                return list.TryAdd(visiblity);
            }

            public bool TryAddType(IKey key, IBox<IReturnable> definition)
            {
                var list = types.GetOrAdd(key, new ConcurrentSet<Visiblity<IBox<IReturnable>>>());
                var visiblity = new Visiblity<IBox<IReturnable>>(DefintionLifetime.Static, definition);
                return list.TryAdd(visiblity);
            }

            protected bool TryAddGeneric(DefintionLifetime defintionLifetime, IKey key, IBox<GenericTypeDefinition> definition)
            {
                var list = genericTypes.GetOrAdd(key, new ConcurrentSet<Visiblity<IBox<GenericTypeDefinition>>>());
                var visiblity = new Visiblity<IBox<GenericTypeDefinition>>(defintionLifetime, definition);
                return list.TryAdd(visiblity);
            }

            public bool TryGetMember(IKey name, bool staticOnly, out IBox<MemberDefinition> member)
            {
                if (!members.TryGetValue(name, out var items))
                {
                    member = default;
                    return false;
                }

                var thing = items.SingleOrDefault();

                if (thing == default)
                {
                    member = default;
                    return false;
                }

                member = thing.Definition;
                return true;
            }

            public bool TryGetType(IKey name, out IBox<IReturnable> type)
            {
                if (!types.TryGetValue(name, out var items))
                {
                    type = default;
                    return false;
                }

                var thing = items.SingleOrDefault();

                if (thing == default)
                {
                    type = default;
                    return false;
                }

                type = thing.Definition;
                return true;
            }
            
            public IReadOnlyList<IBox<IReturnable>> Types
            {
                get
                {
                    return types.Select(x => x.Value.Single().Definition).ToArray();
                }
            }

            public IReadOnlyList<IBox<MemberDefinition>> Members
            {
                get
                {
                    return members.Select(x => x.Value.Single().Definition).ToArray();
                }
            }
        }

        private readonly Scope root;

        public ScopeTree()
        {
            root = new Scope();
        }

        /// <summary>
        /// we only point upwards, say you have:
        /// 
        /// object A {
        ///     x : object B{
        ///     
        ///     }
        /// }
        /// 
        /// then:
        /// 
        /// ScopeParent[B] = A;
        /// </summary>
        private Dictionary<Scope, Scope> ScopeParent { get; } = new Dictionary<Scope, Scope>();
        
        private Scope[] Scopes(Scope scope)
        {
            return Inner().ToArray();

            IEnumerable<Scope> Inner()
            {
                yield return scope;
                while (ScopeParent.ContainsKey(scope))
                {
                    scope = ScopeParent[scope];
                    yield return scope;
                }
            }
        }
        
        private ScopeTree Add(Scope oldTop, Scope newTop)
        {
            ScopeParent[newTop] = oldTop;
            return this;
        }
        
        public class ScopeStack: IPopulatableScope, IResolvableScope
        {
            public static ScopeStack Root() {

                var scopeTree = new ScopeTree();
                return new ScopeStack(scopeTree, scopeTree.root);
            }

            public (IPopulatableScope, ScopeStack) ChildScope()
            {
                var res = new Scope();
                ScopeTree.Add(TopScope, res);
                return (res, new ScopeStack(ScopeTree, res));
            }
            
            private ScopeStack(ScopeTree scopeTree, Scope topScope)
            {
                ScopeTree = scopeTree ?? throw new ArgumentNullException(nameof(scopeTree));
                TopScope = topScope ?? throw new ArgumentNullException(nameof(topScope));
            }
            
            public ScopeTree ScopeTree { get; }
            private Scope TopScope { get; }

            public IReadOnlyList<IBox<MemberDefinition>> Members
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IBox<IReturnable> GetType(IKey key)
            {
                foreach (var scope in ScopeTree.Scopes(TopScope))
                {
                    if (scope.TryGetType(key, out var typeDefinition))
                    {
                        return typeDefinition;
                    }
                }
                throw new Exception("");

            }

            public bool TryGetMember(IKey name, bool staticOnly,  out IBox<MemberDefinition> box)
            {
                foreach (var scope in ScopeTree.Scopes(TopScope))
                {
                    if (scope.TryGetMember(name, staticOnly, out var memberDefinition))
                    {
                        box = memberDefinition;
                        return true;
                    }
                }
                box = default;
                return false;
            }

            public bool TryGetType(IKey key, out IBox<IReturnable> result)
            {
                foreach (var scope in ScopeTree.Scopes(TopScope))
                {
                    if (scope.TryGetType(key, out var typeDefinition))
                    {
                        result = typeDefinition;
                        return true;
                    }
                }
                result = default;
                return false;
            }
            
            public IResolvableScope ToResolvable()
            {
                return this;
            }

            public bool TryAddMember(DefintionLifetime lifeTime, IKey name, IBox<MemberDefinition> member)
            {
                return TopScope.TryAddMember(lifeTime, name, member);
            }

            public bool TryAddType(IKey name, IBox<IReturnable> type)
            {
                return TopScope.TryAddType(name, type);
            }

        }
    }
}

