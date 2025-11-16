using OpenAI.Chat;
using System.Windows.Input;

namespace AiHelper.Actions
{
    public class ReadImageAction : ICustomAction
    {
        private readonly Action<string> addToOutput;
        private readonly Func<bool> getShowImageAfterCapture;

        public ReadImageAction(Action<string> addToOutput, Func<bool> getShowImageAfterCapture)
        {
            this.addToOutput = addToOutput;
            this.getShowImageAfterCapture = getShowImageAfterCapture;
        }

        public Key Key => Key.F2;

        public string KeyText => "F2";

        public string Description => "Bild aufnehmen und Inhalt erklären";

        public string HelpText => "Wenn Du F2 drückst wird das Gleiche gemacht wit bei Leertaste, aber das Bild wird ausführlicher mehr erläutert.";

        public async Task Run()
        {
            bool showImage = getShowImageAfterCapture();
            var imageData = await ImageCapture.CaptureImageWithHeadsup(showImage);

            var message = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(@"Wenn auf dem Bild jemand einen Gegenstand in die Kamera hält, dann erkäre, worum es sich bei dem Gegenstand handelt.
Es muss dann nicht erwähnt werden, dass es z.B. eine Hand ist, die den Gegenstand hält.

Wenn das Bild ein Dokument zeigt, dann lies den Inhalt des Dokuments vor.

Wenn keine der Möglichkeiten zutrifft erläutere den Inhalt des Bildes."),
                    ChatMessageContentPart.CreateImagePart(imageData, "image/png"));

            List<ChatMessage> chatHistory = [];
            chatHistory.Add(message);

            var result = await AiAccessor.AskAi(chatHistory);
            addToOutput(result);
            await Speaker.Say(result);
        }
    }
}
