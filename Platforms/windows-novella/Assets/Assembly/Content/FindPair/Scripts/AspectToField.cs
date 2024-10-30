using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Assembly.FindPair
{
    public sealed class AspectToField : ProtoAspectInject
    {
        public readonly ProtoIt ItCell = new(It.Inc<DataByCell>());
        public readonly ProtoPool<DataByCell> PoolByCell;
    }
}
