// –‒‒–‒––––––‒–‒–‒–‒–‒–‒–‒–‒–‒––‒‒–‒‒‒–‒–‒
// Коммерческая лицензия подписчика
// (c) 2023-2024 Leopotam <leopotam@yandex.ru>
// –‒‒–‒––––––‒–‒–‒–‒–‒–‒–‒–‒–‒––‒‒–‒‒‒–‒–‒

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.QoL {
    public abstract class ProtoAspectInject : IProtoAspect {
        static readonly Type _aspectType = typeof (IProtoAspect);
        static readonly Type _poolType = typeof (IProtoPool);
        static readonly Type _itType = typeof (IProtoIt);

        protected List<IProtoAspect> _aspectAspects;
        protected List<IProtoPool> _aspectPools;
        protected List<IProtoIt> _aspectIts;
        ProtoWorld _world;

        public virtual void Init (ProtoWorld world) {
            world.AddAspect (this);
            _world = world;
            _aspectAspects = new (4);
            _aspectPools = new (8);
            _aspectIts = new (8);
            foreach (var f in GetType ().GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (f.IsStatic) { continue; }
                // аспекты.
                if (_aspectType.IsAssignableFrom (f.FieldType)) {
                    if (world.HasAspect (f.FieldType)) {
                        f.SetValue (this, _world.Aspect (f.FieldType));
                        continue;
                    }
                    var aspect = (IProtoAspect) f.GetValue (this);
                    if (aspect == null) {
#if DEBUG
                        if (f.FieldType.GetConstructor (Type.EmptyTypes) == null) {
                            throw new Exception ($"аспект \"{DebugHelpers.CleanTypeName (f.FieldType)}\" должен иметь конструктор по умолчанию, либо экземпляр должен быть создан заранее");
                        }
#endif
                        aspect = (IProtoAspect) Activator.CreateInstance (f.FieldType);
                        f.SetValue (this, aspect);
                    }
                    _aspectAspects.Add (aspect);
                    aspect.Init (world);
                    continue;
                }
                // пулы.
                if (_poolType.IsAssignableFrom (f.FieldType)) {
                    var pool = (IProtoPool) f.GetValue (this);
                    if (pool == null) {
#if DEBUG
                        if (f.FieldType.GetConstructor (Type.EmptyTypes) == null) {
                            throw new Exception ($"пул \"{DebugHelpers.CleanTypeName (f.FieldType)}\" должен иметь конструктор по умолчанию, либо экземпляр должен быть создан заранее");
                        }
#endif
                        pool = (IProtoPool) Activator.CreateInstance (f.FieldType);
                    }
                    var itemType = pool.ItemType ();
                    if (world.HasPool (itemType)) {
                        pool = world.Pool (itemType);
                    } else {
                        world.AddPool (pool);
                    }
                    _aspectPools.Add (pool);
                    f.SetValue (this, pool);
                    continue;
                }
                // итераторы.
                if (_itType.IsAssignableFrom (f.FieldType)) {
                    var it = (IProtoIt) f.GetValue (this);
#if DEBUG
                    if (it == null) { throw new Exception ($"итератор \"{f.Name}\" в \"{DebugHelpers.CleanTypeName (GetType ())}\" должен быть создан заранее"); }
#endif
                    _aspectIts.Add (it);
                }
            }
        }

        public virtual void PostInit () {
            foreach (var aspect in _aspectAspects) {
                aspect.PostInit ();
            }
            foreach (var it in _aspectIts) {
                it.Init (_world);
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ProtoWorld World () {
            return _world;
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class ProtoWorldExtensions {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void AliveEntities (this ProtoWorld world, Slice<ProtoEntity> result) {
            result.Clear (false);
            var gens = world.EntityGens ();
            for (int i = 0, iMax = gens.Len (); i < iMax; i++) {
                if (gens.Get (i) > 0) {
                    result.Add ((ProtoEntity) i);
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static int AliveEntitiesCount (this ProtoWorld world) {
            return world.EntityGens ().Len () - world.RecycledEntities ().Len ();
        }

        public static void EntityComponents (this ProtoWorld world, ProtoEntity entity, Slice<object> result) {
            result.Clear (false);
            if (world.EntityGen (entity) < 0) { return; }
            var pools = world.Pools ();
            var maskData = world.EntityMasks ().Data ();
            var maskLen = world.EntityMaskItemLen ();
            var maskOffset = world.EntityMaskOffset (entity);
            for (int i = 0, offset = 0; i < maskLen; i++, offset += 64, maskOffset++) {
                var v = maskData[maskOffset];
                for (var j = 0; v != 0 && j < 64; j++) {
                    var mask = 1UL << j;
                    if ((v & mask) != 0) {
                        v &= ~mask;
                        result.Add (pools.Get (offset + j).Raw (entity));
                    }
                }
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static T Aspect<T> (this ProtoWorld world) where T : IProtoAspect {
            return (T) world.Aspect (typeof (T));
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static IProtoPool Pool<T> (this ProtoWorld world) where T : struct {
            return world.Pool (typeof (T));
        }
    }
}
