using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace WikiQuizGenerator.Core;

public static class Utility
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string[] words = input.Split(' ');

        // Process each word
        for (int i = 0; i < words.Length; i++)
        {
            if (!string.IsNullOrEmpty(words[i]))
            {
                // Capitalize the first letter and lowercase the rest
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }

        return string.Join(" ", words);
    }

        /// <summary>
    /// Removes HTML formatting and cleans up the input string.
    /// </summary>
    /// <param name="input">The string to be cleaned.</param>
    /// <returns>A cleaned string without HTML tags and extra whitespace.</returns>
    public static string RemoveFormatting(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove all HTML tags
        input = Regex.Replace(input, @"<[^>]+>", string.Empty);

        // Remove extra whitespace
        input = Regex.Replace(input, @"\s+", " ");

        // Decode HTML entities
        input = System.Net.WebUtility.HtmlDecode(input);

        return input.Trim();   
    }
}

