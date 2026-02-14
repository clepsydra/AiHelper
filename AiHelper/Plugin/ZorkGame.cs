using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using zmachine;
using ZorkConsole;

namespace AiHelper.Plugin
{
    /// <summary>
    /// This allows to play the game "Zork" - a text based adventure, see readme for more details
    /// </summary>
    internal class ZorkGame
    {
        private Action<string, bool> addToOutput;

        private bool isCancelled = false;

        public ZorkGame(Action<string, bool> addToOutput, ICancelRegistrar cancelRegistrar)
        {
            this.addToOutput = addToOutput;
            cancelRegistrar.Cancel += CancelRegistrar_Cancel;
        }



        private void CancelRegistrar_Cancel(object? sender, CancelEventArgs e)
        {
            isCancelled = true;
            e.IsHandled = true;
        }

        public void Play()
        {
            Thread.Sleep(8000);

            isCancelled = false;
            var machine = new Machine(Zork1.GetData(), new ZorkInteraction(addToOutput));            

            while(!machine.isFinished() && !isCancelled)
            {
                machine.processInstruction();
            }
        }
    }

    internal class ZorkInteraction : IZmachineInputOutput
    {
        private readonly Action<string, bool> addToOutput;

        private StringBuilder outputBuilder = new ();

        private bool beginReceived = false;

        public ZorkInteraction(Action<string, bool> addToOutput)
        {
            this.addToOutput = addToOutput;
        }

        public string ReadLine()
        {
            string input = VoiceCommandListener.Instance.GetNextVoiceCommand("en", "The user gives an instruction like going somewhere or doing something.").Result;
            addToOutput(input, true);
            if (input.EndsWith(".", StringComparison.OrdinalIgnoreCase) || input.EndsWith("!", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(0, input.Length - 1);
            }
            return input;
        }

        public void Write(string str)
        {
            if (str == ">")
            {
                this.WriteLine(outputBuilder.ToString());
                this.outputBuilder.Clear();
                return;
            }

            outputBuilder.Append(str);

            //addToOutput(str, false);
            //Speaker2.Say(str, true).Wait();
        }

        public void WriteLine(string str)
        {
            addToOutput(str, false);
            if (!beginReceived)
            {
                beginReceived = true;
                var index = str.IndexOf("West of house", StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                {
                    return;
                }

                str = str.Substring(index);
            }            

            Speaker2.Say(str, true).Wait();
        }
    }
}
