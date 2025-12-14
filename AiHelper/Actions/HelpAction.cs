using System.Windows.Input;

namespace AiHelper.Actions
{
    internal class HelpAction : ICustomAction
    {
        private readonly IReadOnlyList<ICustomAction> actions;

        public HelpAction(IReadOnlyList<ICustomAction> actions)
        {
            this.actions = actions;
        }

        public Key Key => Key.F1;

        public string KeyText => "F1";

        public string Description => "Hilfe - Die verfügbaren Optionen werden erläutert.";

        public string HelpText => "Wenn Du F1 drückst werden die verfügbaren Optionen erläutert";


        public async Task Run()
        {
            foreach (var action in actions)
            {
                Speaker2.Say(action.HelpText);
            }
        }
    }
}
