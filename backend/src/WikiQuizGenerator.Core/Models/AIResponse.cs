using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WikiQuizGenerator.Core.Models;

public class AIResponse
{
    [Key]
    public int Id { get; set; }

    public IList<Question> Questions { get; set; }

    public AIMetadata AIMetadata { get; set; } // This allows to change the metadata without affecting the response

    public int WikipediaPageId { get; set; }
    [ForeignKey("WikipediaPageId")]
    public WikipediaPage WikipediaPage { get; set; }
}