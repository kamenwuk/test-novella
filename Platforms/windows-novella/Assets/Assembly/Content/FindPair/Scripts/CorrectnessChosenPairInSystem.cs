using System.Collections.Generic;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto;
using UnityEngine;

namespace Assembly.FindPair
{
    public sealed class CorrectnessChosenPairInSystem : IProtoRunSystem
    {
        [DI] private readonly AspectToField _aspectToField = null;

        public void Run()
        {
            List<ProtoEntity> used = new();
            foreach(ProtoEntity entity in _aspectToField.ItCell)
            {
                DataByCell cell = _aspectToField.PoolByCell.Get(entity);
                if (cell.View.Toggle.isOn)
                {
                    if(used.Contains(entity))
                        used.Remove(entity);
                    continue;
                }
                if (used.Contains(entity))
                    continue;
                
                if (used.Count < 1)
                {
                    used.Add(entity);
                    continue;
                } 
                else if (used.Count > 1)
                {
                    foreach (ProtoEntity entityUsed in used)
                        _aspectToField.PoolByCell.Get(entityUsed).View.Toggle.isOn = true;
                    used.Add(entity);
                    continue;
                }

                DataByCell comparable = _aspectToField.PoolByCell.Get(used[0]);
                if (comparable.Identifier != cell.Identifier)
                {
                    cell.View.Toggle.isOn = true;
                    comparable.View.Toggle.isOn = true;
                    Debug.Log(entity + " " + used[0]);
                    used.Clear();
                    continue;
                }
                comparable.View.Toggle.interactable = false;
                _aspectToField.PoolByCell.Del(used[0]);
                cell.View.Toggle.interactable = false;
                _aspectToField.PoolByCell.Del(entity);
                used.Clear();
            }
        }
    }
}
