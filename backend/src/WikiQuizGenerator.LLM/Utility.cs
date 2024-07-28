using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WikiQuizGenerator.LLM;

internal static class Utility
{
    public static string CleanJsonString(string input)
    {
        // Remove code fences
        var withoutCodeFences = Regex.Replace(input, @"^\s*```(?:json)?\s*|\s*```\s*$", "", RegexOptions.Multiline);

        // Remove extra brackets at the start and end
        var withoutExtraBrackets = Regex.Replace(withoutCodeFences, @"^\s*\[?\s*|\s*\]?\s*$", "");

        // Trim whitespace and ensure the string is wrapped in brackets
        var trimmed = withoutExtraBrackets.Trim();
        if (!trimmed.StartsWith("["))
            trimmed = "[" + trimmed;
        if (!trimmed.EndsWith("]"))
            trimmed = trimmed + "]";

        return trimmed;
    }
}

