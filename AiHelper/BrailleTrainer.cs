using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Responses;
using Org.BouncyCastle.Ocsp;

namespace AiHelper
{
    internal class BrailleTrainer
    {
        private readonly Action sessionEnded;
        private readonly VoiceCommandListener voiceCommandListener = new();

        public BrailleTrainer(Action sessionEnded)
        {
            this.sessionEnded = sessionEnded;
        }

        public async Task StartTraining()
        {
            Debug.WriteLine("StartTraining started");

            await Speaker2.SayAndCache(@"Willkommen zum Braille Trainer.", true);
            await TrainingByNumbers();

            await Speaker2.SayAndCache(@"Bis zum nächsten Mal beim Braille Trainer.", true);
        }

        private async Task TrainingByNumbers()
        {
            await Speaker2.SayAndCache("Welche Buchstaben möchtest Du üben?", true);

            string input = await GetVoiceInput("Der Benutzer nennt einen Buchstabenbereich.");
            //string input = "Ich würde gerne A bis E üben";
            //string input = "Ich möchte jetzt doch nicht üben";

            string analysisInstructions = @$"
Der Benutzer wurde gefragt, welchen Buchstabenbereich er auswählen möchte.
Entweder sagt er so was wie ""von A bis F"" oder er sagt etwas wie ""Alle"".
Du analysierst die Eingabe des Benutzers und gibst den Buchstabenbereich zurück, für die sich der Benutzer entschieden hat.
Die Ausgabe sind im Falle eines Bereichs die beiden Buchstaben für Anfang und Ende mit Komma getrennt, also z.B ""A,F"",
Wenn er sich für alle Buchstaben entschieden hat gibst Du ""A,Z"" zurück.
Wenn der Benutzer etwas gesagt hat, dass bedeutet er möchte das Programm verlassen, dann ist Deine Antwort ""Exit"".

Eingabe des Benutzers ist: ""{input}""";

            string parsedResult = await AiAccessor.AskAi(analysisInstructions);

            Debug.WriteLine($"parsedResult = {parsedResult}");

            if (parsedResult == "Exit")
            {
                return;
            }

            string[] split = parsedResult.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
            {
                await Speaker2.SayAndCache("Ich habe die Eingabe nicht verstanden.", true);
                return;
            }

            var from = split[0][0];
            var to = split[1][0];

            Random random = new();

            bool isFirst = true;

            char lastCorrectChar = '\0';
            char currentChar = 'A';

            var unknowns = new List<char>();

            while (true)
            {
                if (unknowns.Count > 0 && random.Next(3) > 0)
                {
                    var index = random.Next(unknowns.Count);
                    currentChar = unknowns[index];
                }
                else
                {
                    currentChar = (char)random.Next(from, to + 1);

                    while (currentChar == lastCorrectChar)
                    {
                        currentChar = (char)random.Next(from, to + 1);
                    }
                }

                var points = BrailleProvider.GetBraillePoints(currentChar.ToString());
                string firstText = isFirst ? string.Empty : "Nächste Frage: ";
                await Speaker2.SayAndCache($"{GetPointsTextWithPositions(points)}. Welcher Buchstabe ist das?", true);

                string charInput = await GetVoiceInput("Der Benutzer nennt einen Buchstaben.");

                if (charInput.Length != 1)
                {
                    analysisInstructions = @$"
Der Benutzer wurde nach einem Buchstaben gefragt.
Die Antwort des Benutzers enthält vermutlich den genannten Buchstaben.
Wenn der Buchstabe eindeutig in der Antwort zu identifizieren ist, gibst Du genau diesen Buchstaben zurück, also z.B. ""G"".
Wenn der Benutzer etwas gesagt hat, dass bedeutet er möchte das Programm verlassen, dann ist Deine Antwort ""Exit"".

Eingabe des Benutzers ist: ""{charInput}""";

                    parsedResult = await AiAccessor.AskAi(analysisInstructions);
                    Debug.WriteLine($"parsedResult  {parsedResult}");
                    if (parsedResult == "Exit")
                    {
                        return;
                    }

                    if (parsedResult.Length != 1)
                    {
                        await Speaker2.SayAndCache(@$"Ich habe die Eingabe nicht verstanden.
Die richtige Antwort für {GetPointsTextWithPositions(points)} ist ein {currentChar}.", true);
                        lastCorrectChar = '@';
                        continue;
                    }

                    charInput = parsedResult;
                }

                if (charInput.Equals(currentChar.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    await Speaker2.SayAndCache(GetThatsCorrectVariation(), true);
                    lastCorrectChar = currentChar;
                    unknowns.Remove(currentChar);

                    await Task.Delay(500);
                    continue;
                }

                string ergibtOrErgeben = points.Count == 1 ? "ergibt" : "ergeben";

                await Speaker2.SayAndCache($"{GetThatsIncorrectVariation()}. {GetPointsText(points)} {ergibtOrErgeben} den Buchstaben {currentChar}.", true);
                lastCorrectChar = '@';
                if (!unknowns.Contains(currentChar))
                {
                    unknowns.Add(currentChar);
                }

                await Task.Delay(500);
            }
        }

        private async Task<string> GetVoiceInput(string prompt)
        {
            string input = await voiceCommandListener.GetNextVoiceCommand("de", prompt);
            return input;
        }

        private string GetPointsText(IReadOnlyList<int> points)
        {
            if (points.Count == 0)
            {
                return "Keine Punkte";
            }

            if (points.Count == 1)
            {
                return $"Der Punkt {points[0]}";
            }

            var pointsExceptLast = points.Take(points.Count - 1);

            return $"Die Punkte {string.Join(", ", pointsExceptLast)} und {points.Last()}";
        }

        private string GetPointsTextWithPositions(IReadOnlyList<int> points)
        {
            string text = GetPointsText(points);
            if (points.Count == 0)
            {
                return text;
            }

            text += ", also ";

            var pointsLeft = points.Where(p => p <= 3).ToList();
            var pointsRight = points.Where(p => p >= 4).ToList();

            if (pointsLeft.Count > 0)
            {
                text += "linke Seite " + NumbersToVerticalLocations(pointsLeft);
                if (pointsRight.Count > 0)
                {
                    text += " und ";
                }
            }

            if (pointsRight.Count > 0)
            {
                text += "rechte Seite " + NumbersToVerticalLocations(pointsRight);
            }

            return text;
        }

        private string NumberToVerticalLocation(int number)
        {
            switch (number)
            {
                case 1:
                    return "oben";
                case 2:
                    return "mitte";
                case 3:
                    return "unten";
                case 4:
                    return "oben";
                case 5:
                    return "mitte";
                case 6:
                    return "unten";
            }

            throw new Exception($"Unknown input: {number}");
        }

        private string NumbersToVerticalLocations(IReadOnlyList<int> numbers)
        {
            var strings = numbers.Select(NumberToVerticalLocation).ToList();

            if (strings.Count == 1)
            {
                return strings[0];
            }

            var stringsExceptLast = strings.Take(strings.Count - 1);

            return $"{string.Join(", ", stringsExceptLast)} und {strings.Last()}";
        }

        private string GetThatsCorrectVariation()
        {
            switch (new Random().Next(5))
            {
                case 0:
                    return "Das ist richtig.";
                case 1:
                    return "Das ist korrekt.";
                case 2:
                    return "Ja genau.";
                case 3:
                    return "Perfekt.";
                case 4:
                    return "Sehr gut.";
            }

            return "Das ist richtig.";
        }

        private string GetThatsIncorrectVariation()
        {
            switch (new Random().Next(4))
            {
                case 0:
                    return "Das ist nicht richtig.";
                case 1:
                    return "Das ist leider nicht korrekt.";
                case 2:
                    return "Knapp daneben.";
                case 3:
                    return "Das ist falsch.";
            }

            return "Das ist nicht richtig.";
        }
    }
}
