using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using OpenAI.Chat;

namespace AiHelper.Plugin
{
    public class AnalyzeImagePlugin
    {
        [KernelFunction]
        [Description(@"Takes a picture using the built in camera and gives the user a short summary of what cam be seen in the picture.
When the user e.g. asks what he is holding in his hand you can assume that he is holding it in the direction of the built in camera.
You can then activate this function.
Result: The summary of the contents of the image")]
        public async Task<string> SummarizeCameraImage()
        {
            var imageData = await ImageCapture.CaptureImageWithHeadsup(false);

            var message = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(@"Wenn auf dem Bild jemand einen Gegenstand in die Kamera hält, dann fasse in einem Satz zusammen, worum es sich bei dem Gegenstand handelt.
Es muss dann nicht erwähnt werden, dass es z.B. eine Hand ist, die den Gegenstand hält.

Wenn das Bild ein Dokument zeigt, dann fasse in einem Satz zusammen, um was es sich bei den Dokument handelt.

Wenn keine der Möglichkeiten zutrifft fasse in einem Satz den Inhalt des Bildes zusammen."),
                    ChatMessageContentPart.CreateImagePart(imageData, "image/png"));

            List<ChatMessage> chatHistory = [];
            chatHistory.Add(message);

            var result = await AiAccessor.AskAi(chatHistory);
            return result;
        }

        [KernelFunction]
        [Description(@"Takes a picture using the built in camera and gives the user much more detailed information of what cam be seen in the picture.
When the user e.g. asks what he is holding in his hand you can assume that he is holding it in the direction of the built in camera.
You can then activate this function.
Result: The details of the contents of the image")]
        public async Task<string> AnalyzeDetailsOfCameraImage()
        {
            var imageData = await ImageCapture.CaptureImageWithHeadsup(false);

            var message = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(@"Wenn auf dem Bild jemand einen Gegenstand in die Kamera hält, dann erkäre möglichst genau, worum es sich bei dem Gegenstand handelt.
Es muss dann nicht erwähnt werden, dass es z.B. eine Hand ist, die den Gegenstand hält.

Wenn das Bild ein Dokument zeigt, dann lies den Inhalt des Dokuments vor.

Wenn keine der Möglichkeiten zutrifft erläutere den Inhalt des Bildes im Detail."),
                    ChatMessageContentPart.CreateImagePart(imageData, "image/png"));

            List<ChatMessage> chatHistory = [];
            chatHistory.Add(message);

            var result = await AiAccessor.AskAi(chatHistory);
            return result;
        }
    }
}
