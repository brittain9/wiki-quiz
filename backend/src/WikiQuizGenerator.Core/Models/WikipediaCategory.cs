using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaCategory
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Many-to-many relationship with WikipediaPage
    public IList<WikipediaPage> WikipediaPages { get; set; }
}
