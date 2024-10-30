using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto;
using UnityEngine;
using Naninovel;

namespace Assembly.FindPair
{
    public sealed class GameFieldManager : MonoBehaviour
    {
        [SerializeField] private GenerateFieldInSystem _generateField = null;

        private AspectToField _aspectToField = null;
        private ProtoWorld _world = null;
        private ProtoSystems _systems = null;

        public void Inject()
        {
            _aspectToField = new();
            _world = new(_aspectToField);
            _systems = new(_world);
            _systems
                .AddModule(new AutoInjectModule())
                .AddSystem(_generateField)
                .AddSystem(new CorrectnessChosenPairInSystem())
                .Init();
        }
        private void Update()
        {
            if (_systems == null)
                return;

            _systems.Run();

            if (_aspectToField.PoolByCell.Len() != 0)
                return;

            Destroy();
            Engine.GetService<IScriptPlayer>().PlayTransient($"`{name}` generated script", "@goto ScFinal").Forget();
        }
        public void Destroy()
        {
            _systems?.Destroy();
            _systems = null;
            _world?.Destroy();
            _world = null;
        }
    }
}