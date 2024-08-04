using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaPage
{
    public int Id { get; set; } // id for my database, need this for different languages
    public int WikipediaId { get; set; } // id from wikipedia api
    public string Language { get; set; }
    public string Title { get; set; }
    public string Extract { get; set; }
    public DateTime LastModified { get; set; }
    public string Url { get; set; }
    public int Length { get; set; }

    // Navigational Property for one-to-many relationship between WikipediaPage and AIResponse
    public IList<AIResponse> AIResponses { get; set; }

    // This gets too complicated trying to keep track of all the incoming and outbound links
    // The api query only returns titles of the links anyway. We can just check if the page exists by title
    // if it doesn't then we can query the page info and use it
    public IList<string> Links { get; set; } = new List<string>();

    // Many-to-many relationship with Categories
    public IList<WikipediaCategory> Categories { get; set; } = new List<WikipediaCategory>();
}