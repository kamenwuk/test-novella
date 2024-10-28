using Naninovel.UI;
using Naninovel;

namespace Assembly.Core
{
    [CommandAlias("checkScore")]
    public sealed class CmdShowCoins : CmdCoins
    {
        public override async UniTask Interact(string identifier, ICustomVariableManager customVariable)
        {
            var service = Engine.GetService<IUIManager>();
            IManagedUI  managedUI = service.GetUI("CoinsUI");
            managedUI.Show();
            await UniTask.Delay(1000);
            managedUI.Hide();
        }
    }
}