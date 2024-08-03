using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaLink
{
    [Key]
    public int Id { get; set; }
    public string PageName { get; set; }

    public int? WikipediaPageId { get; set; }

    [JsonIgnore]
    [ForeignKey("WikipediaPageId")]
    public WikipediaPage? WikipediaPage { get; set; }
}