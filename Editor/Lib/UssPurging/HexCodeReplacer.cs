using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


namespace ZoboUI.Editor
{

    /// <summary>
    /// The Css parser we use automatically converts hex colors to rgb colors. This class helps us replace the rgb colors with hex colors in the original uss file
    /// </summary>
    public class HexCodeReplacer
    {

        public string ReplaceRgbWithHex(string parsedContent, string originalContent)
        {

            // Extract all rgb() values from parsed content
            MatchCollection rgbMatches = Regex.Matches(parsedContent, @"rgb\((.*?)\)");

            foreach (Match match in rgbMatches)
            {
                string rgbValue = match.Value;
                string hexEquivalent = FindHexEquivalentInOriginal(rgbValue, originalContent);

                if (!string.IsNullOrEmpty(hexEquivalent))
                {
                    // Replace rgb() with hex code in parsed content
                    parsedContent = parsedContent.Replace(rgbValue, hexEquivalent);
                }
            }

            return parsedContent;
        }

        private string FindHexEquivalentInOriginal(string rgbValue, string originalContent)
        {
            // Convert rgb to hex format
            string hexEquivalent = RgbToHex(rgbValue);
            string pattern = "#" + hexEquivalent;

            // Check if hex code exists in the original content
            if (originalContent.Contains(pattern))
            {
                return pattern;
            }

            return null;
        }

        private string RgbToHex(string rgbValue)
        {
            // Extract individual RGB components
            var matches = Regex.Matches(rgbValue, @"\d+");
            if (matches.Count == 3)
            {
                int r = int.Parse(matches[0].Value);
                int g = int.Parse(matches[1].Value);
                int b = int.Parse(matches[2].Value);

                // Convert to hex format
                return r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            }

            return null;
        }
    }
}