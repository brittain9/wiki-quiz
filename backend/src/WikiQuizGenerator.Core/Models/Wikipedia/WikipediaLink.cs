using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaLink
{
    [Key]
    public int Id { get; set; }
    public string PageName { get; set; }

    public int? WikipediaPageId { get; set; }

    [ForeignKey("WikipediaPageId")]
    public WikipediaPage? WikipediaPage { get; set; }
}