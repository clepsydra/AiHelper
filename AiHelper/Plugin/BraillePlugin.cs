using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace AiHelper.Plugin
{
    public class BraillePlugin
    {
        [KernelFunction]
        [Description(@"Gets the Braille points for the character in the input.
Whenever there is a request to get the points for a braille character use this function.
Paramters:
- input: The braille character
Result:
An array of integer values.")]
        //// Whenever there is the need to verify the information that the user gives to you about braille points use this function to get the points to use this information for verification.
        //// When the user is wrong you must tell him that he is wrong and tell him the correct points for the current braille character.
        public IReadOnlyList<int> GetBraillePoints(string input)
        {
            Debug.WriteLine($"GetBraillePoints: {input}");
            switch (input.ToLowerInvariant())
            {
                case "a":
                    return [1];
                case "b":
                    return [1, 2];
                case "c":
                    return [1, 4];
                case "d":
                    return [1, 4, 5];
                case "e":
                    return [1, 5];
                case "f":
                    return [1, 2, 4];
                case "g":
                    return [1, 2, 4, 5];
                case "h":
                    return [1, 2, 5];
                case "i":
                    return [2, 4];
                case "j":
                    return [2, 4, 5];
                case "k":
                    return [1, 3];
                case "l":
                    return [1, 2, 3];
                case "m":
                    return [1, 3, 4];
                case "n":
                    return [1, 3, 4, 5];
                case "o":
                    return [1, 3, 5];
                case "p":
                    return [1, 2, 3, 4];
                case "q":
                    return [1, 2, 3, 4, 5];
                case "r":
                    return [1, 2, 3, 5];
                case "s":
                    return [2, 3, 4];
                case "t":
                    return [2, 3, 4, 5];
                case "u":
                    return [1, 3, 6];
                case "v":
                    return [1, 2, 3, 6];
                case "w":
                    return [2, 4, 5, 6];
                case "x":
                    return [1, 3, 4, 6];
                case "y":
                    return [1, 3, 4, 5, 6];
                case "z":
                    return [1, 3, 5, 6];
            }

            throw new Exception($"Unsupported input: {input}");
        }


        [KernelFunction]
        [Description(@"Verifies whether a given list of braille point numbers matches a given braille character.
Example: First parameter is a 'C', List of point numbers is 1,4. Result would be true.
Parameters:
- a braille character, type: string
- point number list: an array of integers")]
        public bool VerifyBraillePoints(string brailleCharacter, List<int> braillePointNumbers)
        {
            Debug.WriteLine($"VerifyBraillePoints: {brailleCharacter}, braillePointNumbers: {string.Join(", ", braillePointNumbers)}");

            var realBraillePoints = GetBraillePoints(brailleCharacter);

            if (braillePointNumbers.Count != realBraillePoints.Count)
            {
                return false;
            }

            var sortedInput = braillePointNumbers.OrderBy(i => i).ToList();
            for (int i = 0; i < realBraillePoints.Count; i++)
            {
                if (sortedInput[i] != realBraillePoints[i])
                {
                    return false;
                }
            }

            return true;
        }

        [KernelFunction]
        [Description(@"Gets the locations of the braille dots, i.e. location of point 1 upper left etc.
Call this function to get the correct information of where the braille dots are located in the 2x3 matrix.")]
        public Dictionary<int, string> GetLocationOfBraillePoints()
        {
            return new()
            {
                { 1, "upper left" },
                { 2, "middle left"},
                { 3, "bottom left"},
                { 4, "upper right" },
                { 5, "middle right"},
                { 6, "bottom right"}
            };
        }
    }
}
