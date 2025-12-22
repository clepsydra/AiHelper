using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiHelper.Actions
{
    internal class VoiceChatListenAction : ICustomAction
    {
        private readonly Func<VoiceChat?> voiceChatAccessor;

        public VoiceChatListenAction(Func<VoiceChat?> voiceChatAccessor)
        {
            this.voiceChatAccessor = voiceChatAccessor;
        }

        public Key Key => Key.Space;

        public string KeyText => "Leertaste";

        public string Description => "Aktiviert den Sprach Chat";

        public string HelpText => "Aktiviert den Sprach Chat";

        public async Task Run()
        {
            var voiceChat = voiceChatAccessor();
            if (voiceChat == null)
            {
                return;
            }

            if (voiceChat.IsDeactivated)
            {
                await voiceChat.Activate();
            }
            else
            {
                voiceChat.Deactivate();
            }
        }
    }
}
