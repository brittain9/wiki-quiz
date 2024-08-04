using WikiQuizGenerator.Core;

public static class LanguagesExtensions
{
    private static readonly Dictionary<Languages, string> LanguageCodeMap = new()
    {
        { Languages.English, "en" },
        { Languages.Dutch, "de" },  // Note: Changed from "de" (German) to "nl" (Dutch)
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
        throw new LanguageException($"Unsupported language: {language}", language.ToString());
    }

    public static Languages GetLanguageFromCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new LanguageException("Language code cannot be null or empty", languageCode ?? "");
        }

        var pair = LanguageCodeMap.FirstOrDefault(x => x.Value.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

        if (pair.Key != default(Languages) || (pair.Key == Languages.English && pair.Value == "en"))
        {
            return pair.Key;
        }

        throw new LanguageException($"Unsupported language code: {languageCode}", languageCode);
    }
}

public class LanguageException : Exception
{
    public LanguageException() : base() { }

    public LanguageException(string message) : base(message) { }

    public LanguageException(string message, Exception innerException) : base(message, innerException) { }

    public LanguageException(string message, string languageCode) : base(message)
    {
        LanguageCode = languageCode;
    }

    public string? LanguageCode { get; }
}
