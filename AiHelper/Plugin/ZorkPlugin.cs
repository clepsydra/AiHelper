using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    /// <summary>
    /// This allows to play the game "Zork" - a text based adventure, see readme for more details
    /// </summary>
    public class ZorkPlugin
    {
        private readonly Action<string, bool> addToOutput;
        private readonly Action closeSession;
        private readonly ICancelRegistrar cancelRegistrar;

        public ZorkPlugin(Action<string, bool> addToOutput, Action closeSession, ICancelRegistrar cancelRegistrar)
        {
            this.addToOutput = addToOutput;
            this.closeSession = closeSession;
            this.cancelRegistrar = cancelRegistrar;
        }

        [KernelFunction]
        [Description(@"Starts the game of Zork. That tool does then do the interaction with the user.
The function returns an information whether the Zork game was started successfully, or if there is a problem.
Tell the user about the answer.
After calling this function you MUST not ask the user whether he wants to do something else.")]
        public string StartZork()
        {
            closeSession();
            var game = new ZorkGame(addToOutput, cancelRegistrar);            
            Task.Run(game.Play);

            return "successfully started";
        }
    }
}
