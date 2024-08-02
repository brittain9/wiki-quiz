namespace WikiQuizGenerator.Core;

public static class LanguagesExtensions
{
    private static readonly Dictionary<Languages, string> LanguageCodeMap = new()
    {
        { Languages.English, "en" },
        { Languages.Dutch, "de" },
        { Languages.Spanish, "es" },
        { Languages.Chinese, "zh" },
        { Languages.Japanese, "ja" },
        { Languages.Russian, "ru" },
        { Languages.French, "fr" },
        { Languages.Italian, "it" },
        { Languages.Portuguese, "pt" }
    };

    public static string GetWikipediaLanguageCode(this Languages language)
    {
        if (LanguageCodeMap.TryGetValue(language, out string? code))
        {
            return code;
        }
        throw new ArgumentException("Unsupported language", nameof(language));
    }

    public static Languages GetLanguageFromCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException("Language code cannot be null or empty", nameof(languageCode));
        }

        var pair = LanguageCodeMap.FirstOrDefault(x => x.Value.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

        if (LanguageCodeMap.ContainsKey(pair.Key))
        {
            return pair.Key;
        }

        throw new ArgumentException($"Unsupported language code: {languageCode}", nameof(languageCode));
    }
}