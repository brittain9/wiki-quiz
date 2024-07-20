using System;
using System.Collections.Generic;
using System.Text;

namespace WikiQuizGenerator.Core.Models;

// Stores the info from specific query in GetWikipediaPage
public class WikipediaPage
{
    public int Id { get; set; } // pageid
    public string Title { get; set; } // title
    public string Extract { get; set; } // extract
    public DateTime LastModified { get; set; }  // touched
    public string Url { get; set; } // fullurl
    public int Length { get; set; } // length, seems to be response char count
    public List<string> Links { get; set; } // links

    public WikipediaPage()
    {
        Links = new List<string>();
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
        sb.AppendLine(Extract.Length > 100 ? Extract.Substring(0, 100) + "..." : Extract);

        sb.AppendLine("\nCategories:");
        sb.AppendLine(new string('-', 11)); // Underline "Categories:"

        sb.AppendLine("\nRelated Links:");
        sb.AppendLine(new string('-', 14)); // Underline "Related Links:"
        foreach (var link in Links.Take(10)) // Limit to first 10 related links
        {
            sb.AppendLine($"- {link}");
        }
        if (Links.Count > 10)
        {
            sb.AppendLine($"... and {Links.Count - 10} more");
        }

        return sb.ToString();
    }
}