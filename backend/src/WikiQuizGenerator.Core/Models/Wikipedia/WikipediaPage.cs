using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaPage
{
    [Key]
    public int Id { get; set; }
    public string Language { get; set; }
    public string Title { get; set; }
    public string Extract { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime LastModified { get; set; }

    public string Url { get; set; }
    public int Length { get; set; }

    public IList<WikipediaLink> Links { get; set; }
    public IList<WikipediaCategory> Categories { get; set; }


    public WikipediaPage()
    {
        Links = new List<WikipediaLink>();
        Categories = new List<WikipediaCategory>();
    }
}