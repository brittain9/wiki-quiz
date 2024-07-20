using System;
using System.Collections.Generic;
using System.Text;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaArticle
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Url { get; set; }
    public DateTime LastModified { get; set; }
    public List<string> Categories { get; set; }
    public List<string> RelatedLinks { get; set; }

    public WikipediaArticle()
    {
        Categories = new List<string>();
        RelatedLinks = new List<string>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Wikipedia Article: {Title}");
        sb.AppendLine(new string('=', Title.Length + 19)); // Underline the title

        sb.AppendLine($"\nURL: {Url}");
        sb.AppendLine($"Last Modified: {LastModified:yyyy-MM-dd HH:mm:ss}");

        sb.AppendLine("\nContent:");
        sb.AppendLine(new string('-', 8)); // Underline "Content:"
        sb.AppendLine(Content.Length > 500 ? Content.Substring(0, 500) + "..." : Content);

        sb.AppendLine("\nCategories:");
        sb.AppendLine(new string('-', 11)); // Underline "Categories:"
        foreach (var category in Categories)
        {
            sb.AppendLine($"- {category}");
        }

        sb.AppendLine("\nRelated Links:");
        sb.AppendLine(new string('-', 14)); // Underline "Related Links:"
        foreach (var link in RelatedLinks.Take(10)) // Limit to first 10 related links
        {
            sb.AppendLine($"- {link}");
        }
        if (RelatedLinks.Count > 10)
        {
            sb.AppendLine($"... and {RelatedLinks.Count - 10} more");
        }

        return sb.ToString();
    }
}