using Naninovel;

namespace Assembly.Core
{
    public abstract class CmdCoins : Command
    {
        public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
        {
            const string identifier = "coins";
            var customVariable = Engine.GetService<ICustomVariableManager>();

            if (customVariable.VariableExists(identifier) == false)
                customVariable.SetVariableValue(identifier, "0");
            
            await Interact(identifier, customVariable);
        }
        public abstract UniTask Interact(string identifier, ICustomVariableManager customVariable);
    }
}
