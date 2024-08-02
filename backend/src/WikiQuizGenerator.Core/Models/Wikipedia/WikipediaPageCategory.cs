using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.Models;

public class WikipediaPageCategory
{
    // Junction table
    public int WikipediaPageId { get; set; }
    public int WikipediaCategoryId { get; set; }

    [ForeignKey("WikipediaPageId")]
    public WikipediaPage WikipediaPage { get; set; }

    [ForeignKey("WikipediaCategoryId")]
    public WikipediaCategory WikipediaCategory { get; set; }
}
