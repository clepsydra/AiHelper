using System.Windows.Input;
using OpenAI.Chat;

namespace AiHelper.Actions
{
    public class ShortSummaryImageAction : ICustomAction
    {
        private readonly Action<string> addToOutput;
        private readonly Func<bool> getShowImageAfterCapture;

        public ShortSummaryImageAction(Action<string> addToOutput, Func<bool> getShowImageAfterCapture)
        {
            this.addToOutput = addToOutput;
            this.getShowImageAfterCapture = getShowImageAfterCapture;
        }

        public Key Key => Key.Space;

        public string KeyText => "Leertaste";

        public string Description => "Bild aufnehmen und KURZ zusammenfassen";

        public string HelpText => "Wenn Du die Leertaste drückst wird über die eingebaute Kamera ein Bild von dem Gegenstand vor der Kamera gemacht und der Inhalt in einem Satz zusammengefasst. Also wenn Du z.B. Post bekommst, oder Dir bei der Medikamentenpackung nicht  sicher bist: Halte es vor die Webcam und drücke die Leertaste.";

        public async Task Run()
        {
            bool showImage = getShowImageAfterCapture();
            var imageData = await ImageCapture.CaptureImageWithHeadsup(showImage);

            var message = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(@"Wenn auf dem Bild jemand einen Gegenstand in die Kamera hält, dann fasse in einem Satz zusammen, worum es sich bei dem Gegenstand handelt.
Es muss dann nicht erwähnt werden, dass es z.B. eine Hand ist, die den Gegenstand hält.

Wenn das Bild ein Dokument zeigt, dann fasse in einem Satz zusammen, um was es sich bei den Dokument handelt.

Wenn keine der Möglichkeiten zutrifft fasse in einem Satz den Inhalt des Bildes zusammen."),
                    ChatMessageContentPart.CreateImagePart(imageData, "image/png"));

            List<ChatMessage> chatHistory = [];
            chatHistory.Add(message);

            var result = await AiAccessor.AskAi(chatHistory);
            addToOutput(result);
            await Speaker.Say(result);
        }
    }
}
