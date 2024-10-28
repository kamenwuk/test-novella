// –‒‒–‒––––––‒–‒–‒–‒–‒–‒–‒–‒–‒––‒‒–‒‒‒–‒–‒
// Коммерческая лицензия подписчика
// (c) 2024 Leopotam <leopotam@yandex.ru>
// –‒‒–‒––––––‒–‒–‒–‒–‒–‒–‒–‒–‒––‒‒–‒‒‒–‒–‒

using System.Runtime.CompilerServices;
using Leopotam.EcsProto.QoL;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.Parenting {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class ParentingAspect : ProtoAspectInject {
        readonly ProtoPool<ParentingParent> _parents = default;
        readonly ProtoPool<ParentingChild> _children = default;

        public void SetParent (ProtoEntity childE, ProtoEntity parentE) {
            var w = World ();
            var packedParentE = w.PackEntity (parentE);
            ref var child = ref _children.GetOrAdd (childE);
            if (child.Parent == packedParentE) {
                return;
            }
            var packedChildE = w.PackEntity (childE);
            if (child.Parent.TryUnpack (w, out var oldParentE)) {
                // очистка данных старого родителя.
                var children = _parents.Get (oldParentE).Children;
                for (var i = children.Len () - 1; i >= 0; i--) {
                    if (children.Get (i) == packedChildE) {
                        children.RemoveAt (i);
                        break;
                    }
                }
            }
            child.Parent = packedParentE;
            ref var parent = ref _parents.GetOrAdd (parentE);
            parent.ChildrenPool = _children;
            parent.Children.Add (packedChildE);
        }

        public void ClearParent (ProtoEntity childE) {
            if (_children.Has (childE)) {
                if (_children.Get (childE).Parent.TryUnpack (World (), out var parentE)) {
                    var children = _parents.Get (parentE).Children;
                    var packedChildE = World ().PackEntity (childE);
                    for (var i = children.Len () - 1; i >= 0; i--) {
                        if (children.Get (i) == packedChildE) {
                            children.RemoveAt (i);
                            break;
                        }
                    }
                }
                _children.Del (childE);
            }
        }

#if DEBUG
        [System.Obsolete ("Следует использовать KeepChildrenAfterParentDeath()")]
#endif
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void KeepChildrenOnDead (ProtoEntity parentE, bool state) {
            KeepChildrenAfterParentDeath (parentE, state);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void KeepChildrenAfterParentDeath (ProtoEntity parentE, bool state) {
            _parents.GetOrAdd (parentE).KeepChildrenAfterParentDeath = state;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsParented (ProtoEntity childE) {
            return _children.Has (childE);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public (ProtoEntity entity, bool ok) Parent (ProtoEntity childE) {
            if (_children.Has (childE)) {
                if (_children.Get (childE).Parent.TryUnpack (World (), out var parentE)) {
                    return (parentE, true);
                }
            }
            return (default, false);
        }

        public void Children (ProtoEntity parent, Slice<ProtoEntity> result) {
            result.Clear (false);
            if (!_parents.Has (parent)) {
                return;
            }
            var children = _parents.Get (parent).Children;
            var w = World ();
            for (var i = children.Len () - 1; i >= 0; i--) {
                if (children.Get (i).TryUnpack (w, out var childE)) {
                    result.Add (childE);
                } else {
                    children.RemoveAt (i);
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsSameParent (ProtoEntity child1, ProtoEntity child2) {
            if (!_children.Has (child1) || !_children.Has (child2)) {
                return false;
            }
            var p = _children.Get (child1).Parent;
            return p == _children.Get (child2).Parent && p.TryUnpack (World (), out _);
        }
    }
}
