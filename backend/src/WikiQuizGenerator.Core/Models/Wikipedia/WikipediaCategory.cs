using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaCategory
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    public IList<WikipediaPageCategory> PageCategories { get; set; }
}
