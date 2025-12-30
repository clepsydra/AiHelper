using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiHelper
{
    internal class BrailleProvider
    {
        public static IReadOnlyList<int> GetBraillePoints(string input)
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
    }
}
