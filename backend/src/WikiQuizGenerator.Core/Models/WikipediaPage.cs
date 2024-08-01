using System.Text;
using System.ComponentModel.DataAnnotations;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaPage
{
    [Key]
    public int Id { get; set; } // pageid, maybe add a different property for this
    public string Langauge {get; set;} // pagelangauge
    public string Title { get; set; } // title
    public string Extract { get; set; } // extract
    public DateTime LastModified { get; set; }  // touched
    public string Url { get; set; } // fullurl
    public int Length { get; set; } // length, seems to be response char count
    public IList<string> Links { get; set; } // links
    public IList<string> Categories { get; set; }

    public WikipediaPage()
    {
        Links = new List<string>();
        Categories = new List<string>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"Wikipedia Article: {Title}");

        sb.AppendLine($"\nURL: {Url}");
        sb.AppendLine($"Last Modified: {LastModified:yyyy-MM-dd HH:mm:ss}");

        sb.AppendLine("\nContent:");
        sb.AppendLine(Extract.Length > 100 ? Extract.Substring(0, 100) + "..." : Extract);
        
        sb.AppendLine("\nRelated Links:");
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