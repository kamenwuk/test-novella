using Naninovel;

namespace Assembly.Core
{
    [CommandAlias("addScore")]
    public sealed class CmdAddCoins : CmdCoins
    {
        [RequiredParameter]
        [ParameterAlias(NamelessParameterAlias)]
        public IntegerParameter Parameter;

        public override async UniTask Interact(string identifier, ICustomVariableManager customVariable)
        {
            int value = int.Parse(customVariable.GetVariableValue(identifier)) + Parameter.Value;
            customVariable.SetVariableValue(identifier, value.ToString());
            await UniTask.Yield();
        }
    }
}
